// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');

const find_tests = function (testFileList, discoverResultFile, projectFolder) {
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

            visitNodes(parseResult.root.children, [], testList);
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

const run_tests = function (context, post) {
    const jest = detectPackage(context.testCases[0].projectFolder, 'jest');
    if (!jest) {
        return;
    }

    // Start all test cases, as jest is unable to filter out independently
    for (const testCase of context.testCases) {
        post({
            type: 'test start',
            fullyQualifiedName: testCase.fullyQualifiedName
        });
    }

    const config = {
        json: true,
        reporters: [[__dirname + '/jestReporter.js', { context, post }]],
        testMatch: [context.testCases[0].testFile]
    };

    jest.runCLI(config, [context.testCases[0].projectFolder])
        .catch((error) => logError(error));
};

function visitNodes(nodes, suites, tests) {
    if (!nodes || nodes.length === 0) {
        return;
    }

    for (let node of nodes) {
        switch (node.type) {
            case "describe":
                suites.push(node.name);
                visitNodes(node.children, suites, tests);
                suites.pop();
                break;
            case "it":
                tests.push({
                    column: node.start.column,
                    filepath: node.file,
                    line: node.start.line,
                    suite: suites.length === 0 ? null : suites.join(" "),
                    name: node.name
                });
                break;
        }
    }
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
    var errorArgs = Array.from(arguments);
    errorArgs.unshift("NTVS_ERROR:");
    console.error.apply(console, errorArgs);
}

module.exports.find_tests = find_tests;
module.exports.run_tests = run_tests;