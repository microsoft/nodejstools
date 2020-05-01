// @ts-check

const http = require('http');
const path = require('path');

const jasmineReporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'jasmineReporter.js');

const vsKarmaReporter = function (baseReporterDecorator, config, logger, emitter) {
    baseReporterDecorator(this);
    const log = logger.create('vsKarmaReporter');

    config.files.push({
        included: true,
        pattern: jasmineReporterPath,
        served: true,
        watched: false
    });

    const testList = [];

    // TODO: Is there a better option than onBrowserLog?
    this.onBrowserLog = (browser, browserLog, type) => {
        // TODO: handle status "Excluded";
        const cleaned = browserLog.substring(1, browserLog.length - 1); // Remove extra quote at start and end
        const result = JSON.parse(cleaned);

        const fullFilePath = `${process.env.PROJECTPATH}${result.fileLocation.relativeFilePath}`;
        const suite = result.fullName.substring(0, result.fullName.length - result.description.length - 1);

        let errorLog = "";
        for (const failedExpectation of result.failedExpectations) {
            errorLog += `\n\nMessage: ${failedExpectation.message}\nStack:\n${failedExpectation.stack}`;
        }

        // Handles both scenarios, discovery and execution.
        testList.push({
            // Discovery properties
            name: result.description,
            suite,
            filepath: fullFilePath,
            line: result.fileLocation.line,
            column: result.fileLocation.column,

            // Execution properties
            passed: result.status === "passed",
            pending: result.status === "disabled" || result.status === "pending",
            fullyQualifiedName: `${result.fileLocation.relativeFilePath}::${suite}::${result.description}`,
            stderr: errorLog
        });

        log.debug(`onBrowserLog: ${JSON.stringify(result)}`);
    }

    this.onRunComplete = (browsers, results) => {
        console.log(JSON.stringify(testList));

        log.debug(`onRunComplete: ${JSON.stringify(results)}`);

        // We need to exit the process as angular keeps listening, so it never emits an 'end' event.
        process.exit();
    }

    // Check when browser is ready to request a run.
    emitter.on("browsers_ready", () => {
        const options = {
            hostname: 'localhost',
            path: '/run',
            port: 9876, // Default karma port.
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
        };

        const request = http.request(options);
        request.end(
            JSON.stringify({ args: ['--grep="NTVS_AStringThatShouldNeverExists"'] })
        );
    });
}

vsKarmaReporter.$inject = ['baseReporterDecorator', 'config', 'logger', 'emitter'];

module.exports = {
    'reporter:vsKarmaReporter': ['type', vsKarmaReporter]
}