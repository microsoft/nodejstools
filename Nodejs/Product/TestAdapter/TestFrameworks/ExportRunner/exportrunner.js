//@ts-check
var fs = require('fs');
var path = require('path');
var vm = require('vm');
var result = {
    fullyQualifiedName: '',
    passed: false
};

var find_tests = function (testFileList, discoverResultFile) {
    var debug;
    try {
        if (vm.runInDebugContext) {
            debug = vm.runInDebugContext('Debug');
        }
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
                    if (funcDetails !== undefined) {
                        line = funcDetails.line; // 0 based
                        column = funcDetails.column; // 0 based
                    }
                } catch (e) {
                    //If we take an exception mapping the source line, simply fallback to unknown source map 
                }
            }
            testList.push({
                name: test,
                suite: '',
                filepath: testFile,
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

var run_tests = function (context) {
    for (var test of context.testCases) {
        context.post({
            type: 'test start',
            fullyQualifiedName: test.fullyQualifiedName
        });
        try {
            var testCase = require(test.testFile);
            result.fullyQualifiedName = test.fullyQualifiedName;
            testCase[test.fullTitle]();
            result.passed = true;
            console.log("Test passed.\n");
        } catch (err) {
            result.passed = false;
            console.error(err.name);
            console.error(err.message);
        }
        context.post({
            type: 'result',
            fullyQualifiedName: test.fullyQualifiedName,
            result: result
        });
        context.clearOutputs();
    }
    context.callback({
        type: 'end'
    });
    process.exit();
};
module.exports.run_tests = run_tests;