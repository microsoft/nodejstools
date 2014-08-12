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

var net = require('net');
var spawn = require('child_process').spawn;

var localHost = 'localhost';
var remoteHost = process.env.COMPUTERNAME;

var localClientPort = 5859;
var remoteServerPort = 5858;

var startDebuggeeOnRemoteConnect = false;
var startDebuggeeBrokenAtEntryPoint = false;

var node = null;
var scriptToDebug = null;
var passThroughArgs = null;

var debuggee = null;
var localClientSocket = null;
var remoteServerSocket = null;

function parseCommandLine() {
    var error = false;
    node = process.argv[0];
    var i;
    for (i = 2; i < process.argv.length; ++i) {
        var arg = process.argv[i];
        var nextArg = process.argv[i + 1];
        if (arg.substring(0, 1) != '-') {
            break;
        }
        var argname = arg.substring(1).toLowerCase();
        switch (argname) {
            case ('localport'): {
                localClientPort = parseInt(nextArg);
                ++i;
                break;
            }
            case ('machineport'): {
                ++i;
                remoteServerPort = parseInt(nextArg);
                break;
            }
            case ('waitforattach'): {
                startDebuggeeOnRemoteConnect = true;
                break;
            }
            case ('breakatentrypoint'): {
                startDebuggeeBrokenAtEntryPoint = true;
                break;
            }
            default: {  // Covers -help and -h
                error = true;
                break;
            }
        }
        if (error) {
            break;
        }
    }

    if (i >= process.argv.length) {
        error = true;
    }

    if (error) {
        console.log('Remote Debug Proxy');
        console.log('    Runs a given nodejs script in debug mode, exposing the debugging protocol');
        console.log('    over a machine port.');
        console.log('Usage:');
        console.log('    Node RemoteDebug.js [args] <script to debug> [script args]');
        console.log('Args:');
        console.log('    -localport <port num>   - Local host port used by node to expose debugging');
        console.log('                              protocol (defaults to 5859)');
        console.log('    -machineport <port num> - Proxied machine port for use by remote debugger');
        console.log('                              (defaults to 5858)');
        console.log('    -waitforattach          - Wait until remote debugger attach before');
        console.log('                              starting node running/debugging script');
        console.log('    -breakatentrypoint      - Break at entrypoint when starting node');
        console.log('                              running/debugging script');
        console.log('    -help                   - Show this help text');
        process.exit();
    }

    scriptToDebug = process.argv[i];
    passThroughArgs = process.argv.slice(i + 1);
}

function ensureDebuggeeStarted() {
    if (debuggee == null && (!startDebuggeeOnRemoteConnect || remoteServerSocket)) {
        var debugArg = '--debug';
        if (startDebuggeeBrokenAtEntryPoint) {
            debugArg = debugArg + '-brk';
        }
        var debugArg = debugArg + '=' + localClientPort;
        var spawnArgs = [debugArg, scriptToDebug];
        spawnArgs = spawnArgs.concat(passThroughArgs);
        debuggee = spawn(node, spawnArgs, [], { stdio: 'inherit' });
        debuggee.stdout.on('data', function (data) {
            process.stdout.write(data);
        });
        debuggee.stderr.on('data', function (data) {
            process.stderr.write(data);
        });
        debuggee.on('exit', function (code) {
            process.exit();
        });
        debuggee.on('SIGTERM', function (code) {
            process.exit();
        });
        process.on('SIGTERM', function (code) {
            debuggee.exit();
            debuggee = null;
        });
        console.log('Debuggee started: ' + node + ' ' + debugArg + ' ' + scriptToDebug + ' ' + passThroughArgs);
    }
}

function ensureLocalClientSocketConnected() {
    ensureDebuggeeStarted();
    if (localClientSocket == null) {
        localClientSocket = new net.Socket();
        localClientSocket.on('data', function (data) {
            if (remoteServerSocket != null) {
                remoteServerSocket.write(data);
            }
        });
        localClientSocket.on('close', function () {
            console.log('localClientSocket disconnected');
            localClientSocket = null;
        });
        localClientSocket.connect(localClientPort, localHost, function () {
            console.log('localClientSocket connected');
        });
    }
}

// Parse command line
parseCommandLine();
console.log('node: ' + node);
console.log('scriptToDebug: ' + scriptToDebug);
console.log('passThroughArgs: ' + passThroughArgs);
console.log('localClientPort: ' + localHost + ':' + localClientPort);
console.log('remoteServerPort: ' + remoteHost + ':' + remoteServerPort);
console.log('startDebuggeeOnRemoteConnect: ' + startDebuggeeOnRemoteConnect);
console.log('startDebuggeeBrokenAtEntryPoint: ' + startDebuggeeBrokenAtEntryPoint);

// Start remote server listening for connection
var remoteServer = net.createServer(function (socket) {
    // Service only one remote server connection at a time
    if (remoteServerSocket != null) {
        console.log('Remote connection rejected');
    }

    // Wire up remote server socket
    remoteServerSocket = socket
    remoteServerSocket.on('data', function (data) {
        if (localClientSocket != null) {
            localClientSocket.write(data);
        }
    });
    remoteServerSocket.on('close', function (data) {
        console.log('remoteServerSocket disconnected');
        remoteServerSocket = null;
    });
    console.log('remoteServerSocket connected');

    // Ensure local client socket connected
    ensureLocalClientSocketConnected();
});
remoteServer.listen(remoteServerPort, remoteHost);
console.log('remoteServerSocket listening for connection');

// Ensure debuggee started (if not waiting for remote server connection)
ensureDebuggeeStarted();

