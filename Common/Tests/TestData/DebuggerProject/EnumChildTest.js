var i = 0;
var apple = {
    kind: "macintosh",
    color: "red",
    states: ["Washington", "Oregon"],
    nested_object: { nested_string: "stringval", nested_number: 1, nested_boolean: true },
    getInfo: function () {
        return this.color + ' ' + this.kind + ' apple';
    }
};
// adfadfasdf
apple.nested_object.nested_boolean = false;
console.log(apple.getInfo());

console.log("done");

while (true) {
    i += 1;
}