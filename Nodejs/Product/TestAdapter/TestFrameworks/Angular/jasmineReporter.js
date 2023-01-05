//@ts-check

let lookup = new Map();

var myReporter = {
    specDone: result => {
        result.fileLocation = lookup.get(result.id);

        // Communicate results through the console.
        console.log(JSON.stringify(result, getCircularReplacer()));
    },
};

// Function used to add fileLocation to the result. Includes filepath, line and column of the test.
function itReplacement(it) {
    return (description, testFunction, timeout) => {
        const specMetadata = it(description, testFunction, timeout);
        lookup.set(specMetadata.id, getFileLocation());
        return specMetadata;
    };
}

function getFileLocation() {
    const stackLineRegex = /at.*\(.*_karma_webpack_(?:\/webpack\:)?(.*\.spec\.ts):(\d*):(\d*)/;
    const match = (new Error()).stack.match(stackLineRegex);
    
    return match
        ? {
            relativeFilePath: match[1], // Relative to project root defined on angular.json.
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

jasmine.getEnv().addReporter(myReporter);
if (jasmine.getEnv().it_) {
    jasmine.getEnv().it_ = itReplacement(jasmine.getEnv().it_);
} else {
    jasmine.getEnv().it = itReplacement(jasmine.getEnv().it);
}