var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var _fruit = require('./TypeScriptInheritFruit');

var TypeScriptInheritApple = (function (_super) {
    __extends(TypeScriptInheritApple, _super);
    function TypeScriptInheritApple() {
        _super.call(this);
        console.log("Line before the breakpoint executed");
        console.log("Expected to stop in TS file");
    }
    return TypeScriptInheritApple;
})(_fruit.TypeScriptInheritFruit);
exports.TypeScriptInheritApple = TypeScriptInheritApple;
//# sourceMappingURL=TypeScriptInheritApple.js.map
