var fs = require('fs');

var find_tests = function (testFile, discoverResultFile, projectFolder) {
  var Mocha = require(projectFolder + '\\node_modules\\mocha');
  var mocha = new Mocha();
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
  mocha.addFile(testFile);
  mocha.loadFiles();
  getTestList(mocha.suite);
  if (testList) {
    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, testList);
    fs.closeSync(fd);
  }
}
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile, projectFolder) {
  var Mocha = new require(projectFolder + '\\node_modules\\mocha');
  var mocha = new Mocha();
  mocha.suite.timeout(30000); //TODO: config
  if (testName) {
    mocha.grep(testName);
  }
  mocha.addFile(testFile);
  mocha.run(function (code) {
    console.log('Done with Code:' + code);
    process.exit(code);
  });
}
module.exports.run_tests = run_tests;