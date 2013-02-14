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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Microsoft.NodeTools.Debugger.Remote {
    [ComVisible(true)]
    [Guid("A241707C-7DB3-464F-8D3E-F3D33E86AE99")]
    public class NodeRemoteDebugPortSupplier : IDebugPortSupplier2, IDebugPortSupplierDescription2 {
        public const string PortSupplierId = "{FEB76325-D127-4E02-B59D-B16D93D46CF5}";
        private static readonly Guid _guid = new Guid(PortSupplierId);
        private static readonly string _defaultHost = "localhost";
        private static readonly ushort _defaultPort = 5858;
        private static readonly Regex _portNameRegex = new Regex(@"^(?<hostName>[^:\n]+?)?(:(?<portNum>\d+))?$", RegexOptions.ExplicitCapture);

        public NodeRemoteDebugPortSupplier() {
        }

        // Qualifier for our transport is parsed as 'hostname:port', where ':port' is optional.
        public int AddPort(IDebugPortRequest2 pRequest, out IDebugPort2 ppPort) {
            ppPort = null;

            string name;
            pRequest.GetPortName(out name);

            Match m = _portNameRegex.Match(name);
            if (!m.Success) {
                return new FormatException().HResult;
            }

            string hostName = _defaultHost;
            if (m.Groups["hostName"].Success) {
                hostName = m.Groups["hostName"].Value;
            }

            ushort portNum = _defaultPort;
            if (m.Groups["portNum"].Success) {
                if (!ushort.TryParse(m.Groups["portNum"].Value, out portNum)) {
                    return new FormatException().HResult;
                }
            }

            ppPort = new NodeRemoteDebugPort(this, pRequest, hostName, portNum);
            return VSConstants.S_OK;
        }

        public int CanAddPort() {
            return VSConstants.S_OK;
        }

        public int EnumPorts(out IEnumDebugPorts2 ppEnum) {
            ppEnum = null;
            return VSConstants.S_OK;
        }

        public int GetPort(ref Guid guidPort, out IDebugPort2 ppPort) {
            throw new NotImplementedException();
        }

        public int GetPortSupplierId(out Guid pguidPortSupplier) {
            pguidPortSupplier = _guid;
            return VSConstants.S_OK;
        }

        public int GetPortSupplierName(out string pbstrName) {
            pbstrName = "Node remote debugging (unsecured)";
            return VSConstants.S_OK;
        }

        public int RemovePort(IDebugPort2 pPort) {
            return VSConstants.S_OK;
        }

        public int GetDescription(enum_PORT_SUPPLIER_DESCRIPTION_FLAGS[] pdwFlags, out string pbstrText) {
            pbstrText =
                "Allows debugging a Node.js process on a remote machine running any OS, if it can be connected to via TCP, " +
                "and the process has enabled remote debugging by running Node.exe with the --debug argument. " +
                "Specify the hostname and port to connect to in the 'Qualifier' textbox, e.g. 'localhost:5858'. " +
                "This transport is not secure, and should not be used on a network that might have hostile traffic.";
            return VSConstants.S_OK;
        }
    }
}
