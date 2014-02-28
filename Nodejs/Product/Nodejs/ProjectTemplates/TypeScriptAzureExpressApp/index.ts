/*
 * GET home page.
 */

export function index(req: ServerRequest, res: ExpressResponse) {
    res.render('index', { title: 'Express' });
};