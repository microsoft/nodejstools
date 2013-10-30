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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Script.Serialization;

namespace Microsoft.NodejsTools.Debugger {
    /// <summary>
    /// Defines a node debugger connection.
    /// </summary>
    class NodeConnection : JsonListener, INodeConnection {
        private readonly string _hostName;
        private readonly ushort _portNumber;
        private readonly Dictionary<int, object> _requestData = new Dictionary<int, object>();
        private readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();
        private int _currentRequestSequence = 1;

        public NodeConnection() : this("localhost", 5858) {
        }

        public NodeConnection(string hostName, ushort portNumber) {
            _hostName = hostName;
            _portNumber = portNumber;
        }

        /// <summary>
        /// Gets a value indicating whether connection established.
        /// </summary>
        public bool Connected {
            get { return Socket != null && Socket.Connected; }
        }

        public event EventHandler<EventArgs> SocketDisconnected;
        public event EventHandler<NodeEventEventArgs> NodeEvent;

        /// <summary>
        /// Connects to the node debugger.
        /// </summary>
        public void Connect() {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.NoDelay = true;
            Socket.Connect(new DnsEndPoint(_hostName, _portNumber));

            StartListenerThread();
        }

        /// <summary>
        /// Disconnects from the node debugger.
        /// </summary>
        public void Disconnect() {
            if (Socket != null && Socket.Connected) {
                Socket.Disconnect(false);
            }
            Socket = null;
        }

        protected override void OnSocketDisconnected() {
            EventHandler<EventArgs> socketDisconnected = SocketDisconnected;
            if (socketDisconnected != null) {
                socketDisconnected(this, EventArgs.Empty);
            }
        }

        protected override void ProcessPacket(JsonResponse response) {
            Debug.WriteLine("Headers:");

            foreach (var keyValue in response.Headers) {
                Debug.WriteLine("{0}: {1}", keyValue.Key, keyValue.Value);
            }

            Debug.WriteLine(String.Format("Body: {0}", string.IsNullOrEmpty(response.Body) ? string.Empty : response.Body));

            if (response.Headers.ContainsKey("type")) {
                switch (response.Headers["type"]) {
                    case "connect":
                        // No-op, as ProcessConnect() is called on the main thread
                        break;
                    default:
                        Debug.WriteLine(String.Format("Unknown header type: {0}", response.Headers["type"]));
                        break;
                }
                return;
            }
            var json = (Dictionary<string, object>) _serializer.DeserializeObject(response.Body);
            switch ((string) json["type"]) {
                case "response":
                    ProcessCommandResponse(json);
                    break;
                case "event":
                    EventHandler<NodeEventEventArgs> nodeEvent = NodeEvent;
                    if (nodeEvent != null) {
                        nodeEvent(this, new NodeEventEventArgs(json));
                    }
                    break;
                default:
                    Debug.WriteLine("Unknown body type: {0}", json["type"]);
                    break;
            }
        }

        private void ProcessCommandResponse(Dictionary<string, object> json) {
            object reqIdObj;
            if (!json.TryGetValue("request_seq", out reqIdObj)) {
                return;
            }
            var reqId = (int) reqIdObj;

            object responseHandlerObj;
            if (!_requestData.TryGetValue(reqId, out responseHandlerObj)) {
                return;
            }
            var responseHandler = responseHandlerObj as ResponseHandler;
            if (responseHandler == null) {
                return;
            }
            _requestData.Remove(reqId);

            responseHandler.HandleResponse(json);
        }

        private int DispenseRequestId() {
            return _currentRequestSequence++;
        }

        /// <summary>
        /// Sends a command to the node debugger.
        /// </summary>
        /// <param name="command">Command name.</param>
        /// <param name="args">Command arguments.</param>
        /// <param name="successHandler">Successful handler.</param>
        /// <param name="failureHandler">Failure handler.</param>
        /// <param name="timeout">Timeout interval in ms.</param>
        /// <param name="shortCircuitPredicate"></param>
        /// <returns></returns>
        public bool SendRequest(
            string command,
            Dictionary<string, object> args = null,
            Action<Dictionary<string, object>> successHandler = null,
            Action<Dictionary<string, object>> failureHandler = null,
            int? timeout = null,
            Func<bool> shortCircuitPredicate = null) {
            if (shortCircuitPredicate != null && shortCircuitPredicate()) {
                if (failureHandler != null) {
                    failureHandler(null);
                }
                return false;
            }

            int reqId = DispenseRequestId();

            // Use response handler if followup (given success or failure handler) or synchronous (given timeout)
            ResponseHandler responseHandler = null;
            if ((successHandler != null) || (failureHandler != null) || (timeout != null)) {
                responseHandler = new ResponseHandler(successHandler, failureHandler, timeout, shortCircuitPredicate);
                _requestData[reqId] = responseHandler;
            }

            Socket socket = Socket;
            if (socket == null) {
                return false;
            }
            try {
                socket.Send(CreateRequest(command, args, reqId));
            }
            catch (SocketException) {
                return false;
            }

            return responseHandler == null || responseHandler.Wait();
        }

        private byte[] CreateRequest(string command, Dictionary<string, object> args, int reqId) {
            string json;

            if (args != null) {
                json = _serializer.Serialize(
                    new
                    {
                        command,
                        seq = reqId,
                        type = "request",
                        arguments = args
                    });
            } else {
                json = _serializer.Serialize(
                    new
                    {
                        command,
                        seq = reqId,
                        type = "request"
                    });
            }

            string requestStr = string.Format("Content-Length: {0}\r\n\r\n{1}", Encoding.UTF8.GetByteCount(json), json);

            Debug.WriteLine(String.Format("Request: {0}", requestStr));

            return Encoding.UTF8.GetBytes(requestStr);
        }
    }
}