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
using System.Diagnostics;

namespace Microsoft.NodejsTools.Debugger {
    class NodeBreakpointBinding {
        private readonly NodeBreakpoint _breakpoint;
        private readonly int _breakpointId;
        private readonly int? _scriptID;
        private bool _enabled;
        private bool _engineEnabled;
        private BreakOn _breakOn;
        private string _condition;
        private uint _engineHitCount;
        private uint _hitCountDelta;
        private int _engineIgnoreCount;

        public NodeBreakpointBinding(
            NodeBreakpoint breakpoint,
            int lineNo,
            int breakpointId,
            int? scriptID,
            bool fullyBound
        ) {
            _breakpoint = breakpoint;
            LineNo = lineNo;
            _breakpointId = breakpointId;
            _scriptID = scriptID;
            _enabled = breakpoint.Enabled;
            _breakOn = breakpoint.BreakOn;
            _condition = breakpoint.Condition;
            _engineEnabled = GetEngineEnabled();
            _engineIgnoreCount = GetEngineIgnoreCount();
            FullyBound = fullyBound;
        }

        public NodeDebugger Process {
            get {
                return _breakpoint.Process;
            }
        }

        public async void Remove() {
            await Process.RemoveBreakPointAsync(this).ConfigureAwait(false);
        }

        public NodeBreakpoint Breakpoint {
            get {
                return _breakpoint;
            }
        }

        public string FileName {
            get {
                return _breakpoint.FileName;
            }
        }

        public string RequestedFileName {
            get {
                return _breakpoint.RequestedFileName;
            }
        }

        /// <summary>
        /// 1 based line number that corresponds with the actual JavaScript code
        /// </summary>
        public int LineNo { get; set; }

        /// <summary>
        /// 1 based line number that corresponds to the file the breakpoint was requested in
        /// </summary>
        public int RequestedLineNo {
            get {
                var mapping = _breakpoint.Process.MapToOriginal(FileName, LineNo - 1);
                if (mapping != null) {
                    return mapping.Line + 1;
                }
                return LineNo;
            }
        }

        public bool Enabled {
            get {
                return _enabled;
            }
        }

        public BreakOn BreakOn {
            get {
                return _breakOn;
            }
        }

        public string Condition {
            get {
                return _condition;
            }
        }

        public int BreakpointID {
            get {
                return _breakpointId;
            }
        }

        internal int? ScriptID {
            get {
                return _scriptID;
            }
        }

        public uint GetHitCount() {
            SyncCounts();
            return HitCount;
        }

        internal bool SetEnabled(bool enabled) {
            if (_enabled != enabled) {
                SyncCounts();
                var engineEnabled = GetEngineEnabled(enabled, _breakOn, HitCount);
                if (_engineEnabled != engineEnabled) {
                    if (!Process.UpdateBreakpointBindingAsync(_breakpointId, engineEnabled, validateSuccess: true).Result) {
                        return false;
                    }
                    _engineEnabled = engineEnabled;
                }
                _enabled = enabled;
            }
            return true;            
        }

        internal bool SetBreakOn(BreakOn breakOn, bool force = false) {
            if (force || _breakOn.Kind != breakOn.Kind || _breakOn.Count != breakOn.Count) {
                SyncCounts();
                var engineEnabled = GetEngineEnabled(_enabled, breakOn, HitCount);
                var enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;
                var engineIgnoreCount = GetEngineIgnoreCount(breakOn, HitCount);
                if (!Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).Result) {
                    return false;
                }
                _engineEnabled = engineEnabled;
                _engineIgnoreCount = engineIgnoreCount;
                _breakOn = breakOn;
            }
            return true;
        }

        internal bool SetHitCount(uint hitCount) {
            SyncCounts();
            if (HitCount != hitCount) {
                if (_breakOn.Kind != BreakOnKind.Always) {
                    // When BreakOn (not BreakOnKind.Always), handle change to hit count by resetting ignore count 
                    var engineEnabled = GetEngineEnabled(_enabled, _breakOn, hitCount);
                    var enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;
                    var engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    if (!Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).Result) {
                        return false;
                    }
                    _engineEnabled = engineEnabled;
                    _engineIgnoreCount = engineIgnoreCount;
                }
                _hitCountDelta = _engineHitCount - hitCount;
            }
            return true;
        }

        internal bool SetCondition(string condition) {
            if (!Process.UpdateBreakpointBindingAsync(_breakpointId, condition: condition, validateSuccess: true).Result) {
                return false;
            }
            _condition = condition;
            return true;
        }

        internal async void ProcessBreakpointHit(Action followupHandler) {
            Debug.Assert(GetEngineEnabled(_enabled, _breakOn, HitCount));

            // Compose followup handler
            var engineEnabled = true;
            var engineHitCount = _engineHitCount + (uint)_engineIgnoreCount + 1;
            var engineIgnoreCount = 0;
            Action followupHandlerWrapper = () => {
                // Update engine state
                _engineEnabled = engineEnabled;
                _engineHitCount = engineHitCount;
                _engineIgnoreCount = engineIgnoreCount;

                // Handle followup
                followupHandler();
            };

            // Handle pass count
            switch (_breakOn.Kind) {
                case BreakOnKind.Always:
                case BreakOnKind.GreaterThanOrEqual:
                    followupHandlerWrapper();
                    break;
                case BreakOnKind.Equal:
                    engineEnabled = false;
                    await Process.UpdateBreakpointBindingAsync(_breakpointId, engineEnabled, followupHandler: followupHandlerWrapper).ConfigureAwait(false);
                    break;
                case BreakOnKind.Mod:
                    var hitCount = engineHitCount - _hitCountDelta;
                    engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, followupHandler: followupHandlerWrapper).ConfigureAwait(false);
                    break;
            }
        }

        internal bool GetEngineEnabled() {
            return GetEngineEnabled(_enabled, _breakOn, HitCount);
        }

        internal static bool GetEngineEnabled(bool enabled, BreakOn breakOn, uint hitCount) {
            if (enabled && breakOn.Kind == BreakOnKind.Equal && hitCount >= breakOn.Count) {
                // Disable BreakOnKind.Equal breakpoints if hit count "exceeds" pass count 
                return false;
            }
            return enabled;
        }

        internal int GetEngineIgnoreCount() {
            return GetEngineIgnoreCount(_breakOn, HitCount);
        }

        internal static int GetEngineIgnoreCount(BreakOn breakOn, uint hitCount) {
            int count = 0;
            switch (breakOn.Kind) {
                case BreakOnKind.Always:
                    count = 0;
                    break;
                case BreakOnKind.Equal:
                case BreakOnKind.GreaterThanOrEqual:
                    count = (int)breakOn.Count - (int)hitCount - 1;
                    if (count < 0) {
                        count = 0;
                    }
                    break;
                case BreakOnKind.Mod:
                    count = (int)(breakOn.Count - hitCount % breakOn.Count - 1);
                    break;
            }
            return count;
        }

        private bool TestHit() {
            // Not hit if any ignore count
            if (GetEngineIgnoreCount() > 0) {
                return false;
            }

            // Not hit if false condition
            if (!string.IsNullOrEmpty(_condition)) {
                return Process.TestPredicateAsync(_condition).Result;
            }

            // Otherwise, hit
            return true;
        }

        internal void TestAndProcessHit(Action<NodeBreakpointBinding> processBinding) {
            // Process based on whether hit (based on hit count and/or condition predicates)
            if (TestHit()) {
                // Fixup hit count
                _hitCountDelta = _engineHitCount - 1;

                // Process as hit
                processBinding(this);
            } else {
                // Process as not hit
                processBinding(null);
            }
        }

        private void SyncCounts() {
            if (_engineIgnoreCount > 0) {
                var hitCount = Process.GetBreakpointHitCountAsync(_breakpointId).Result;
                if (hitCount != null) {
                    _engineIgnoreCount -= hitCount.Value - (int)_engineHitCount;
                    _engineHitCount = (uint)hitCount;
                }
            }
        }

        private uint HitCount {
            get {
                return _engineHitCount - _hitCountDelta;
            }
        }

        internal bool FullyBound { get; set; }

        public bool Unbound { get; set; }
    }
}
