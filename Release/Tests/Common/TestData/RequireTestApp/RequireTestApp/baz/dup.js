var parent_dup = require('./../dup.js');
parent_dup.top_level('dup.js')

exports.baz_dup = function f(arg) {
    console.log(arg, 'RequireTestApp.baz.dup.f()');
}
