var framework;
try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    process.exit(1);
}

framework.run_tests(process.argv[3], process.argv[4], process.argv[5], process.argv[6]);

