// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');

const find_tests = function (testFileList, discoverResultFile, projectFolder) {
    // Jest package is not required for this tests to run, but it's still good to check 
    // in case the user have a build system or an automation that requires Jest.
    const jest = detectPackage(projectFolder, 'jest');
    const jestEditorSupport = detectPackage(projectFolder, 'jest-editor-support');

    if (!jest || !jestEditorSupport) {
        return;
    }

    let testList = [];
    for (let testFile of testFileList.split(';')) {
        process.chdir(path.dirname(testFile));

        try {
            const parseResult = jestEditorSupport.parse(testFile);

            for (let test of parseResult.itBlocks) {
                testList.push({
                    column: test.start.column,
                    file: test.file,
                    line: test.start.line,
                    test: test.name
                });
            }

        }
        catch (e) {
            // We would like continue discover other files, so swallow, log and continue;
            console.error("Test discovery error:", e, "in", testFile);
        }
    }

    const fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testList));
    fs.closeSync(fd);
};

const run_tests = function (testCases, post) {
    const jest = detectPackage(testCases[0].projectFolder, 'jest');
    if (!jest) {
        return;
    }

    // Start all test cases, as jest is unable to filter out independently
    for (const testCase of testCases) {
        post({
            type: 'test start',
            title: testCase.testName
        });
    }

    const config = {
        json: true,
        reporters: [[__dirname + '/jestReporter.js', { post: post }]],
        testMatch: [testCases[0].testFile]
    };

    jest.runCLI(config, [testCases[0].projectFolder])
        .catch((error) => logError(error));
};

function detectPackage(projectFolder, packageName) {
    try {
        const packagePath = path.join(projectFolder, 'node_modules', packageName);
        const pkg = require(packagePath);

        return pkg;
    } catch (ex) {
        logError(
            `Failed to find "${packageName}" package. "${packageName}" must be installed in the project locally.` + os.EOL +
            `Install "${packageName}" locally using the npm manager via solution explorer` + os.EOL +
            `or with ".npm install ${packageName} --save-dev" via the Node.js interactive window.`);
        return null;
    }
}

function logError() {
    var errorArgs = Array.prototype.slice.call(arguments);
    errorArgs.unshift("NTVS_ERROR:");
    console.error.apply(console, errorArgs);
}

module.exports.find_tests = find_tests;
module.exports.run_tests = run_tests;