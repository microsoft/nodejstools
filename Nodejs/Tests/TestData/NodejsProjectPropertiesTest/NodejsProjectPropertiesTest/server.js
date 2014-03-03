var fs = require('fs');
var result = "argv: " + process.argv + "\r\n" +
             "execArgv: " + process.execArgv + "\r\n" +
             "port: " + process.env.port + "\r\n" +
             "cwd: " + process.cwd();

fs.writeFile(process.env.temp + '\\nodejstest.txt', result, function (err) {
  if (err) throw err;
  console.log('It\'s saved!');
});
