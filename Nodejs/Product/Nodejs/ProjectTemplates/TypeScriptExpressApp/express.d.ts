/// <reference path="node.d.ts" />

declare module "express" {
    function exports(): exports.Express;

    module exports {
        export function favicon(): any;
        export function logger(s: string);
        export function json();
        export function urlencoded();
        export function methodOverride();
        export function static(s: string);

        export function errorHandler();
        export function createServer();

        export interface Express {
            get(name: string, handler?: any);
            set(name: string, value: any);
            use(value: any);
            router: any;
            (request: ServerRequest, response: ServerResponse);
        }
    }

    export = exports;
}

interface ExpressResponse {
    status(code: number): ExpressResponse;
    header(field: string, value: any);
    set(field: string, value: any);
    get(field: string): string;
    cookie(name: string, value: string, options?);
    clearCookie(name: string, options?);
    redirect(status: number, url: string);
    location(url:string);
    redirect(url:string);
    send(body: string);
    send(status: number, body:string);
    render(view: string, callback?: (err, html: string) => any);
    json(body: string);
    json(status: number, body:string);
    jsonp(body: string);
    jsonp(status: number, body:string);
    type(type:string);
    attachment(filename?:string);
    format(object);
    sendfile(path: string, options?, fn?);
    download(path: string, filename?: string, fn?);
    links(links);
    render(view: string, locals?: any, callback? : (err, html:string) => any);
}

declare module "stylus" {
    function middleware(dir: string);
}