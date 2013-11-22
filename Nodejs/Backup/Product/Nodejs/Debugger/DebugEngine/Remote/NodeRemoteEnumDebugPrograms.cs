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

namespace Microsoft.NodejsTools.Debugger.Remote {
    internal class NodeRemoteEnumDebugPrograms : NodeRemoteEnumDebug<IDebugProgram2>, IEnumDebugPrograms2 {

        public NodeRemoteEnumDebugPrograms(NodeRemoteDebugProcess process)
            : base(new NodeRemoteDebugProgram(process)) {
        }

        public NodeRemoteEnumDebugPrograms(NodeRemoteEnumDebugPrograms programs)
            : base(programs.Element) {
        }

        public int Clone(out IEnumDebugPrograms2 ppEnum) {
            ppEnum = new NodeRemoteEnumDebugPrograms(this);
            return VSConstants.S_OK;
        }
    }
}
