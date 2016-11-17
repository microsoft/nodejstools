var fs = require('fs');
var path = require('path');
var vm = require('vm');
var result = {
    'title': '',
    'passed': false,
    'stdOut': '',
    'stdErr': ''
};

function append_stdout(string, encoding, fd) {
    result.stdOut += string;
}
function append_stderr(string, encoding, fd) {
    result.stdErr += string;
}
process.stdout.write = append_stdout;
process.stderr.write = append_stderr;

var find_tests = function (testFileList, discoverResultFile) {
    var debug;
    try {
        debug = vm.runInDebugContext('Debug');
    } catch (ex) {
        console.error("NTVS_ERROR:", ex);
    }

    var testList = [];
    testFileList.split(';').forEach(function (testFile) {
        var testCases;
        process.chdir(path.dirname(testFile));
        try {
            testCases = require(testFile);
        } catch (ex) {
            console.error("NTVS_ERROR:", ex, "in", testFile);
            return;
        }
        for (var test in testCases) {
            var line = 0;
            var column = 0;
            if (debug !== undefined) {
                try {
                    var funcDetails = debug.findFunctionSourceLocation(testCases[test]);
                    if (funcDetails != undefined) {
                        line = funcDetails.line; // 0 based
                        column = funcDetails.column; // 0 based
                    }
                } catch (e) {
                    //If we take an exception mapping the source line, simply fallback to unknown source map 
                }
            }
            testList.push({
                test: test,
                suite: '',
                file: testFile,
                line: line,
                column: column
            });
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};
module.exports.find_tests = find_tests;

var run_tests = function (testCases, callback) {
    var event = {};
    for (var test of testCases) {
        event.type = 'testStart';
        event.title = test.testName;
        callback(event);
        try {
            var testCase = require(test.testFile);
            result.title = test.testName;
            testCase[test.testName]();
            result.passed = true;
        } catch (err) {
            result.passed = false;
            console.error(err.name);
            console.error(err.message);
        }
        event.type = 'result';
        event.result = result;
        callback(event);
        process.stdout.write = append_stdout;
        process.stderr.write = append_stderr;
        result = {
            'title': '',
            'passed': false,
            'stdOut': '',
            'stdErr': ''
        };
    }
};
module.exports.run_tests = run_tests;