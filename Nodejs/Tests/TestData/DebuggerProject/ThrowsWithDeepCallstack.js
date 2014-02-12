function recurse(i) {
    if (i > 0)
        recurse(--i);
    else {
        try {
            throw "exception";
        } catch(exception) {
        }
    }
}

recurse(1000);

console.log("end");