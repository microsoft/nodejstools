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
    return new Promise(async resolve => {
        const angular = detectPackage(projectFolder, '@angular/cli');
        if (!angular) {
            return;
        }
        for (let configFile of configFiles.split(';')) {
            const projectPath = path.dirname(configFile);
            const karmaConfigPath = path.resolve(projectPath, `./${karmaConfigName}`);

            // process.chdir(configPath);
            if (!fs.existsSync(karmaConfigPath)) {
                logError(`Failed to find "${karmaConfigName}" file. The "${karmaConfigName}" file must exists in the same path as the "angular.json" file.`);
                continue;
            }

            // Set the environment variable to share it across processes.
            process.env.PROJECTPATH = projectPath;

            // TODO: Handle npx or some other way to run ng not using the node_modules path.
            const ngTest = spawn(
                'E:/NodeJS/node-v13.11.0-win-x64/node.exe',
                [
                    path.resolve(projectPath, "./node_modules/@angular/cli/bin/ng"),
                    'test',
                    `--karmaConfig="${vsKarmaConfigPath}"`
                ],
                {
                    cwd: projectPath,
                    shell: true,
                });

            // TODO: Handle multiple projects. aka. multiple spawns running.
            // const ngTest = spawn(
            //     'npx',
            //     ['ng', 'test', `--karmaConfig="${vsKarmaConfigPath}"`],
            //     {
            //         cwd: projectPath,
            //         shell: true,
            //     });

            let data = "";
            ngTest.stdout.on('data', (chunk) => {
                data += chunk
            });

            ngTest.stdout.on('end', () => {
                const fd = fs.openSync(discoverResultFile, 'w');
                fs.writeSync(fd, data);
                fs.closeSync(fd);

                resolve();
            });
        };
    });

}

const run_tests = function (context) {
    throw new Error('Not implemented');
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