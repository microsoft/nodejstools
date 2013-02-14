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

namespace Microsoft.NodeTools.Debugger.Remote {
    internal class NodeRemoteDebugPort : IDebugPort2 {
        private readonly NodeRemoteDebugPortSupplier _supplier;
        private readonly IDebugPortRequest2 _request;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly string _hostName;
        private readonly ushort _portNumber;

        public NodeRemoteDebugPort(NodeRemoteDebugPortSupplier supplier, IDebugPortRequest2 request, string hostName, ushort portNumber) {
            _supplier = supplier;
            _request = request;
            _hostName = hostName;
            _portNumber = portNumber;
        }

        public string HostName {
            get { return _hostName; }
        }

        public ushort PortNumber {
            get { return _portNumber; }
        }

        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum) {
            ppEnum = new NodeRemoteEnumDebugProcesses(this);
            return VSConstants.S_OK;
        }

        public int GetPortId(out Guid pguidPort) {
            pguidPort = _guid;
            return VSConstants.S_OK;
        }

        public int GetPortName(out string pbstrName) {
            pbstrName = _hostName + ":" + _portNumber;
            return VSConstants.S_OK;
        }

        public int GetPortRequest(out IDebugPortRequest2 ppRequest) {
            ppRequest = _request;
            return VSConstants.S_OK;
        }

        public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier) {
            ppSupplier = _supplier;
            return VSConstants.S_OK;
        }

        public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess) {
            throw new NotImplementedException();
        }
    }
}
