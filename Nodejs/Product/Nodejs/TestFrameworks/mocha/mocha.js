var fs = require('fs');

var find_tests = function (testFiles, discoverResultFile, projectFolder) {
    var Mocha;
    try {
        Mocha = require(projectFolder + '\\node_modules\\mocha');
    } catch (ex) {
        console.log("NTVS_ERROR:Failed to find Mocha package.  Mocha must be installed in the project locally.  Mocha can be installed locally with the npm manager via solution explorer or with \".npm install mocha\" via the Node.js interactive window.");
        return;
    }
    function getTestList(suite, testFile) {
        if (suite) {
            if (suite.tests && suite.tests.length !== 0) {
                suite.tests.forEach(function (t, i, testArray) {
                    testList.push({
                        test: t.fullTitle(),
                        suite: suite.fullTitle(),
                        file: testFile
                    });
                });
            }

            if (suite.suites) {
                suite.suites.forEach(function (s, i, suiteArray) {
                    getTestList(s, testFile);
                });
            }
        }
    }
    var testList = [];
    testFiles.split(';').forEach(function (testFile) {
        var mocha = new Mocha();
        try {
            mocha.ui('tdd');
            mocha.addFile(testFile);
            mocha.loadFiles();
            getTestList(mocha.suite, testFile);
        } catch (e) {
            //we would like continue discover other files, so swallow, log and continue;
            console.log('catch discover error:' + e);
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile, workingFolder, projectFolder) {
    var Mocha = new require(projectFolder + '\\node_modules\\mocha');
    var mocha = new Mocha();
    mocha.ui('tdd');
    //default at 2 sec might be too short (TODO: make it configuable)
    mocha.suite.timeout(3000000);
    if (testName) {
        mocha.grep(testName);
    }
    mocha.addFile(testFile);
    //Choose 'xunit' rather 'min'. The reason is when under piped/redirect,
    //mocha produces undisplayable text to stdout and stderr. Using xunit works fine 
    mocha.reporter('xunit');
    mocha.run(exitLater);
};

function exitLater(code) {
    process.on('exit', function () { process.exit(code); });
}

module.exports.run_tests = run_tests;