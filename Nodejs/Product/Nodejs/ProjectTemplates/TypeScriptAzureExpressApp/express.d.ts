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

declare module "stylus" {
    function middleware(dir: string);
}