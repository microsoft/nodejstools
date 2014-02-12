console.log("start");

function func1(x) {
    return x;
}

function recurse(i) {
    if (i > 0)
        recurse(--i);
    else {
        console.log("Deepest recusion");
        func1(3);
    }
}

recurse(1000);

console.log("end");
