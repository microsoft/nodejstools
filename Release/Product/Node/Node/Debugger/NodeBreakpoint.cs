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
    class NodeBreakpoint {
        private readonly NodeDebugger _process;
        private readonly string _fileName;
        private int _lineNo;
        private bool _enabled;
        private bool _engineEnabled;
        private BreakOn _breakOn;
        private string _condition;
        private int _breakpointId;
        private uint _engineHitCount;
        private uint _hitCountDelta;
        private int _engineIgnoreCount;
        private bool _boundByName;
        private bool _pendingLocationFixup;

        public NodeBreakpoint(
            NodeDebugger process,
            string fileName,
            int lineNo,
            bool enabled,
            BreakOn breakOn,
            string condition
        ) {
            _process = process;
            _fileName = fileName;
            _lineNo = lineNo;
            _enabled = enabled;
            _breakOn = breakOn;
            _condition = condition;
            _engineEnabled = GetEngineEnabled();
            _engineIgnoreCount = GetEngineIgnoreCount();
        }

        /// <summary>
        /// Requests the remote process enable the break point.  An event will be raised on the process
        /// when the break point is received.
        /// </summary>
        public void Add(Action<bool> successHandler = null, Action failureHandler = null) {
            _process.BindBreakpoint(this, successHandler, failureHandler);
        }

        /// <summary>
        /// Removes the provided break point
        /// </summary>
        public void Remove() {
            _process.RemoveBreakPoint(this.Id);
            _breakpointId = 0;
        }

        public string FileName {
            get {
                return _fileName;
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

        internal int Id {
            get {
                return _breakpointId;
            }
            set {
                _breakpointId = value;
            }
        }

        internal bool BoundByName {
            get {
                return _boundByName;
            }
            set {
                _boundByName = value;
            }
        }

        internal bool PendingLocationFixup {
            get {
                return _pendingLocationFixup;
            }
            set {
                _pendingLocationFixup = value;
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
                    if (!_process.UpdateBreakpointBinding(Id, enabled: engineEnabled, validateSuccess: true)) {
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
                if (!_process.UpdateBreakpointBinding(Id, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true)) {
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
                    if (!_process.UpdateBreakpointBinding(Id, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true)) {
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
            if (!_process.UpdateBreakpointBinding(Id, condition: condition, validateSuccess: true)) {
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
                    _process.UpdateBreakpointBinding(Id, enabled: engineEnabled, followupHandler: followupHandlerWrapper);
                    break;
                case BreakOnKind.Mod:
                    var hitCount = engineHitCount - _hitCountDelta;
                    engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    _process.UpdateBreakpointBinding(Id, ignoreCount: engineIgnoreCount, followupHandler: followupHandlerWrapper);
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

        private static bool GetEngineEnabled(bool enabled, BreakOn breakOn, uint hitCount) {
            if (enabled && breakOn.kind == BreakOnKind.Equal && hitCount >= breakOn.count) {
                // Disable BreakOnKind.Equal breakpoints if hit count "exceeds" pass count 
                return false;
            }
            return enabled;
        }

        internal int GetEngineIgnoreCount() {
            return GetEngineIgnoreCount(_breakOn, HitCount);
        }

        private static int GetEngineIgnoreCount(BreakOn breakOn, uint hitCount) {
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
                var hitCount = _process.GetBreakpointHitCount(Id);
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
    }
}
