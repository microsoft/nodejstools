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
        private NodeBreakpoint _breakpoint;
        private int _lineNo;
        private int _breakpointId;
        private int? _scriptID;
        private bool _enabled;
        private bool _engineEnabled;
        private BreakOn _breakOn;
        private string _condition;
        private uint _engineHitCount;
        private uint _hitCountDelta;
        private int _engineIgnoreCount;
        private bool _fullyBound;
        private bool _unbound;

        public NodeBreakpointBinding(
            NodeBreakpoint breakpoint,
            int lineNo,
            int breakpointId,
            int? scriptID
        ) {
            _breakpoint = breakpoint;
            _lineNo = lineNo;
            _breakpointId = breakpointId;
            _scriptID = scriptID;
            _enabled = breakpoint.Enabled;
            _breakOn = breakpoint.BreakOn;
            _condition = breakpoint.Condition;
            _engineEnabled = GetEngineEnabled();
            _engineIgnoreCount = GetEngineIgnoreCount();
            _fullyBound = (_scriptID.HasValue && _lineNo == _breakpoint.LineNo);
        }

        public NodeDebugger Process {
            get {
                return _breakpoint.Process;
            }
        }

        public void Remove() {
            Process.RemoveBreakPoint(this);
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

        public int LineNo {
            get {
                return _lineNo;
            }
            set {
                _lineNo = value;
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
                    if (!Process.UpdateBreakpointBinding(_breakpointId, enabled: engineEnabled, validateSuccess: true)) {
                        return false;
                    }
                    _engineEnabled = engineEnabled;
                }
                _enabled = enabled;
            }
            return true;            
        }

        internal bool SetBreakOn(BreakOn breakOn, bool force = false) {
            if (force || _breakOn.kind != breakOn.kind || _breakOn.count != breakOn.count) {
                SyncCounts();
                var engineEnabled = GetEngineEnabled(_enabled, breakOn, HitCount);
                var enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;
                var engineIgnoreCount = GetEngineIgnoreCount(breakOn, HitCount);
                if (!Process.UpdateBreakpointBinding(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true)) {
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
                if (_breakOn.kind != BreakOnKind.Always) {
                    // When BreakOn (not BreakOnKind.Always), handle change to hit count by resetting ignore count 
                    var engineEnabled = GetEngineEnabled(_enabled, _breakOn, hitCount);
                    var enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;
                    var engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    if (!Process.UpdateBreakpointBinding(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true)) {
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
            if (!Process.UpdateBreakpointBinding(_breakpointId, condition: condition, validateSuccess: true)) {
                return false;
            }
            _condition = condition;
            return true;
        }

        internal void ProcessBreakpointHit(Action followupHandler) {
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
            switch (_breakOn.kind) {
                case BreakOnKind.Always:
                case BreakOnKind.GreaterThanOrEqual:
                    followupHandlerWrapper();
                    break;
                case BreakOnKind.Equal:
                    engineEnabled = false;
                    Process.UpdateBreakpointBinding(_breakpointId, enabled: engineEnabled, followupHandler: followupHandlerWrapper);
                    break;
                case BreakOnKind.Mod:
                    var hitCount = engineHitCount - _hitCountDelta;
                    engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    Process.UpdateBreakpointBinding(_breakpointId, ignoreCount: engineIgnoreCount, followupHandler: followupHandlerWrapper);
                    break;
            }
        }

        internal bool TryIgnore() {
            if (GetEngineEnabled() && GetEngineIgnoreCount() > 0) {
                --_hitCountDelta;
                return SetBreakOn(_breakOn, force: true);
            }
            return false;
        }

        internal bool GetEngineEnabled() {
            return GetEngineEnabled(_enabled, _breakOn, HitCount);
        }

        internal static bool GetEngineEnabled(bool enabled, BreakOn breakOn, uint hitCount) {
            if (enabled && breakOn.kind == BreakOnKind.Equal && hitCount >= breakOn.count) {
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
            switch (breakOn.kind) {
                case BreakOnKind.Always:
                    count = 0;
                    break;
                case BreakOnKind.Equal:
                case BreakOnKind.GreaterThanOrEqual:
                    count = (int)breakOn.count - (int)hitCount - 1;
                    if (count < 0) {
                        count = 0;
                    }
                    break;
                case BreakOnKind.Mod:
                    count = (int)(breakOn.count - hitCount % breakOn.count - 1);
                    break;
            }
            return count;
        }

        private void SyncCounts() {
            if (_engineIgnoreCount > 0) {
                var hitCount = Process.GetBreakpointHitCount(_breakpointId);
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

        internal bool FullyBound {
            get {
                return _fullyBound;
            }
            set {
                _fullyBound = value;
            }
        }

        public bool Unbound {
            get {
                return _unbound;
            }
            set {
                _unbound = value;
            }
        }
    }
}
