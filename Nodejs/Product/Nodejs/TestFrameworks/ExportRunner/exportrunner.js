var fs = require('fs');

var find_tests = function (testFile, discoverResultFile) {
    var testCases = require(testFile);
    var testList = [];
    for (var test in testCases) {
        var line = 0;
        var column = 0;
        if (dbg != undefined) {
            try {
                var funcDetails = dbg.Debug.findFunctionSourceLocation(testCases[test]);
                if (funcDetails != undefined) {
                    //v8 is 0 based line numbers, The editor is 1 based
                    line = parseInt(funcDetails.line) + 1;
                    column = parseInt(funcDetails.column) + 1;
                }
            } catch(e) {
                //If we take an exception mapping the source line, simply fallback to unknown source map 
            }
        }
        testList.push({
            test: test,
            suite: '',
            line: line,
            column: column
        });
    }
    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
}
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile) {
    var testCase = require(testFile);
    testCase[testName]();
}
module.exports.run_tests = run_tests;