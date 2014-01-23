var Greeter = (function () {
    function Greeter(greeting) {
        this.greeting = greeting;
    }
    Greeter.prototype.greet = function () {
        return "<h1>" + this.greeting + "</h1>";
    };
    return Greeter;
})();
;

var greeter = new Greeter('hi');
greeter.greet();
//# sourceMappingURL=TypeScriptTest.js.map
