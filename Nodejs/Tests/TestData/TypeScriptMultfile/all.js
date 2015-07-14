var Greeter = (function () {
    function Greeter() {
    }
    Greeter.prototype.start = function () {
    };
    Greeter.prototype.stop = function () {
    };
    return Greeter;
})();
var class1 = (function () {
    function class1(param1) {
        this.var1 = param1;
    }
    class1.prototype.function1 = function () {
        return this.var1;
    };
    return class1;
})();
var class2 = (function () {
    function class2(param) {
        this.myClass1 = new class1(param);
    }
    class2.prototype.myFunction = function () {
        return this.myClass1.function1();
    };
    return class2;
})();
var x = new class2(3);
var z = x.myFunction();
//# sourceMappingURL=all.js.map