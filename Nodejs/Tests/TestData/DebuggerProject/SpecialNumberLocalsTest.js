function SpecialNumberLocals() {
    var posInf = +1 / 0;
    var negInf = -1 / 0;
    var nan = 0 / 0;
    var nul = null;
    console.log("SpecialNumberLocals() done");
}

SpecialNumberLocals();

console.log("done");