function throwAndcatchException() {
    try {
        console.log('raising')
        throw new Error("msg")
    } catch (exception) {
        console.log('caught');
    }
}

function helper() {
    console.log('helper() begin');
    throwAndcatchException();
    throwAndcatchException();
    throwAndcatchException();
    console.log('helper() end');
}

helper();
console.log('done');
