// @ts-check
const path = require('path');

const reporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'vsKarmaReporter.js');
const testCases = JSON.parse(process.env.TESTCASES);
const project = JSON.parse(process.env.PROJECT);

module.exports = function (config) {
    const karmaConfig = require(project.karmaConfigFilePath);

    karmaConfig(config);

    config.autoWatch = false;
    config.browsers = ['ChromeHeadless'];
    config.logLevel = config.LOG_DISABLE;
    // Keep the original plugins
    config.plugins = config.plugins || [];
    config.plugins.push(require(reporterPath));
    // Replace all reporters
    config.reporters = ['vsKarmaReporter'];

    if (!config.hostname && !config.listenAddress) {
        // The default address is IPv4 but node 17+ uses IPv6. Update the value to 'localhost' (which works with both IPv4 and IPv6) if it was not set.
        config.hostname = config.listenAddress = 'localhost';
    }

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