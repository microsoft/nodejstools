var framework;
var readline = require('readline');
var result = {
    "title": "",
    "passed": false,
    "stdOut": "",
    "stdErr": ""
};
var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    process.exit(1);
}

rl.on('line', (line) => {
    var data = JSON.parse(line);
    console.log(JSON.stringify(data));
    result.title = data.testName.replace(' ', '::');
    framework.run_tests(data.testName, data.testFile, data.workingFolder, data.projectFolder);
});

process.stdout.write = (function (write) {
    return function (string, encoding, fileDescriptor) {
        result.stdOut += string;
        write.apply(process.stdout, arguments);
    };
})(process.stdout.write);

process.stderr.write = (function (write) {
    return function (string, encoding, fileDescriptor) {
        result.stdErr += string;
        write.apply(process.stderr, arguments);
    };
})(process.stderr.write);

process.on('exit', (code) => {
    result.passed = code === 0 ? true : false;
    console.log(JSON.stringify(result));
    // clear stdOut and stdErr
    result.stdErr = "";
    result.stdOut = "";
});
