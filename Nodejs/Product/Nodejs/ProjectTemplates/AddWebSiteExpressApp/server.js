var express = require('express');
var app = express();

app.get('/', function (req, res) {
    res.send('Hello World');
});


var port = process.env.port || 1337;

app.listen(port);
console.log('Listening on port ' + port);