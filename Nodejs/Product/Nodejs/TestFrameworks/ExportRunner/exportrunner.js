var fs = require('fs');

var find_tests = function (testFileList, discoverResultFile) {
  var testList = [];
  testFileList.split(';').forEach(function (testFile) {
    var testCases;
    try {
      testCases = require(testFile);
    } catch (ex) {
      console.error(ex);
    }
    for (var test in testCases) {
      testList.push({
        test: test,
        suite: '',
        file: testFile
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