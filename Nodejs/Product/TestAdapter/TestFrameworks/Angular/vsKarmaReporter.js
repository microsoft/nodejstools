// @ts-check

const http = require('http');
const path = require('path');

const jasmineReporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'jasmineReporter.js');
const isDiscovery = process.env.ISDISCOVERY === 'true';
const project = JSON.parse(process.env.PROJECT);

const vsKarmaReporter = function (baseReporterDecorator, config, logger, emitter) {
    baseReporterDecorator(this);
    const log = logger.create('vsKarmaReporter');
    let testCaseCount = 0;

    process.on('message', () => {
        // We have recieved an indication that the test case has been processed. Decrement the amount
        testCaseCount--;
    });

    config.files.push({
        included: true,
        pattern: jasmineReporterPath,
        served: true,
        watched: false
    });

    this.onBrowserError = (browser, error) => {
        // TODO: Report error to user
        log.debug(`onBrowserError: ${JSON.stringify(error)}`);

        // If there's an error we want to clear the test cases so that we can finish the process.
        testCaseCount = 0;
    }

    // TODO: Is there a better option than onBrowserLog?
    // So far, since this is run by multiple out of proc, the only way I have found to communicate
    // is through the console, thus, the need for capturing the browser log. JasmineReporter uses 
    // console.log for this purpose.
    this.onBrowserLog = (browser, browserLog, type) => {
        const cleaned = browserLog.substring(1, browserLog.length - 1); // Remove extra quote at start and end
        const result = JSON.parse(cleaned);

        // If not discovering, ignore all excluded tests. Jasmine reports the test as excluded if it was not marked for execution.
        if (!isDiscovery && result.status === "excluded") {
            return;
        }

        // Increment the amount of test cases found.
        testCaseCount++;

        const fullFilePath = path.join(project.rootPath, result.fileLocation.relativeFilePath);
        const suite = result.fullName.substring(0, result.fullName.length - result.description.length - 1);

        let errorLog = "";
        for (const failedExpectation of result.failedExpectations) {
            errorLog += `${failedExpectation.stack}\n`;
        }

        // Handles both scenarios, discovery and execution.
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

        log.debug(`onBrowserLog: ${JSON.stringify(result)}`);
    }

    this.onRunComplete = async (browsers, results) => {
        log.debug(`onRunComplete: ${JSON.stringify(results)}`);

        // Wait until we have processed all of the test cases.
        while (testCaseCount > 0) {
            await sleep(1000);
        }

        // We need to exit the process as angular keeps it running and to emit the 'exit' event.
        process.exit();
    }

    // Override specFailure to avoid crashing the process as Karma sends a string output that cannot be parsed as JSON.
    this.specFailure = () => {
        // no-op
    }

    let hasStarted = false;

    // Check when browser is ready to request a run.
    emitter.on("browsers_ready", () => {
        // There's a scenario that I'm not sure how to repro where the browser do something (refresh? crashes?)
        // and we get the event again. We only want to executed it once.
        if (!hasStarted) {
            hasStarted = true;

            // At least one test case should exists.
            runTestCase();
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
        request.end();
    }

    async function sleep(ms) {
        return new Promise(resolve => {
            setTimeout(resolve, ms);
        });
    }
}

vsKarmaReporter.$inject = ['baseReporterDecorator', 'config', 'logger', 'emitter'];

module.exports = {
    'reporter:vsKarmaReporter': ['type', vsKarmaReporter]
}