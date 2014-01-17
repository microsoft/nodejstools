/// <reference path="node.d.ts" />

declare module "express" {
    function createServer();

    function favicon(): any;
    function logger(s: string);
    function json();
    function urlencoded();
    function methodOverride();
    function static(s: string);

    function errorHandler();
}

declare module "stylus" {
    function middleware(dir: string);
}