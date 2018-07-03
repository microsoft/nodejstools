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
    // Jest package is not required for this tests to run, but it's still good to check 
    // in case the user have a build system or an automation that requires Jest.
    const jest = detectPackage(testCases[0].projectFolder, 'jest');
    const jestEditorSupport = detectPackage(testCases[0].projectFolder, 'jest-editor-support');

    if (!jest || !jestEditorSupport) {
        return;
    }

    const workspace = {
        rootPath: testCases[0].projectFolder,
        pathToJest: getJestPath(testCases[0].projectFolder),
        pathToConfig: '',
        localJestMajorVersion: 23 // TODO: Get the jest version from the package.
    };

    // Promise chain to make it process the test cases one by one.
    testCases.reduce((promise, testCase) => {
        return promise.then(() => {
            return runTest(testCase, post);
        });
    }, Promise.resolve())
        .then(() => post({
            type: 'suite end',
            result: {}
        }));

    function runTest(testCase, post) {
        return new Promise((resolve, reject) => {
            post({
                type: 'test start',
                title: testCase.testName
            });

            const runner = new jestEditorSupport.Runner(workspace, {
                testFileNamePattern: escapeRegExp(testCase.testFile),
                testNamePattern: escapeRegExp(testCase.testName)
            });

            runner
                // Jest uses the stdErr for output to the console.
                .on('executableStdErr', (data) => {
                    if (data.toString().indexOf("Test results written to") !== -1) {
                        const tempFile = os.tmpdir + '/jest_runner.json';
                        fs.readFile(tempFile, (err, resultFile) => {

                            if (err) {
                                return reject(err);
                            }

                            const parsedResult = JSON.parse(resultFile.toString());

                            for (const testResult of parsedResult.testResults) {
                                // Filter out pending test cases.
                                const assertionResult = testResult.assertionResults.find(x => x.status !== 'pending');
                                if (assertionResult.length !== 0) {
                                    const result = {
                                        passed: assertionResult.status === 'passed',
                                        pending: false,
                                        stderr: assertionResult.failureMessages.join('\n'),
                                        stdout: "",
                                        title: assertionResult.title
                                    };

                                    post({
                                        type: 'result',
                                        title: assertionResult.title,
                                        result: result
                                    });

                                    return resolve(testCase);
                                }
                            }

                            return reject(new Error('Test case has no result or was not found.'));
                        });
                    }
                })
                .on('terminalError', (error) => reject(error));

            runner.start(false);
        });
    }
};

function getJestPath(projectFolder) {
    return path.join(projectFolder, "node_modules/.bin/jest.cmd");
}

function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}

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