// Well known exceptions types
try {
    console.log('raising')
    throw new Error("msg")
} catch(exception) {
    console.log('caught');
}
try {
    console.log('raising')
    var x = UndefinedVariable;  // ReferenceError
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    var UserDefinedArray = new Array(-1);   // RangeError
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    var x = "UserStringValue"; x.UndefinedFunction();   // TypeError
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    var x = decodeURIComponent("%");    // URIError
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    var x = eval("/");  // SyntaxError
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    throw new EvalError("msg");
} catch (exception) {
    console.log('caught');
}

// User defined exception types
function UserDefinedError(message) {
    this.name = 'UserDefinedError';
    this.message = message || 'A UserDefinedError ocurred!';
}
UserDefinedError.prototype = Error.prototype;
UserDefinedError.prototype.constructor = UserDefinedError;
try {
    console.log('raising')
    throw new UserDefinedError("msg");
} catch (exception) {
    console.log('caught');
}
function UserDefinedRangeError(message) {
    this.name = 'UserDefinedRangeError';
    this.message = message || 'A UserDefinedRangeError ocurred!';
}
UserDefinedRangeError.prototype = RangeError.prototype;
UserDefinedRangeError.prototype.constructor = UserDefinedRangeError;
try {
    console.log('raising')
    throw new UserDefinedRangeError("msg");
} catch (exception) {
    console.log('caught');
}
function UserDefinedType() {
}
try {
    console.log('raising')
    throw new UserDefinedType();
} catch (exception) {
    console.log('caught');
}


// Primitive types as exception types
try {
    console.log('raising')
    throw 1;
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    throw "exception_string";
} catch (exception) {
    console.log('caught');
}
try {
    console.log('raising')
    throw false;
} catch (exception) {
    console.log('caught');
}
