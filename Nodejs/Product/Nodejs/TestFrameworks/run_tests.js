var framework;
var readline = require('readline');

var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.on('line', (line) => {
    var testInfo = JSON.parse(line);
    // get rid of leftover quotations from C# (necessary?)
    for(var s in testInfo) {
        testInfo[s] = testInfo[s].replace(/["]+/g, '');
    }

    try {
        framework = require('./' + testInfo.framework + '/' + testInfo.framework + '.js');
    } catch (exception) {
        console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
        process.exit(1);
    }
    // run the test
    framework.run_tests(testInfo.testName, testInfo.testFile, testInfo.workingFolder, testInfo.projectFolder);

    // close readline interface
    rl.close();
});
