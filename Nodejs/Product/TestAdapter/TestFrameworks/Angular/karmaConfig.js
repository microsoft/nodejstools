// @ts-check

const path = require('path');

const karmaConfigPath = path.resolve(process.env.PROJECTPATH, 'karma.conf.js');
const reporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'vsKarmaReporter.js');
const testCases = JSON.parse(process.env.TESTCASES);

const karmaConfig = require(karmaConfigPath);

module.exports = function (config) {
    karmaConfig(config);

    config.autoWatch = false;
    // config.browsers = ['ChromeHeadless'];
    config.browsers = ['Chrome'];
    config.logLevel = config.LOG_DISABLE;
    // Keep the original plugins
    config.plugins = config.plugins || [];
    config.plugins.push(require(reporterPath));
    // Replace all reporters
    config.reporters = ['vsKarmaReporter'];

    setGrep(config);
};

function setGrep(config) {
    // Search for an existing grep on clientArgs.
    const clientArgs = [];
    let hasGrep = false;
    for (let arg of config.client.args) {
        if (arg.substring(0, 6) === '--grep=') {
            hasGrep = true;
        }
        clientArgs.push(arg);
    }

    // If grep is not configured already, use VS configuration
    if (!hasGrep) {
        const testCasesRegex = testCases
            .reduce((previous, current) => {
                // TODO: Escape all of regex reserved characters.
                previous.push(current.fullTitle);
                return previous;
            }, [])
            .join('|');
        config.client.args.push(`--grep=${testCasesRegex}`);
    }
}