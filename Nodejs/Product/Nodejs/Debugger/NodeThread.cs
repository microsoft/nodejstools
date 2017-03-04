// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeThread
    {
        private readonly int _identity;
        private readonly NodeDebugger _process;
        private readonly bool _isWorkerThread;

        internal NodeThread(NodeDebugger process, int identity, bool isWorkerThread)
        {
            this._process = process;
            this._identity = identity;
            this._isWorkerThread = isWorkerThread;
            this.Name = "main thread";
        }

        public void StepInto()
        {
            this._process.SendStepInto(this._identity);
        }

        public void StepOver()
        {
            this._process.SendStepOver(this._identity);
        }

        public void StepOut()
        {
            this._process.SendStepOut(this._identity);
        }

        public void Resume()
        {
            this._process.SendResumeThread(this._identity);
        }

        public bool IsWorkerThread => this._isWorkerThread;

        internal void ClearSteppingState()
        {
            this._process.SendClearStepping(this._identity);
        }

        public IList<NodeStackFrame> Frames { get; set; }

        public int CallstackDepth => this.Frames != null ? this.Frames.Count : 0;

        public NodeStackFrame TopStackFrame => this.Frames != null && this.Frames.Count > 0 ? this.Frames[0] : null;

        public NodeDebugger Process => this._process;

        public string Name { get; set; }

        internal int Id => this._identity;
    }
}

