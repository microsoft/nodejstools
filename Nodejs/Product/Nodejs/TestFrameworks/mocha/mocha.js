var fs = require('fs');

var find_tests = function (testFileList, discoverResultFile, projectFolder) {
    var Mocha = detectMocha(projectFolder);
    if (!Mocha) {
        return;
    }

    function getTestList(suite, testFile) {
        if (suite) {
            if (suite.tests && suite.tests.length !== 0) {
                suite.tests.forEach(function (t, i, testArray) {
                    testList.push({
                        test: t.fullTitle(),
                        suite: suite.fullTitle(),
                        file: testFile,
                        line: 0,
                        column: 0
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
    testFileList.split(';').forEach(function (testFile) {
        var mocha = new Mocha();
        try {
            mocha.ui('tdd');
            mocha.addFile(testFile);
            mocha.loadFiles();
            getTestList(mocha.suite, testFile);
        } catch (e) {
            //we would like continue discover other files, so swallow, log and continue;
            console.error('NTVS_ERROR:', e, "in", testFile);
        }
    });

    var fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};
module.exports.find_tests = find_tests;

var run_tests = function (testName, testFile, workingFolder, projectFolder) {
    var Mocha = detectMocha(projectFolder);
    if (!Mocha) {
        return;
    }

    var mocha = new Mocha();
    mocha.ui('tdd');
    //set timeout to 10 minutes, because the default of 2 sec might be too short (TODO: make it configurable)
    mocha.suite.timeout(600000);
    if (testName) {
        mocha.grep(testName);
    }
    mocha.addFile(testFile);

    // Choose 'tap' rather than 'min' or 'xunit'. The reason is that
    // 'min' produces undisplayable text to stdout and stderr under piped/redirect, 
    // and 'xunit' does not print the stack trace from the test.
    mocha.reporter('tap');

    mocha.run(function (code) {
        process.exit(code);
    });
};

function detectMocha(projectFolder) {
    try {
        var Mocha = new require(projectFolder + '\\node_modules\\mocha');
        return Mocha;
    } catch (ex) {
        console.log("NTVS_ERROR:Failed to find Mocha package.  Mocha must be installed in the project locally.  Mocha can be installed locally with the npm manager via solution explorer or with \".npm install mocha\" via the Node.js interactive window.");
        return null;
    }
}

module.exports.run_tests = run_tests;
