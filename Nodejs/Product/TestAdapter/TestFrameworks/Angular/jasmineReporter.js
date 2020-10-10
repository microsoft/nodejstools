//@ts-check

// Function used to add fileLocation to the result. Includes filepath, line and column of the test.
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

// Example provided by MDN. Caused data loss by removing the circular reference.
const getCircularReplacer = () => {
    const seen = new WeakSet();

    return (key, value) => {
        if (isObject(value)) {
            if (seen.has(value)) {
                return;
            }
            seen.add(value);
        }

        return value;
    };
};

function isObject(value) {
    return typeof value === "object"
        && value !== null
        && !(value instanceof Boolean)
        && !(value instanceof Date)
        && !(value instanceof Number)
        && !(value instanceof RegExp)
        && !(value instanceof String)
}

var myReporter = {
    specDone: result => {
        // Communicate results through the console.
        console.log(JSON.stringify(result, getCircularReplacer()));
    }
};

jasmine.getEnv().addReporter(myReporter);
jasmine.getEnv().it = itReplacement(jasmine.getEnv().it);

