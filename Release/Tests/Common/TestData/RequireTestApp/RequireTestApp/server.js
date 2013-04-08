console.log('-- RequireTestApp.server.js')
var mymod = require('mymod');
mymod.mymod_export(); // mymod.f()


var mymod2 = require('mymod.js');
mymod2.mymod_export();  // mymod.f()


try {
    var mymod3 = require('./mymod.js');

} catch (err) {
    console.log('./mymod.js fails');
}

var foo = require('foo');
foo.foo_export('server.js');

var bar = require('bar');
bar.bar_entry('server.js')

var bar2 = require('bar2');
bar2.bar2_entry('server.js')

var dup = require('dup');
dup.node_modules_dup('server.js');// RequireTestApp.node_modules.dup.f()

var dup1 = require('./dup');
dup1.top_level('server.js');  // RequireTestApp.dup.f()

var dup2 = require('./dup.js');
dup2.top_level('server.js');    

var baz_dup = require('./baz/dup.js');
baz_dup.baz_dup('server.js')

var baz_dup2 = require('./baz/dup');
baz_dup2.baz_dup('server.js')

var recursive = require('./recursive1.js');

exports.bar = function abc() {
};