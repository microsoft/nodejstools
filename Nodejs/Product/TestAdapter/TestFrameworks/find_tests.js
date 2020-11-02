//@ts-check
const fs = require("fs");

async function findTests() {
    let framework;
    try {
        framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
    } catch (exception) {
        throw new Error("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
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

        await framework.find_tests(testFilesList, process.argv[4], process.argv[5]);
    } catch (exception) {
        throw new Error("NTVS_ERROR:TestFramework (" + process.argv[2] + ") threw an exception processing (" + process.argv[3] + "), " + exception);
    }
}

findTests().catch(e => {
    console.log(e);
    process.exit(1);
});