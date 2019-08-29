//@ts-check
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
                fullyQualifiedName: this._options.context.getFullyQualifiedName(assertionResult.fullName)
            };

            this._options.context.post({
                type: result.pending ? 'pending' : 'result',
                fullyQualifiedName: result.fullyQualifiedName,
                result
            });
        }

        this._options.context.post({
            type: 'end'
        });
    }
}

module.exports = jestReporter;