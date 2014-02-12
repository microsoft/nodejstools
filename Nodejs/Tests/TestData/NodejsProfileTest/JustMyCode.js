var fs = require('fs');
for (var i = 0; i < 1000; i++) {
    fs.writeFileSync('foo.txt', 'data')
}
