var http = require('http');
var port = process.env.port || 1337;
var fs = require('fs');

http.createServer(function (req, res) {
    res.writeHead(200, { 'Content-Type': 'text/plain' });
    res.end('Hello World\n');

    fs.writeFile(process.env.temp + '\\nodejstest.txt', "request successful", function (err) {
      if (err) throw err;
      console.log('It\'s saved!');
      process.exit(0);
    });

}).listen(port);