declare module "http" {
    function createServer(requestListener: (request: ServerRequest, response: ServerResponse) => any): HttpServer;
}

declare module "path" {
    function join(a: string, b: string): string;
}

declare var process: NodejsProcess;
declare var exports: any;
declare var __dirname: string;

interface ServerRequest {
}

interface ServerResponse {
    writeHead(status: number, headers: any);
    end(body: string);
}

interface HttpServer {
    listen(port: number, started?: any);
}

interface NodejsProcess {
    env: any;
}