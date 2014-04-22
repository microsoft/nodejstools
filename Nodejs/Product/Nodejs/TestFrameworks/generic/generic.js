var fs = require('fs');

var find_tests = function(testFile, discoverResultFile) {
  var testCases = require(testFile);
  var testList;
  for(var test in testCases) {
    testList = testList ? testList + '\r\n' + test : test;
  }
  if (testList) {
    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, testList);
    fs.closeSync(fd);
  }
}
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile) {
  var testCase = require(testFile);
  testCase[testName]();
}
module.exports.run_tests = run_tests;




