class jestReporter {
    constructor(globalConfig, options) {
        this._globalConfig = globalConfig;
        this._options = options;
    }

    onTestResult(test, testResult, aggregatedResult) {
        for (const assertionResult of testResult.testResults) {
            const result = {
                passed: assertionResult.status === 'passed',
                pending: assertionResult.status === 'pending',
                stderr: assertionResult.failureMessages.join('\n'),
                stdout: "",
                title: assertionResult.title
            };

            this._options.post({
                type: result.pending ? 'pending' : 'result',
                title: assertionResult.title,
                result: result
            });
        }

        this._options.post({
            type: 'suite end',
            result: {}
        });
    }
}

module.exports = jestReporter;