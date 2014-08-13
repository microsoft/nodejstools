var framework;
try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    exit;
}
try {
    framework.run_tests(process.argv[3], process.argv[4], process.argv[5], process.argv[6]);
} catch (exception) {
    console.log("NTVS_ERROR:TestFramework (" + process.argv[2] + ") threw an exception processing (" + process.argv[3] + "), " + exception);
}

