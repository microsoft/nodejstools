var framework;
var readline = require('readline');
var old_stdout = process.stdout.write;
var old_stderr = process.stderr.write;
var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

rl.on('line', function (line) {
    rl.close();

    // strip the BOM in case of UTF-8
    if (line.charCodeAt(0) === 0xFEFF) {
        line = line.slice(1);
    }

    var testCases = JSON.parse(line);
    // get rid of leftover quotations from C# (necessary?)
    for (var test in testCases) {
        for (var value in testCases[test]) {
            testCases[test][value] = testCases[test][value].replace(/["]+/g, '');
        }
    }

    try {
        framework = require('./' + testCases[0].framework + '/' + testCases[0].framework + '.js');
    } catch (exception) {
        console.log("NTVS_ERROR:Failed to load TestFramework (" + testCases[0].framework + "), " + exception);
        process.exit(1);
    }

    function postResult(result) {
        // unhook stdout and stderr
        process.stdout.write = old_stdout;
        process.stderr.write = old_stderr;
        if (result) {
            console.log(JSON.stringify(result));
        }
    }
    // run the test
    framework.run_tests(testCases, postResult);
});
