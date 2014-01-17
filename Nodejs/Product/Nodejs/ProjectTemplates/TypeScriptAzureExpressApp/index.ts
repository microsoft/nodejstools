/// <reference path="../node.d.ts" />

/*
 * GET home page.
 */

export function index(req, res) {
    res.render('index', { title: 'Express' });
};