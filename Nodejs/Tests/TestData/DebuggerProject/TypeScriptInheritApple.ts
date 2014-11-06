import _fruit = require('./TypeScriptInheritFruit');

export class TypeScriptInheritApple extends _fruit.TypeScriptInheritFruit {
    constructor() {
        super();
        console.log("Line before the breakpoint executed");
        console.log("Expected to stop in TS file");
    }
}
