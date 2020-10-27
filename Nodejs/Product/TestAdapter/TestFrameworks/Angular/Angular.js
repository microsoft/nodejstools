// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');
const { fork } = require("child_process");

process.env.VSTESTADAPTERPATH = __dirname;

const vsKarmaConfigPath = path.resolve(__dirname, "./karmaConfig.js");

function getKarmaConfigPath(configFile, configPath) {
    let karmaConfigPath = '';
    const angularConfig = require(configFile);
    for (const projectName of Object.keys(angularConfig.projects)) {
        const project = angularConfig.projects[projectName];

        karmaConfigPath = project.architect.test
            && project.architect.test.options
            && project.architect.test.options.karmaConfig
            && path.resolve(configPath, project.architect.test.options.karmaConfig);

        // TODO: For now, only return the first one found. We need to support multi-project workspaces.
        if (karmaConfigPath) {
            return karmaConfigPath;
        }
    }
}

const find_tests = function (configFiles, discoverResultFile) {
    return new Promise(resolve => {
        for (let configFile of configFiles.split(';')) {
            const configPath = path.dirname(configFile);

            if (!detectPackage(configPath, '@angular/cli')) {
                continue;
            }

            const karmaConfigPath = getKarmaConfigPath(configFile, configPath);

            // Set the environment variable to share it across processes.
            process.env.KARMACONFIGPATH = karmaConfigPath;
            process.env.PROJECTPATH = configPath;
            process.env.TESTCASES = JSON.stringify([{ fullTitle: "NTVS_Discovery_ThisStringShouldExcludeAllTestCases" }]);
            process.env.ISDISCOVERY = 'true';

            const testsDiscovered = [];

            const ngTest = fork(
                path.resolve(configPath, "./node_modules/@angular/cli/bin/ng"),
                [
                    'test',
                    `--karmaConfig=${vsKarmaConfigPath}`
                ],
                {
                    cwd: configPath,
                }).on('message', message => {
                    testsDiscovered.push(message);

                    // We need to keep track and communicate when we have received a testcase because the IPC channel
                    // does not guarantees that we'll receive the event on the order it has been emitted.
                    // Send to the child process as simple signal that we have parsed the testcase.
                    ngTest.send({});
                }).on('exit', () => {
                    const fd = fs.openSync(discoverResultFile, 'w');
                    fs.writeSync(fd, JSON.stringify(testsDiscovered));
                    fs.closeSync(fd);

                    resolve();
                });
        };
    });
}

const run_tests = function (context) {
    const projectFolder = context.testCases[0].projectFolder;

    // TODO: Handle the scenario where Angular.json may not exists on a child folder instead of root.
    // One way would be to send the location of angular.json instead of assuming it's on root.
    const configFile = `${projectFolder}/angular.json`;

    for (const testCase of context.testCases) {
        context.post({
            type: 'test start',
            fullyQualifiedName: testCase.fullyQualifiedName
        });
    }

    return new Promise(resolve => {
        const angular = detectPackage(projectFolder, '@angular/cli');
        if (!angular) {
            return resolve();
        }

        const configPath = path.dirname(configFile);

        // Set the environment variable to share it across processes.
        process.env.KARMACONFIGPATH = getKarmaConfigPath(configFile, configPath);
        process.env.PROJECTPATH = configPath;
        process.env.TESTCASES = JSON.stringify(context.testCases);

        const ngTest = fork(
            path.resolve(configPath, "./node_modules/@angular/cli/bin/ng"),
            [
                'test',
                `--karmaConfig=${vsKarmaConfigPath}`
            ],
            {
                cwd: configPath,
                stdio: ['ignore', 1, 2, 'ipc'] // We need to ignore the stdin as NTVS keeps it open and causes the process to wait indefinitely.
            }).on("message", message => {
                context.post({
                    type: message.pending ? 'pending' : 'result',
                    fullyQualifiedName: context.getFullyQualifiedName(message.fullName),
                    result: message
                });

                ngTest.send({});
            }).on('exit', () => {
                context.post({
                    type: 'end'
                });

                resolve();
            }).on('error', err => {
                console.log(err);
            });
    });
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