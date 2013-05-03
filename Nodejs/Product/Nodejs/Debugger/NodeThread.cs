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

using System.Collections.Generic;

namespace Microsoft.NodejsTools.Debugger {
    class NodeThread {
        private readonly int _identity;
        private readonly NodeDebugger _process;
        private readonly bool _isWorkerThread;
        private string _name;
        private IList<NodeStackFrame> _frames;

        internal NodeThread(NodeDebugger process, int identity, bool isWorkerThread) {
            _process = process;
            _identity = identity;
            _isWorkerThread = isWorkerThread;
            _name = "";
        }

        public void StepInto() {
            _process.SendStepInto(_identity);
        }

        public void StepOver() {
            _process.SendStepOver(_identity);
        }

        public void StepOut() {
            _process.SendStepOut(_identity);
        }

        public void Resume() {
            _process.SendResumeThread(_identity);
        }

        public bool IsWorkerThread {
            get {
                return _isWorkerThread;
            }
        }

        internal void ClearSteppingState() {
            _process.SendClearStepping(_identity);
        }

        public IList<NodeStackFrame> Frames {
            get {
                return _frames;
            }
            set {
                _frames = value;
            }
        }

        public NodeDebugger Process {
            get {
                return _process;
            }
        }

        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        internal int Id {
            get {
                return _identity;
            }
        }
    }
}
