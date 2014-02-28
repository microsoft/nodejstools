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
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteDebugPort : IDebugPort2 {
        private readonly NodeRemoteDebugPortSupplier _supplier;
        private readonly IDebugPortRequest2 _request;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly Uri _uri;

        public NodeRemoteDebugPort(NodeRemoteDebugPortSupplier supplier, IDebugPortRequest2 request, Uri uri) {
            _supplier = supplier;
            _request = request;
            _uri = uri;
        }

        public Uri Uri {
            get { return _uri; }
        }

        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum) {
            ppEnum = new NodeRemoteEnumDebugProcesses(this, new NetworkClientFactory());
            return VSConstants.S_OK;
        }

        public int GetPortId(out Guid pguidPort) {
            pguidPort = _guid;
            return VSConstants.S_OK;
        }

        public int GetPortName(out string pbstrName) {
            pbstrName = _uri.ToString();
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
            ppProcess = null;

            if (ProcessId.ProcessIdType != (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM) {
                return VSConstants.E_FAIL;
            }

            IEnumDebugProcesses2 processEnum;
            int hr = EnumProcesses(out processEnum);
            if (ErrorHandler.Failed(hr)) {
                return hr;
            }

            var processes = new IDebugProcess2[1];
            var pids = new AD_PROCESS_ID[1];
            uint fetched = 0;
            while (true) {
                hr = processEnum.Next(1, processes, ref fetched);
                if (ErrorHandler.Failed(hr)) {
                    return hr;
                } else if (fetched == 0) {
                    return VSConstants.E_FAIL;
                }

                if (ErrorHandler.Succeeded(processes[0].GetPhysicalProcessId(pids)) && ProcessId.dwProcessId == pids[0].dwProcessId) {
                    ppProcess = processes[0];
                    return VSConstants.S_OK;
                }
            }
        }
    }
}
