// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');
const { spawn } = require("child_process");

process.env.VSTESTADAPTERPATH = __dirname;

const karmaConfigName = "karma.conf.js";
const vsKarmaConfigPath = path.resolve(__dirname, "./karmaConfig.js");

const find_tests = function (configFiles, discoverResultFile, projectFolder) {
    return new Promise(resolve => {
        const angular = detectPackage(projectFolder, '@angular/cli');
        if (!angular) {
            return;
        }
        for (let configFile of configFiles.split(';')) {
            const projectPath = path.dirname(configFile);
            const karmaConfigPath = path.resolve(projectPath, `./${karmaConfigName}`);

            if (!fs.existsSync(karmaConfigPath)) {
                logError(`Failed to find "${karmaConfigName}" file. The "${karmaConfigName}" file must exists in the same path as the "angular.json" file.`);
                continue;
            }

            // Set the environment variable to share it across processes.
            process.env.PROJECTPATH = projectPath;
            process.env.TESTCASES = JSON.stringify([{ fullTitle: "NTVS_Discovery_ThisStringShouldExcludeAllTestCases" }]);
            process.env.ISDISCOVERY = 'true';

            const ngTest = spawn(
                process.argv0, // Node executable path
                [
                    path.resolve(projectPath, "./node_modules/@angular/cli/bin/ng"),
                    'test',
                    `--karmaConfig="${vsKarmaConfigPath}"`
                ],
                {
                    cwd: projectPath,
                    shell: true,
                    stdio: ['pipe', 'ipc', 'pipe']
                });

            const testsDiscovered = [];

            ngTest.on('message', message => {
                testsDiscovered.push(message);
            });

            ngTest.on('exit', () => {
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
            return;
        }

        const projectPath = path.dirname(configFile);
        const karmaConfigPath = path.resolve(projectPath, `./${karmaConfigName}`);

        if (!fs.existsSync(karmaConfigPath)) {
            logError(`Failed to find "${karmaConfigName}" file. The "${karmaConfigName}" file must exists in the same path as the "angular.json" file.`);
            return;
        }

        // Set the environment variable to share it across processes.
        process.env.PROJECTPATH = projectPath;
        process.env.TESTCASES = JSON.stringify(context.testCases);

        const ngTest = spawn(
            process.argv0, //Node executable path
            [
                path.resolve(projectPath, "./node_modules/@angular/cli/bin/ng"),
                'test',
                `--karmaConfig="${vsKarmaConfigPath}"`
            ],
            {
                cwd: projectPath,
                shell: true,
                stdio: ['pipe', 'ipc', 'pipe']
            });

        ngTest.on("message", message => {
            context.post({
                type: message.pending ? 'pending' : 'result',
                fullyQualifiedName: context.getFullyQualifiedName(message.fullName),
                result: message
            });
        });

        ngTest.on('exit', () => {
            context.post({
                type: 'end'
            });

            resolve();
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