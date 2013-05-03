var http = require('http');

var port = process.env.port || 1337;
var mymod = require('./mymod.js');
var mutatemod = require('./mutatemod.js');


http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
}).listen(port);
