var http = require('http');
var port = process.env.port || 1337;

var closedOver = 0;

http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');
    closedOver = 1;
}).listen(port);
