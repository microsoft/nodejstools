var typings = require('typings-core');

/**
 * Create a promise that runs one or more promises sequentially.
 */
module.exports.runAll = function (promises) {
    return promises.reduce(function (p1, p2) {
        return p1.then(function () {
            return p2;
        });
    });
};

/**
 * Installs the typings for `packageName` with `options`.
 */
module.exports.installTypingsForPackage = function (packageName, options) {
    return typings.installDependenciesRaw(["dt~" + packageName], options)
        .then(function () {
            console.log("Acquired typings for '" + packageName + "'");
        })
        .catch(function (e) {
            console.error("Could not acquire typings for '" + packageName + "'");
        });
};

/**
 * Installs the typings for the current project.
 */
module.exports.installTypingsForProject = function(options) {
    return typings.install(options)
        .then(function () {
            console.log("Acquired typings for project");
        })
        .catch(function (e) {
            console.error("Could not acquire typings for project");
        });
};
