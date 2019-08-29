// @ts-check
"use strict";

var EOL = require("os").EOL;
var fs = require("fs");
var path = require("path");

var defaultJasmineOptions = {};

function logError(...args) {
    var errorArgs = Array.prototype.slice.call(arguments);
    errorArgs.unshift("NTVS_ERROR:");
    console.error.apply(console, errorArgs);
}

function getJasmineOptionsPath(projectFolder) {
    return path.join(projectFolder, "test", "jasmine.json");
}

function detectJasmine(projectFolder) {
    try {
        var node_modulesFolder = path.join(projectFolder, "node_modules");
        var options = loadJsonOptions(getJasmineOptionsPath(projectFolder));
        if (options && options.path) {
            node_modulesFolder = path.resolve(projectFolder, options.path);
        }
        return require(path.join(node_modulesFolder, "jasmine"));
    }
    catch (ex) {
        logError('Failed to find Jasmine package. Jasmine must be installed in the project locally.' + EOL +
            'Install Jasmine locally using the npm manager via solution explorer' + EOL +
            'or with ".npm install jasmine --save-dev" via the Node.js interactive window.');
    }
    return null;
}

function loadJsonOptions(optionsPath) {
    if (fs.existsSync(optionsPath)) {
        return require(optionsPath);
    }
}

function loadJasmineOptions(projectFolder) {
    var options = loadJsonOptions(getJasmineOptionsPath(projectFolder));
    if (options && options.configFile) {
        var optionsPath = path.join(projectFolder, "test", options.configFile);
        options = loadJsonOptions(optionsPath);
    }
    return options;
}

function mergeOptions(target, source) {
    for (var opt in source) {
        target[opt] = source[opt];
    }
}

function getJasmineOptions(projectFolder) {
    var jasmineOptions = defaultJasmineOptions;
    try {
        var options = loadJasmineOptions(projectFolder);
        options && mergeOptions(jasmineOptions, options);
        options && console.log("Found jasmine.json file.");
    }
    catch (ex) {
        console.error("Failed to load Jasmine setting, using default settings.", ex);
    }
    console.log("Using Jasmine settings: ", jasmineOptions);
    return jasmineOptions;
}

function applyJasmineOptions(jasmineInstance, options) {
    if (options) {
        jasmineInstance.loadConfig(options);
    }
}

function initializeJasmine(Jasmine, projectFolder) {
    var instance = new Jasmine();
    applyJasmineOptions(instance, getJasmineOptions(projectFolder));
    return instance;
}

/**
 * @param {jasmine.Suite} suite
 * @param {object[]} testList
 * @param {string} testFile
 */
function enumerateSpecs(suite, testList, testFile) {
    suite.children.forEach((child) => {
        if (child instanceof jasmine.Suite) {
            enumerateSpecs(child, testList, testFile);
        } else {
            testList.push({
                name: child.description,
                suite: suite.description === "Jasmine__TopLevel__Suite" ? null : suite.getFullName(),
                filepath: testFile,
                line: 0,
                column: 0
            });
        }
    });
}

/**
 * @param {string} testFileList
 * @param {string} discoverResultFile
 * @param {string} projectFolder
 */
function find_tests(testFileList, discoverResultFile, projectFolder) {
    var Jasmine = detectJasmine(projectFolder);
    if (!Jasmine) {
        return;
    }
    var jasmineInstance = initializeJasmine(Jasmine, projectFolder);
    setSpecFilter(jasmineInstance, _ => false);

    var testList = [];
    testFileList.split(";").forEach((testFile) => {
        try {
            jasmineInstance.specDir = "";
            jasmineInstance.specFiles = [];
            jasmineInstance.addSpecFiles([testFile]);
            jasmineInstance.loadSpecs();

            var topSuite = jasmineInstance.env.topSuite();
            enumerateSpecs(topSuite, testList, testFile);
        }
        catch (ex) {
            //we would like continue discover other files, so swallow, log and continue;
            console.error("Test discovery error:", ex, "in", testFile);
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
}

exports.find_tests = find_tests;

function createCustomReporter(context) {
    return {
        specStarted: (specResult) => {
            context.post({
                type: "test start",
                fullyQualifiedName: context.getFullyQualifiedName(specResult.fullName)
            });
        },
        specDone: (specResult) => {
            // TODO: Report the output of the test. Currently is only showing "F" for a regression.
            var type = "result";
            var result = {
                passed: specResult.status === "passed",
                pending: false
            };

            if (specResult.status === "disabled" || specResult.status === "pending") {
                type = "pending";
                result.pending = true;
            }
            context.post({
                type,
                result,
                fullyQualifiedName: context.getFullyQualifiedName(specResult.fullName)
            });
            context.clearOutputs();
        },
        jasmineDone: (suiteInfo) => {
            context.post({
                type: "end"
            });
        }
    };
}

function run_tests(context) {
    var projectFolder = context.testCases[0].projectFolder;
    var Jasmine = detectJasmine(projectFolder);
    if (!Jasmine) {
        return;
    }
    var testFileList = [];
    var testNameList = {};

    context.testCases.forEach((testCase) => {
        if (testFileList.indexOf(testCase.testFile) < 0) {
            testFileList.push(testCase.testFile);
        }
        testNameList[testCase.fullTitle] = true;
    });
    try {
        var jasmineInstance = initializeJasmine(Jasmine, projectFolder);
        jasmineInstance.configureDefaultReporter({ showColors: false });
        setSpecFilter(jasmineInstance, spec => testNameList.hasOwnProperty(spec.getSpecName(spec)));
        jasmineInstance.addReporter(createCustomReporter(context));
        jasmineInstance.execute(testFileList);
    }
    catch (ex) {
        logError("Execute test error:", ex);
    }
}

function setSpecFilter(jasmineInstance, specFilter) {
    if (jasmineInstance.env.configure) {
        jasmineInstance.env.configure({ specFilter });
    } else {
        jasmineInstance.env.specFilter = specFilter;
    }
}

exports.run_tests = run_tests;
