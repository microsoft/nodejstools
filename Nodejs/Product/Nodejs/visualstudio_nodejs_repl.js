/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

if (process.argv.length <= 2) {
    console.log('Repl requires port number to connect to');
    process.exit(1);
}

var path = require('path');

// setup our filename and search paths to match how they would if the user
// started inside of their own project...  When the REPL starts our CWD
// is set to the project home.
module.filename = path.join(process.cwd(), 'server.js');
module.paths = []
var curDir = process.cwd();
for (; ;) {
    module.paths.push(path.join(curDir, "node_modules"));
    var nextDir = path.join(curDir, '..');
    if (nextDir == curDir) {
        break;
    }
    curDir = nextDir;
}


function send_response(socket, data) {
    var str = JSON.stringify(data);
    var buf = Buffer(str);
    socket.write('Content-length: ' + buf.length + '\r\n\r\n');
    socket.write(buf);
}

function findEndOfHeaders(buf) {
    for (var i = 0; i < buf.length - 4; i++) {
        if (buf[i] == 13 &&
            buf[i + 1] == 10 &&
            buf[i + 2] == 13 &&
            buf[i + 3] == 10) {
            return i;
        }
    }
    return -1;
}


var net = require("net");

var client = net.connect(+process.argv[2], '127.0.0.1', function () { });

function stdstream(isError) {
    this.writable = true;
    this.readable = false;

    this.write = function (string, encoding) {
        if (typeof string != 'string') {
            string = string.toString();
        }
        var type = isError ? 'output_error' : 'output';
        send_response(client, { 'type': type, 'output': string });
    }

    this.destroy = this.destroySoon = this.end = function () {
        throw "process.stdout cannot be closed.";
    }
}

// need to delete so we can replace...
delete process.stdout;
delete process.stderr;
process.stdout = new stdstream(false);
process.stderr = new stdstream(true);

var util = require('util');
var outerGlobal = this;
var vm = require('vm');

var context = global;
context.require = require;
context.module = module;

util.inspect.styles['number'] = 'blue';
util.inspect.styles['boolean'] = 'blue';
util.inspect.colors['blue'] = [94, 39];

function processRequest(command) {
    switch (command["type"]) {
        case "execute":
            // eval against the global object, in node this is process
            try {
                var obj;
                // function f() { } should return undefined, wrapping it in parens
                // produces a value, so ignore it if we have a function definition.
                if (!/^\s*function/.test(command["code"])) {
                    try {
                        // object literals are ambigious, so first try w/ parens.
                        obj = eval.call(context, '(' + command["code"] + ')');
                    } catch (err) {
                        // fallback to normal code if we can't parse this.
                        if (err.toString().substr(0, 12) == 'SyntaxError:') {
                            obj = eval.call(context, command["code"]);
                        } else {
                            throw err;
                        }
                    }
                } else {
                    obj = eval.call(context, command["code"]);
                }
                
                var result = util.inspect(obj, undefined, undefined, true);
                send_response(client, { 'type': 'execute', 'result': result });
            } catch (err) {
                if (err === null || err == undefined) {
                    var result = util.inspect(undefined, undefined, undefined, true);
                    send_response(client, { 'type': 'execute', 'result': result });
                } else {
                    send_response(client, { 'type': 'execute', 'error': err.toString() });
                }

            }
            break;
        case "clear":
            context = {};
            break;
        default:
            console.log("Unknown command: " + command["type"]);
            break;
    }
}

reader_state = { 'prevData': Buffer(0), 'state': 'header' }

client.on('data', function (data) {
    try {
        switch (reader_state['state']) {
            case "header":
                // we're reading the header...
                var incoming = Buffer.concat([reader_state['prevData'], data]);
                var endOfHeaders = findEndOfHeaders(incoming);

                if (endOfHeaders != -1) {
                    // we've read all of the headers
                    var header = incoming.slice(0, endOfHeaders).toString();
                    var headerNameEnd = header.indexOf(':');
                    var contentLength = undefined;
                    if (headerNameEnd != -1) {
                        var headerName = header.substring(0, headerNameEnd);
                        if (headerName == 'Content-length') {
                            contentLength = +header.substring(headerNameEnd + 1);
                        }
                    }

                    if (contentLength == undefined) {
                        // currently we only support/expect the content length header
                        throw 'expected Content-Length header, got ' + header;
                    }

                    data = Buffer(0);
                    reader_state['prevData'] = incoming.slice(endOfHeaders + 4, incoming.length);
                    reader_state['state'] = 'body';
                    reader_state['contentLength'] = contentLength;

                    if (reader_state['prevData'].length < contentLength) {// <-+
                        break;                                        //       |
                    }                                                 //       |
                } else {                                              //       |
                    reader_state['prevData'] = incoming;              //       |
                    break;                                            //       |
                }                                                     //       |
                // we fall through to body if there's enough data...     ------+
            case "body":
                // we're reading the body
                var body = reader_state['prevData'] = Buffer.concat([reader_state['prevData'], data]);
                if (body.length >= reader_state['contentLength']) {
                    var payload = body.slice(0, reader_state['contentLength']);
                    reader_state['prevData'] = body.slice(reader_state['contentLength'], body.length);
                    reader_state['state'] = 'header';
                    processRequest(JSON.parse(payload.toString()));
                } // otherwise keep reading...
                break;
            default:
                console.log('unknown state');
                break;
        }
    } catch (e) {
        console.log(e);
        while (true) {
        }
    }
});

client.on('end', function () {
    process.exit(1);

});

client.on('error', function (e) {
    console.log(e);
    process.exit(1);

});
