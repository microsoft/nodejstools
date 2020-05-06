//@ts-check

function itReplacement(it) {
    return (description, testFunction, timeout) => {
        const spec = it(description, testFunction, timeout);
        spec.result.fileLocation = getFileLocation();

        return spec;
    };
}

function getFileLocation() {
    const stackLineRegex = /at.*\(.*_karma_webpack_(.*\.spec\.ts):(\d*):(\d*)/;
    const match = (new Error()).stack.match(stackLineRegex);

    return match
        ? {
            relativeFilePath: match[1], // Relative to root. This is due to webpack.
            line: match[2],
            column: match[3]
        }
        : null;
}

var myReporter = {
    specDone: result => {
        console.log(JSON.stringify(result));
    }
};

jasmine.getEnv().addReporter(myReporter);
jasmine.getEnv().it = itReplacement(jasmine.getEnv().it);