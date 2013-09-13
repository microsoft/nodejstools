function f1() {
    var i1 = 0;
}

function f2() {
    var i2 = 0;
    f1();
}

var i3 = 0; // Entrypoint
f2();

i3 = 1;
f2();

i3 = 2;
f2();

i3 = 3;
