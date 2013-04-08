var parent_dup = require('./../dup.js');
parent_dup.top_level('dup.js')

var bar = require('bar');
bar.bar_entry('dup.js')

var parent_dup2 = require('../dup.js');
parent_dup2.top_level('dup.js')

exports.baz_dup = function f(arg) {
    console.log(arg, 'RequireTestApp.baz.dup.f()');
}
