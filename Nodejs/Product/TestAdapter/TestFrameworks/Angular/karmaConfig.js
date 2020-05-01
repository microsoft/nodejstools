// @ts-check

const path = require('path');

const karmaConfigPath = path.resolve(process.env.PROJECTPATH, 'karma.conf.js');
const reporterPath = path.resolve(process.env.VSTESTADAPTERPATH, 'vsKarmaReporter.js');

const karmaConfig = require(karmaConfigPath);

module.exports = function (config) {
    karmaConfig(config);

    // config.browsers = ['ChromeHeadless'];
    config.browsers = ['Chrome'];
    config.autoWatch = false;
    config.logLevel = config.LOG_DISABLE;
    // Keep the original plugins
    config.plugins = config.plugins || [];
    config.plugins.push(require(reporterPath));
    // Replace all reportrs.
    config.reporters = ['vsKarmaReporter'];
};
