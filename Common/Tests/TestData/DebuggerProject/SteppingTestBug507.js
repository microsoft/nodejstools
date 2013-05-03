function add_two_numbers(x, y) {
    return x + y;
}

function Z() {
    this.text = "Z";
}
Z.prototype.foo = function () {
    return 7;
};

var p = new Z();
var val = add_two_numbers(p.foo(), 3);
console.log(val);

console.log("Done");