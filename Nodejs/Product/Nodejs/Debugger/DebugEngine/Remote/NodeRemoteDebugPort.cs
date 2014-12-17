//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
