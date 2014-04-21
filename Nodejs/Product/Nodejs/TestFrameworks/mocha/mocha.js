var fs = require('fs');

var find_tests = function (testFile, discoverResultFile, projectFolder) {
  var mocha = require(projectFolder + '\\node_modules\\mocha');
  var discover = new mocha({});
  var testList;
  function getTestList(suite) {
    if (suite) {
		  if (suite.tests && suite.tests.length !== 0) {
        testList = testList ? testList + '\r\n' + suite.title : suite.title;
      }
      if (suite.suites) {
        suite.suites.forEach(function (s, i, ss) {
          getTestList(s);
        });
      }
    }
  }
  discover.addFile(testFile);
  discover.loadFiles();
  getTestList(discover.suite);
  if (testList) {
    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, testList);
    fs.closeSync(fd);
  }
}
module.exports.find_tests = find_tests;