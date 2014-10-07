var fs = require('fs');

var find_tests = function (testFile, discoverResultFile, projectFolder) {
    var Jasmine;
    try {
        Jasmine = require(projectFolder + '\\node_modules\\jasmine');
    } catch (ex) {
        console.log("NTVS_ERROR:Failed to find Jasmine package.  Jasmine must be installed in the project locally.  Jasmine can be installed locally with the npm manager via solution explorer or with \".npm install jasmine\" via the Node.js interactive window.")
        return;
    }
    var jasmine = new Jasmine();
    var testList = [];
    function getTestList(suite) {
        if (suite) {
            if (suite.tests && suite.tests.length !== 0) {
                suite.tests.forEach(function (t, i, testArray) {
                    testList.push({
                        test: t.fullTitle(),
                        suite: suite.fullTitle()
                    });
                });
            }

            if (suite.suites) {
                suite.suites.forEach(function (s, i, suiteArray) {
                    getTestList(s);
                });
            }
        }
    }
    jasmine.addFile(testFile);
    jasmine.loadFiles();
    getTestList(jasmine.suite);

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
}
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile, workingFolder, projectFolder) {
    try {
        var Jasmine = new require(projectFolder + '\\node_modules\\jasmine');
    } catch (ex) {
        console.log("NTVS_ERROR:Failed to find Jasmine package.  Jasmine must be installed in the project locally.  Jasmine can be installed locally with the npm manager via solution explorer or with \".npm install jasmine\" via the Node.js interactive window.")
        return;
    }
    var jasmine = new Jasmine();
    //default at 2 sec might be too short (TODO: make it configuable)
    jasmine.suite.timeout(30000);
    if (testName) {
        jasmine.grep(testName);
    }
    jasmine.addFile(testFile);
    //Choose 'xunit' rather 'min'. The reason is when under piped/redirect,
    //jasmine produces undisplayable text to stdout and stderr. Using xunit works fine 
    jasmine.reporter('xunit');
    jasmine.run(exitLater);
}
function exitLater(code) {
    process.on('exit', function () { process.exit(code); })
}
module.exports.run_tests = run_tests;