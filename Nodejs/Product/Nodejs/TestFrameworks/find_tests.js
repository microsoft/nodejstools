var framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
framework.find_tests(process.argv[3], process.argv[4], process.argv[5]);