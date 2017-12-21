// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote
{
    [ComVisible(true)]
    [Guid(Guids.RemoteDebugPortSupplier)]
    public class NodeRemoteDebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2
    {
        public const string PortSupplierId = "{9E16F805-5EFC-4CE5-8B67-9AE9B643EF80}";
        public static readonly Guid PortSupplierGuid = new Guid(PortSupplierId);

        private static readonly Guid _guid = new Guid(PortSupplierId);

        public NodeRemoteDebugPortSupplier()
        {
        }

        // Qualifier for our transport is parsed either as a tcp://, ws:// or ws:// URI,
        // or as 'hostname:port', where ':port' is optional.
        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort)
        {
            ppPort = null;

            pRequest.GetPortName(out var name);

            // Support old-style 'hostname:port' format, as well.
            if (!name.Contains("://"))
            {
                name = "tcp://" + name;
            }

            var uri = new Uri(name, UriKind.Absolute);
            switch (uri.Scheme)
            {
                case "tcp":
                    // tcp:// URI should only specify host and optionally port, path has no meaning and is invalid.
                    if (uri.PathAndQuery != "/")
                    {
                        return new FormatException().HResult;
                    }
                    // Set default port if not specified.
                    if (uri.Port < 0)
                    {
                        uri = new UriBuilder(uri) { Port = NodejsConstants.DefaultDebuggerPort }.Uri;
                    }
                    break;

                case "ws":
                case "wss":
                    // WebSocket URIs are used as is
                    break;

                default:
                    // Anything else is not a valid debugger endpoint
                    return new FormatException().HResult;
            }

            ppPort = new NodeRemoteDebugPort(this, pRequest, uri);
            return VSConstants.S_OK;
        }

        public int CanAddPort()
        {
            return VSConstants.S_OK;
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum)
        {
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort)
        {
            throw new NotImplementedException();
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier)
        {
            pguidPortSupplier = _guid;
            return VSConstants.S_OK;
        }

        public int GetPortSupplierName(out string pbstrName)
        {
            pbstrName = "Node.js remote debugging";
            return VSConstants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort)
        {
            return VSConstants.S_OK;
        }

        public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText)
        {
            pbstrText = string.Format(Resources.DebuggerAttachToDescription, NodejsConstants.DefaultDebuggerPort);
            return VSConstants.S_OK;
        }
    }
}
