//@ts-check
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

    const context = createContext(line);

    // get rid of leftover quotations from C# (necessary?)
    for (var test in context.testCases) {
        for (var value in context.testCases[test]) {
            context.testCases[test][value] = context.testCases[test][value].replace(/["]+/g, '');
        }
    }

    try {
        framework = require('./' + context.testCases[0].framework + '/' + context.testCases[0].framework + '.js');
    } catch (exception) {
        console.log("NTVS_ERROR:Failed to load TestFramework (" + context.testCases[0].framework + "), " + exception);
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
    framework.run_tests(context, postResult);
});


function createContext(line) {
    function setFullTitle(testCases) {
        // FullyQualifiedName looks like `<filepath>::<suite><subSuite>::<testName>`.
        // <suite> will be `global` for all tests on the "global" scope.
        const cleanRegex = /.*?::(global::)?/;

        for (let testCase of testCases) {
            testCase.fullTitle = testCase.fullyQualifiedName.replace(cleanRegex, "").replace("::", " ");
        }
    }

    function getFullyQualifiedName(testCases, fullTitle) {
        for (let testCase of testCases) {
            if (testCase.fullTitle === fullTitle) {
                return testCase.fullyQualifiedName;
            }
        }
    }

    let testCases = JSON.parse(line);
    setFullTitle(testCases);

    return {
        testCases: testCases,
        getFullyQualifiedName: (fullTitle) => getFullyQualifiedName(testCases, fullTitle)
    };
}