"use strict";
var EOL = require('os').EOL;
var fs = require('fs');
var path = require('path');
var result = {
    'title': '',
    'passed': false,
    'stdOut': '',
    'stdErr': '',
    'time': 0
};

function append_stdout(string, encoding, fd) {
    result.stdOut += string;
}
function append_stderr(string, encoding, fd) {
    result.stdErr += string;
}

function find_tests(testFileList, discoverResultFile, projectFolder) {
    var test = findTape(projectFolder);
    if (test === null) {
        return;
    }

    var harness = test.getHarness({ exit: false });
    var tests = harness['_tests'];

    var count = 0;
    var testList = [];
    testFileList.split(';').forEach(function (testFile) {
        var testCases = loadTestCases(testFile);
        if (testCases === null) return; // continue to next testFile

        for (; count < tests.length; count++) {
            var t = tests[count];
            t._skip = true; // don't run tests
            testList.push({
                test: t.name,
                suite: '',
                file: testFile,
                line: 0,
                column: 0
            });
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};
module.exports.find_tests = find_tests;

function run_tests(testInfo, callback) {
    var testResults = [];
    var testCases = loadTestCases(testInfo[0].testFile);
    process.stdout.write = append_stdout;
    process.stderr.write = append_stderr;
    if (testCases === null) {
        return;
    }

    var tape = findTape(testInfo[0].projectFolder);
    if (tape === null) {
        return;
    }

    for (var test in testInfo) {
        result.title = testInfo[test].testName;
        try {
            result.time = Date.now();
            var harness = tape.getHarness();
            harness(testInfo[test].testName);
            result.passed = true;
        } catch (e) {
            result.passed = false;
            logError('Error running test:', testInfo[test].testName, 'in', testInfo[test].testFile, e);
        }
        result.time = Date.now() - result.time;
        testResults.push(result);
        result = {
            'title': '',
            'passed': false,
            'stdOut': '',
            'stdErr': '',
            'time': 0
        };
    }

    callback(testResults);
}
module.exports.run_tests = run_tests;

function loadTestCases(testFile) {
    try {
        process.chdir(path.dirname(testFile));
        return require(testFile);
    } catch (e) {
        // we would like continue discover other files, so swallow, log and continue;
        logError("Test discovery error:", e, "in", testFile);
        return null;
    }
}

function findTape(projectFolder) {
    try {
        var tapePath = path.join(projectFolder, 'node_modules', 'tape');
        return require(tapePath);
    } catch (e) {
        logError(
            'Failed to find Tape package.  Tape must be installed in the project locally.' + EOL +
            'Install Tape locally using the npm manager via solution explorer' + EOL +
            'or with ".npm install tape --save-dev" via the Node.js interactive window.');
        return null;
    }
}

function logError() {
    var errorArgs = Array.prototype.slice.call(arguments);
    errorArgs.unshift("NTVS_ERROR:");
    console.error.apply(console, errorArgs);
}
