var Greeter = (function () {
    function Greeter(greeting) {
        this.greeting = greeting;
        console.log('hi');
    }
    Greeter.prototype.greet = function () {
        return "<h1>" + this.greeting + "</h1>";
    };
    return Greeter;
})();
;

var greeter = new Greeter('hi');
greeter.greet();
//# sourceMappingURL=TypeScriptTest2.js.map
