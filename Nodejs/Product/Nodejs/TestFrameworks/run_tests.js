var framework;
var readline = require('readline');
var result = {
    "title": "",
    "passed": false,
    "stdOut": "",
    "stdErr": ""
};

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

var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.on('line', (line) => {
    var data = JSON.parse(line);
    result.title = data.testName.replace(' ', '::');
    // get rid of leftover quotations from C#
    for(var s in data) {
        data[s] = data[s].replace(/['"]+/g, '');
    }
    // run the test
    framework.run_tests(data.testName, data.testFile, data.workingFolder, data.projectFolder);
});

try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    process.exit(1);
}

process.on('exit', (code) => {
    result.passed = code === 0 ? true : false;
    console.log(JSON.stringify(result));
    // clear stdOut and stdErr
    result.stdErr = "";
    result.stdOut = "";
});