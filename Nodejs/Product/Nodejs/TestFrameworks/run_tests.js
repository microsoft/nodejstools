var framework = require('./' + process.argv[2] + '/' + process.argv[2] + '.js');
framework.run_tests(process.argv[3], process.argv[4], process.argv[5], process.argv[6]);