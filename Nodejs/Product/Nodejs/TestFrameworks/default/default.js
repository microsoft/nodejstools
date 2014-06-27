var fs = require('fs');

var find_tests = function(testFile, discoverResultFile) {
  var testCases = require(testFile);
  var testList = [];
  for (var test in testCases) {
    testList.push({
      test: test,
      suite: ''
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