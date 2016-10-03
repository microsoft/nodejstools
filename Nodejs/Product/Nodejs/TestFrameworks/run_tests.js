var framework;
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

try {
    framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
} catch (exception) {
    console.log("NTVS_ERROR:Failed to load TestFramework (" + process.argv[2] + "), " + exception);
    process.exit(1);
}

process.on('exit', (code) => {
    result.passed = code === 0 ? true : false;
    console.log(JSON.stringify(result));
});

result.title = process.argv[3].replace(' ', '::');

framework.run_tests(process.argv[3], process.argv[4], process.argv[5], process.argv[6]);
