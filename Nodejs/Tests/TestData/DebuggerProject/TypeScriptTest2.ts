class Greeter {
    constructor(public greeting: string) { 
      console.log('hi');
    }
    greet() {
        return "<h1>" + this.greeting + "</h1>";
    }
};

var greeter = new Greeter('hi');
greeter.greet();
