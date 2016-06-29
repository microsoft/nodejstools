var express = require('express');
var router = express.Router();

router.get('/', function (req, res) {
    res.render('index', { title: 'Express', year: new Date().getFullYear() });
});

router.get('/about', function (req, res) {
    res.render('about', { title: 'About', year: new Date().getFullYear(), message: 'Your application description page' });
});

router.get('/contact', function (req, res) {
    res.render('contact', { title: 'Contact', year: new Date().getFullYear(), message: 'Your contact page' });
});

module.exports = router;