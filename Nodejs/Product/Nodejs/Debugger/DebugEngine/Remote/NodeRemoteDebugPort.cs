// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.NodejsTools.Debugger.Communication;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote
{
    internal class NodeRemoteDebugPort : IDebugPort2
    {
        private readonly NodeRemoteDebugPortSupplier _supplier;
        private readonly IDebugPortRequest2 _request;
        private readonly Guid _guid = Guid.NewGuid();
        private readonly Uri _uri;

        public NodeRemoteDebugPort(NodeRemoteDebugPortSupplier supplier, IDebugPortRequest2 request, Uri uri)
        {
            this._supplier = supplier;
            this._request = request;
            this._uri = uri;
        }

        public Uri Uri => this._uri;
        public int EnumProcesses(out IEnumDebugProcesses2 ppEnum)
        {
            ppEnum = new NodeRemoteEnumDebugProcesses(this, new NetworkClientFactory());
            return VSConstants.S_OK;
        }

        public int GetPortId(out Guid pguidPort)
        {
            pguidPort = this._guid;
            return VSConstants.S_OK;
        }

        public int GetPortName(out string pbstrName)
        {
            pbstrName = this._uri.ToString();
            return VSConstants.S_OK;
        }

        public int GetPortRequest(out IDebugPortRequest2 ppRequest)
        {
            ppRequest = this._request;
            return VSConstants.S_OK;
        }

        public int GetPortSupplier(out IDebugPortSupplier2 ppSupplier)
        {
            ppSupplier = this._supplier;
            return VSConstants.S_OK;
        }

        public int GetProcess(AD_PROCESS_ID ProcessId, out IDebugProcess2 ppProcess)
        {
            ppProcess = null;

            if (ProcessId.ProcessIdType != (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM)
            {
                return VSConstants.E_FAIL;
            }

            var hr = EnumProcesses(out var processEnum);
            if (ErrorHandler.Failed(hr))
            {
                return hr;
            }

            var processes = new IDebugProcess2[1];
            var pids = new AD_PROCESS_ID[1];
            uint fetched = 0;
            while (true)
            {
                hr = processEnum.Next(1, processes, ref fetched);
                if (ErrorHandler.Failed(hr))
                {
                    return hr;
                }
                else if (fetched == 0)
                {
                    return VSConstants.E_FAIL;
                }

                if (ErrorHandler.Succeeded(processes[0].GetPhysicalProcessId(pids)) && ProcessId.dwProcessId == pids[0].dwProcessId)
                {
                    ppProcess = processes[0];
                    return VSConstants.S_OK;
                }
            }
        }
    }
}
