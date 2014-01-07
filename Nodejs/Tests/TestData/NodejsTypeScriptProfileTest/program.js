var Greeter = (function () {
    function Greeter(greeting) {
        this.greeting = greeting;
    }
    Greeter.prototype.greet = function () {
        return "<h1>" + this.greeting + "</h1>";
    };
    Greeter.prototype.f = function (n) {
        if (n == 0) {
            return (0);
        } else if (n == 1) {
            return (1);
        } else {
            return (this.f(n - 1) + this.f(n - 2));
        }
    };
    return Greeter;
})();
;

var greeter = new Greeter('hello');
for (var i = 0; i < 37; i++) {
    console.log(greeter.f(i));
}
//# sourceMappingURL=program.js.map
