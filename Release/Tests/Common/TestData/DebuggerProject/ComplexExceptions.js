// raise an exception that's not a built-in
function UserDefinedClass() {
}
try {
    console.log('raising')
    throw new UserDefinedClass();
} catch(exception) {
    console.log('caught');
}

try {
    console.log('raising')
    try {
        throw new TypeError('TypeError description');
    }
    finally {
        throw new ReferenceError('ReferenceError description');
    }
} catch(exception) {
    console.log('caught');
}

try {
    try {
        throw new TypeError('TypeError description');
    }
    catch(exception) {
        throw new ReferenceError('ReferenceError description');
    }
} catch(exception) {
}

function g() {
    function f() {
        throw new Error('Error description');
    }

    try {
        f();
    } catch(exception) {
        console.log('line 41');
        console.log('line 42');
    }
    console.log('after exception here');
}

g();

console.log('ex done');
console.log('no, really, its done');