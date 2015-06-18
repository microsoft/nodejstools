var fs = require('fs');
var path = require('path');

// Choose 'tap' rather than 'min' or 'xunit'. The reason is that
// 'min' produces undisplayable text to stdout and stderr under piped/redirect, 
// and 'xunit' does not print the stack trace from the test.
var defaultMochaOptions = { ui: 'tdd', reporter: 'tap', timeout: 600000 };

var find_tests = function (testFileList, discoverResultFile, projectFolder) {
    var Mocha = detectMocha(projectFolder);
    if (!Mocha) {
        return;
    }

    function getTestList(suite, testFile) {
        if (suite) {
            if (suite.tests && suite.tests.length !== 0) {
                suite.tests.forEach(function (t, i, testArray) {
                    testList.push({
                        test: t.title,
                        suite: suite.fullTitle(),
                        file: testFile,
                        line: 0,
                        column: 0
                    });
                });
            }

            if (suite.suites) {
                suite.suites.forEach(function (s, i, suiteArray) {
                    getTestList(s, testFile);
                });
            }
        }
    }
    var testList = [];
    testFileList.split(';').forEach(function (testFile) {
        var mocha = initializeMocha(Mocha, projectFolder);
        try {
            mocha.addFile(testFile);
            mocha.loadFiles();
            getTestList(mocha.suite, testFile);
        } catch (e) {
            //we would like continue discover other files, so swallow, log and continue;
            logError('NTVS_ERROR: An error occurred during mocha test discovery in file: ' + testFile, e);
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile, workingFolder, projectFolder) {
    var Mocha = detectMocha(projectFolder);
    if (!Mocha) {
        return;
    }

    var mocha = initializeMocha(Mocha, projectFolder);

    if (testName) {
        mocha.grep(testName);
    }
    mocha.addFile(testFile);

    mocha.run(function (code) {
        process.exit(code);
    });
};

function logError(errorMessage, error) {
    if (typeof error === 'undefined') {
        console.error("NTVS_ERROR: " + errorMessage);
    } else {
        console.error("NTVS_ERROR: " + errorMessage, error);
    }
}

function detectMocha(projectFolder) {
    try {
        var mochaPath = path.join(projectFolder, 'node_modules', 'mocha');
        var Mocha = new require(mochaPath);
        return Mocha;
    } catch (ex) {
        logError("Failed to find Mocha package.  Mocha must be installed in the project locally.  Mocha can be installed locally with the npm manager via solution explorer or with \".npm install mocha\" via the Node.js interactive window.");
        return null;
    }
}

function initializeMocha(Mocha, projectFolder) {
    var mocha = new Mocha();
    applyMochaOptions(mocha, getMochaOptions(projectFolder));
    return mocha;
}

function applyMochaOptions(mocha, options) {
    if (options) {
        for (var opt in options) {
            var mochaOpt = mocha[opt];
            var optValue = options[opt];

            if (typeof mochaOpt === 'function') {
                try {
                    mochaOpt.call(mocha, optValue);
                } catch (e) {
                    logError("Could not set mocha option '" + opt + "' with value '" + optValue + "' due to error: ", e);
                }
            }
        }
    }
}

function getMochaOptions(projectFolder) {
    var mochaOptions = defaultMochaOptions;
    try {
        var optionsPath = path.join(projectFolder, 'test', 'mocha.json');
        var options = require(optionsPath);
        options = options || {};
        for (var opt in options) {
            mochaOptions[opt] = options[opt];
        }
    } catch (ex) {
        console.log("NTVS: mocha.json options file not found. Using default values.");
    }

    return mochaOptions;
}

module.exports.run_tests = run_tests;
