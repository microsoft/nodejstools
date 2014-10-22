function functionObject() {
    console.log('Function object')
    var nestedFunctionExpression = function () {
        console.log('Nested function expression')
        function nestedFunctionObject() {
            console.log('Nested function object');
        }
    }
}