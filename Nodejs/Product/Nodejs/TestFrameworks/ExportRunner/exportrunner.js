var fs = require('fs');

var find_tests = function (testFileList, discoverResultFile) {
    var testList = [];
    testFileList.split(';').forEach(function (testFile) {
        var testCases;
        try {
            testCases = require(testFile);
        } catch (ex) {
            console.error("NTVS_ERROR:" + ex);
            return;
        }
        for (var test in testCases) {
            var line = 0;
            var column = 0;
            if (dbg != undefined) {
                try {
                    var funcDetails = dbg.Debug.findFunctionSourceLocation(testCases[test]);
                    if (funcDetails != undefined) {
                        //v8 is 0 based line numbers, editor is 1 based
                        line = parseInt(funcDetails.line) + 1;
                        //v8 and editor are both 1 based column numbers, no adjustment necessary
                        column = parseInt(funcDetails.column);
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

var run_tests = function (testName, testFile) {
    var testCase = require(testFile);
    testCase[testName]();
};
module.exports.run_tests = run_tests;