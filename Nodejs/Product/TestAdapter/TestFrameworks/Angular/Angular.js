// @ts-check
"use strict";
const fs = require('fs');
const os = require('os');
const path = require('path');
const { fork } = require("child_process");

process.env.VSTESTADAPTERPATH = __dirname;

const vsKarmaConfigFilePath = path.resolve(__dirname, "./karmaConfig.js");

function getTestProjects(configFile) {
    const configPath = path.dirname(configFile);

    const angularProjects = [];
    const angularConfig = require(configFile);
    for (const projectName of Object.keys(angularConfig.projects)) {
        const project = angularConfig.projects[projectName];

        const karmaConfigFilePath = project.architect.test
            && project.architect.test.options
            && project.architect.test.options.karmaConfig
            && path.resolve(configPath, project.architect.test.options.karmaConfig);

        if (karmaConfigFilePath) {
            angularProjects.push({
                angularConfigPath: configPath,
                karmaConfigFilePath,
                name: projectName,
                rootPath: path.join(configPath, project.root),
            });
        }
    }

    return angularProjects;
}

const find_tests = async function (configFiles, discoverResultFile) {
    const projects = [];

    for (const configFile of configFiles.split(';')) {
        const configPath = path.dirname(configFile);

        if (!detectPackage(configPath, '@angular/cli')) {
            continue;
        }

        projects.push(...getTestProjects(configFile));
    }

    process.env.TESTCASES = JSON.stringify([{ fullTitle: "NTVS_Discovery_ThisStringShouldExcludeAllTestCases" }]);
    process.env.ISDISCOVERY = 'true';

    const testsDiscovered = [];

    for (const project of projects) {
        // Loop each project one by one. I'm not sure why multiple instances gets locked. We do receive an Angular warning
        // on a lock file for building the project, that might be the reason.
        await new Promise((resolve, reject) => {
            const ngTest = fork(
                path.resolve(project.angularConfigPath, "./node_modules/@angular/cli/bin/ng"),
                [
                    'test',
                    project.name,
                    `--karmaConfig=${vsKarmaConfigFilePath}`
                ],
                {
                    env: {
                        ...process.env,
                        PROJECT: JSON.stringify(project)
                    },
                    cwd: project.angularConfigPath,
                }).on('message', message => {
                    testsDiscovered.push(message);

                    // We need to keep track and communicate when we have received a testcase because the IPC channel
                    // does not guarantees that we'll receive the event on the order it has been emitted.
                    // Send to the child process as simple signal that we have parsed the testcase.
                    ngTest.send({});
                }).on('error', err => {
                    reject(err);
                }).on('exit', code => {
                    resolve(code);
                });
        });
    }

    // Save tests to file.
    const fd = fs.openSync(discoverResultFile, 'w');
    fs.writeSync(fd, JSON.stringify(testsDiscovered));
    fs.closeSync(fd);
}

const run_tests = async function (context) {
    for (const testCase of context.testCases) {
        context.post({
            type: 'test start',
            fullyQualifiedName: testCase.fullyQualifiedName
        });
    }

    // Get all the projects
    const projects = [];
    const angularConfigPaths = new Set();
    for (let testCase of context.testCases) {
        if (!angularConfigPaths.has(testCase.projectFolder)) {
            angularConfigPaths.add(testCase.projectFolder);

            if (!detectPackage(testCase.projectFolder, '@angular/cli')) {
                continue;
            }

            projects.push(...getTestProjects(`${testCase.projectFolder}/angular.json`));
        }
    }

    // Set the environment variable to share it across processes.
    process.env.TESTCASES = JSON.stringify(context.testCases);

    for (const project of projects) {
        // Loop each project one by one. I'm not sure why multiple instances gets locked. We do receive an Angular warning
        // on a lock file for building the project, that might be the reason.
        await new Promise((resolve, reject) => {
            const ngTest = fork(
                path.resolve(project.angularConfigPath, "./node_modules/@angular/cli/bin/ng"),
                [
                    'test',
                    project.name,
                    `--karmaConfig=${vsKarmaConfigFilePath}`
                ],
                {
                    env: {
                        ...process.env,
                        PROJECT: JSON.stringify(project)
                    },
                    cwd: project.angularConfigPath,
                    stdio: ['ignore', 1, 2, 'ipc'] // We need to ignore the stdin as NTVS keeps it open and causes the process to wait indefinitely.
                }).on('message', message => {
                    context.post({
                        type: message.pending ? 'pending' : 'result',
                        fullyQualifiedName: context.getFullyQualifiedName(message.fullName),
                        result: message
                    });

                    ngTest.send({});
                }).on('exit', code => {
                    resolve(code);
                }).on('error', err => {
                    reject(err);
                });
        });
    }

    context.post({
        type: 'end'
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