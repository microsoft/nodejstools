function triangular_number(a) { return a > 0 ? a + triangular_number(a - 1) : 0; }

var val = triangular_number(3);
console.log(val);
