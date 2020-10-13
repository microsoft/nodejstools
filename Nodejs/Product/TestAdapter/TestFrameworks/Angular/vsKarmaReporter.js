// @ts-check

const http = require('http');
const path = require('path');

const jasmineReporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'jasmineReporter.js');
const testCases = JSON.parse(process.env.TESTCASES);
const isDiscovery = process.env.ISDISCOVERY === 'true';

const vsKarmaReporter = function (baseReporterDecorator, config, logger, emitter) {
    baseReporterDecorator(this);
    const log = logger.create('vsKarmaReporter');

    config.files.push({
        included: true,
        pattern: jasmineReporterPath,
        served: true,
        watched: false
    });

    this.onBrowserError = (browser, error) => {
        log.debug(`onBrowserError: ${JSON.stringify(error)}`);
    }

    // TODO: Is there a better option than onBrowserLog?
    // So far, since this is runned by multiple out of proc, the only way I have found to communicate
    // is through the console, thus, the need for capturing the browser log. JasmineReporter uses 
    // console.log for this purpose.
    this.onBrowserLog = (browser, browserLog, type) => {
        const cleaned = browserLog.substring(1, browserLog.length - 1); // Remove extra quote at start and end
        const result = JSON.parse(cleaned);

        // If not discovering, ignore all excluded tests. Jasmine reports the test as excluded if it was not marked for execution.
        if (!isDiscovery && result.status === "excluded") {
            return;
        }

        const fullFilePath = `${process.env.PROJECTPATH}${result.fileLocation.relativeFilePath}`;
        const suite = result.fullName.substring(0, result.fullName.length - result.description.length - 1);

        let errorLog = "";
        for (const failedExpectation of result.failedExpectations) {
            errorLog += `${failedExpectation.stack}\n`;
        }

        // Handles both scenarios, discovery and execution.
        try {
            process.send({
                // Discovery properties
                name: result.description,
                suite,
                filepath: fullFilePath,
                line: result.fileLocation.line,
                column: result.fileLocation.column,

                // Execution properties
                passed: result.status === "passed",
                pending: result.status === "disabled" || result.status === "pending",
                fullName: result.fullName,
                stderr: errorLog
            });
        } catch (e) {
            log.debug(`error: ${JSON.stringify(e)}`);
        }

        log.debug(`onBrowserLog: ${JSON.stringify(result)}`);
    }

    this.onRunComplete = (browsers, results) => {
        log.debug(`onRunComplete: ${JSON.stringify(results)}`);

        const testCase = testCases.pop();
        if (testCase) {
            runTestCase(testCase);
        } else {
            // We need to exit the process as angular keeps it running and to emit the 'exit' event.
            process.exit();
        }
    }

    // Override specFailure to avoid crashing the process as Karma sends a string output that cannot be parsed as JSON.
    this.specFailure = () => { }

    let hasStarted = false;

    // Check when browser is ready to request a run.
    emitter.on("browsers_ready", () => {
        // There's a scenario that I'm not sure how to repro where the browser do something (refresh? crashes?)
        // and we get the event again. We only want to executed it once.
        if (!hasStarted) {
            hasStarted = true;

            // At least one test case should exists.
            runTestCase(testCases.pop());
        }
    });

    function runTestCase(testCase) {
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
            JSON.stringify({ args: [`--grep=${testCase.fullTitle}`] })
        );
    }
}

vsKarmaReporter.$inject = ['baseReporterDecorator', 'config', 'logger', 'emitter'];

module.exports = {
    'reporter:vsKarmaReporter': ['type', vsKarmaReporter]
}