// @ts-check
const path = require('path');

const reporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'vsKarmaReporter.js');
const testCases = JSON.parse(process.env.TESTCASES);
const karmaConfigPath = process.env.KARMACONFIGPATH;

module.exports = function (config) {
    const karmaConfig = require(karmaConfigPath);

    karmaConfig(config);

    config.autoWatch = false;
    config.browsers = ['ChromeHeadless'];
    config.logLevel = config.LOG_DISABLE;
    // Keep the original plugins
    config.plugins = config.plugins || [];
    config.plugins.push(require(reporterPath));
    // Replace all reporters
    config.reporters = ['vsKarmaReporter'];

    setGrep(config);
};

function setGrep(config) {
    // Remove any existing --grep argument
    config.client.args = config.client.args.filter(x => x.substring(0, 7) !== '--grep=');

    // Push custom grep.
    const testCasesRegex = testCases
        .reduce((previous, current) => {
            previous.push(escapeRegExp(current.fullTitle));
            return previous;
        }, [])
        .join('|');
    config.client.args.push(`--grep=/${testCasesRegex}/`);
}

function escapeRegExp(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); // $& means the whole matched string
}