/// <reference path="../node.d.ts" />

/*
 * GET home page.
 */

export function index(req: ServerRequest, res: ServerResponse) {
    res.render('index', { title: 'Express' });
};