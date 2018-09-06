var fs = require("fs");
var framework;
try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    process.exit(1);
}
try {
    var testFilesListInputFile = process.argv[3];
    if (!fs.existsSync(testFilesListInputFile)) {
        throw new Error("testFilesListInputFile '" + testFilesListInputFile + "' does not exist.");
    }

    var testFilesList = fs.readFileSync(testFilesListInputFile, "utf-8");

    // strip the BOM in case of UTF-8
    if (testFilesList.charCodeAt(0) === 0xFEFF) {
        testFilesList = testFilesList.slice(1);
    }

    framework.find_tests(testFilesList, process.argv[4], process.argv[5]);
} catch (exception) {
    console.log("NTVS_ERROR:TestFramework (" + process.argv[2] + ") threw an exception processing (" + process.argv[3] + "), " + exception);
    process.exit(1);
}
process.exit(0);