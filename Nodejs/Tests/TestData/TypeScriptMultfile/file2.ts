class class2 {
    myClass1: class1;
    constructor(param: number) {
        this.myClass1 = new class1(param);
    }

    myFunction()
    {
        return this.myClass1.function1();
    }
}

var x = new class2(3);
var z = x.myFunction();