// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

namespace Microsoft.NodejsTools.Debugger.Remote
{
    internal class NodeRemoteEnumDebugPrograms : NodeRemoteEnumDebug<IDebugProgram2>, IEnumDebugPrograms2
    {
        public NodeRemoteEnumDebugPrograms(NodeRemoteDebugProcess process)
            : base(new NodeRemoteDebugProgram(process))
        {
        }

        public NodeRemoteEnumDebugPrograms(NodeRemoteEnumDebugPrograms programs)
            : base(programs.Element)
        {
        }

        public int Clone(out IEnumDebugPrograms2 ppEnum)
        {
            ppEnum = new NodeRemoteEnumDebugPrograms(this);
            return VSConstants.S_OK;
        }
    }
}
