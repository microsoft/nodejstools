// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');

const find_tests = function (testFileList, discoverResultFile, projectFolder) {
    return new Promise(resolve => {
        const jest = detectPackage(projectFolder, 'jest');
        const jestEditorSupport = detectPackage(projectFolder, 'jest-editor-support');

        if (!jest || !jestEditorSupport) {
            return resolve();
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

        resolve();
    });
};

const run_tests = function (context) {
    const projectFolder = context.testCases[0].projectFolder;

    // NODE_ENV sets the environment context for Node, it can be development, production or test and it needs to be set up for jest to work.
    // If no value was assigned, assign test.
    process.env.NODE_ENV =  process.env.NODE_ENV || 'test';
    return new Promise(async resolve => {
        const jest = detectPackage(projectFolder, 'jest');
        if (!jest) {
            return resolve();
        }

        // Start all test cases, as jest is unable to filter out independently
        for (const testCase of context.testCases) {
            context.post({
                type: 'test start',
                fullyQualifiedName: testCase.fullyQualifiedName
            });
        }

        let config = readConfigs(projectFolder, context);

        const argv = {
            json: true,
            reporters: [[__dirname + '/jestReporter.js', { context }]],
            config : JSON.stringify(config)
        }

        try {
            await jest.runCLI(argv, [projectFolder]);
        } catch (error) {
            logError(error);
        }

        resolve();
    });
};

function visitNodes(nodes, suites, tests) {
    if (!nodes || nodes.length === 0) {
        return;
    }

    for (let node of nodes) {
        switch (node.type) {
            case "describe":
                const parent = suites.length > 0 ? `${suites[suites.length - 1]} ` : '';
                suites.push(`${parent}${node.name}`);

                visitNodes(node.children, suites, tests);

                suites.pop();
                break;
            case "it":
                tests.push({
                    column: node.start.column,
                    filepath: node.file,
                    line: node.start.line,
                    suite: suites.length === 0 ? null : suites[suites.length - 1],
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

function readConfigs(projectFolder, context)
{
    // First look for Jest.config.js, otherwise look at the package.json under "jest" tag
    var userConfig;
    const jestConfigPath = projectFolder + "\\jest.config.js";
    const packageJsonPath = projectFolder + "\\package.json";

    if(fs.existsSync(jestConfigPath))
    {
        userConfig = require(jestConfigPath);
        mergeConfigs();
        return userConfig;
    }

    if(fs.existsSync(packageJsonPath))
    {
        userConfig = require(packageJsonPath).jest;
        mergeConfigs();
        return userConfig;
    }

    function mergeConfigs()
    {
        // If no config was found OR the user doesn't have these tags set up, add it.
        if(!userConfig) userConfig = {};
        if(!userConfig.setupFilesAfterEnv) userConfig.setupFilesAfterEnv = ["<rootDir>/src/setupTests.js"];
        if(!userConfig.testMatch) userConfig.testMatch = [context.testCases[0].testFile];
        if(!userConfig.transform) userConfig.transform = {"^.+\\.(js|jsx|mjs|cjs|ts|tsx)$": projectFolder + "\\node_modules\\react-scripts\\config\\jest\\babelTransform.js","^.+\\\\.css$": projectFolder + "\\\\node_modules\\\\react-scripts\\\\config\\\\jest\\\\cssTransform.js","^(?!.*\\\\.(js|jsx|mjs|cjs|ts|tsx|css|json)$)": projectFolder + "\\\\node_modules\\\\react-scripts\\\\config\\\\jest\\\\fileTransform.js"};
    }
}

module.exports.find_tests = find_tests;
module.exports.run_tests = run_tests;