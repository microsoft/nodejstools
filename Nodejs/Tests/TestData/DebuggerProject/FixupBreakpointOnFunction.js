// Set breakpoint on function below here
function A() { }
// Above breakpoint should bind on above line

// Set breakpoint on function below here
function B() {
}
// Above breakpoint should bind on above line

// Set breakpoint on function below here
function C() {
    // Above breakpoint should bind on next line
}

// Set breakpoint on function below here
function D(x, y) { return x + y; }
// Above breakpoint should bind on above line

// Set breakpoint on function below here
function E(x, y, z) {
    // Above breakpoint should bind on next line
    return x + y - z;
}

A();
B();
C();
D(1, 2);
E(1, 2, 3);
