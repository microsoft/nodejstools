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

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger {
    class NodeBreakpointBinding {
        private readonly NodeBreakpoint _breakpoint;
        private readonly int _breakpointId;
        private readonly int? _scriptId;
        private BreakOn _breakOn;
        private string _condition;
        private bool _enabled;
        private bool _engineEnabled;
        private uint _engineHitCount;
        private int _engineIgnoreCount;
        private uint _hitCountDelta;

        public NodeBreakpointBinding(
            NodeBreakpoint breakpoint,
            int lineNo,
            int breakpointId,
            int? scriptId,
            bool fullyBound
        ) {
            _breakpoint = breakpoint;
            LineNo = lineNo;
            _breakpointId = breakpointId;
            _scriptId = scriptId;
            _enabled = breakpoint.Enabled;
            _breakOn = breakpoint.BreakOn;
            _condition = breakpoint.Condition;
            _engineEnabled = GetEngineEnabled();
            _engineIgnoreCount = GetEngineIgnoreCount();
            FullyBound = fullyBound;
        }

        public NodeDebugger Process {
            get { return _breakpoint.Process; }
        }

        public NodeBreakpoint Breakpoint {
            get { return _breakpoint; }
        }

        public string FileName {
            get { return _breakpoint.FileName; }
        }

        public string RequestedFileName {
            get { return _breakpoint.RequestedFileName; }
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
                SourceMapping mapping = _breakpoint.Process.SourceMapper.MapToOriginal(FileName, LineNo);
                if (mapping != null) {
                    return mapping.Line;
                }
                return LineNo;
            }
        }

        public bool Enabled {
            get { return _enabled; }
        }

        public BreakOn BreakOn {
            get { return _breakOn; }
        }

        public string Condition {
            get { return _condition; }
        }

        public int BreakpointId {
            get { return _breakpointId; }
        }

        internal int? ScriptId {
            get { return _scriptId; }
        }

        private uint HitCount {
            get { return _engineHitCount - _hitCountDelta; }
        }

        internal bool FullyBound { get; private set; }

        public bool Unbound { get; set; }

        public async void Remove() {
            await Process.RemoveBreakPointAsync(this).ConfigureAwait(false);
        }

        public uint GetHitCount() {
            SyncCounts();
            return HitCount;
        }

        internal async Task<bool> SetEnabledAsync(bool enabled) {
            if (_enabled == enabled) {
                return true;
            }

            SyncCounts();

            bool engineEnabled = GetEngineEnabled(enabled, _breakOn, HitCount);
            if (_engineEnabled != engineEnabled) {
                await Process.UpdateBreakpointBindingAsync(_breakpointId, engineEnabled, validateSuccess: true);
                _engineEnabled = engineEnabled;
            }

            _enabled = enabled;
            return true;
        }

        internal async Task<bool> SetBreakOnAsync(BreakOn breakOn, bool force = false) {
            if (!force && _breakOn.Kind == breakOn.Kind && _breakOn.Count == breakOn.Count) {
                return true;
            }

            SyncCounts();

            bool engineEnabled = GetEngineEnabled(_enabled, breakOn, HitCount);
            bool? enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

            int engineIgnoreCount = GetEngineIgnoreCount(breakOn, HitCount);
            await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true);

            _engineEnabled = engineEnabled;
            _engineIgnoreCount = engineIgnoreCount;
            _breakOn = breakOn;

            return true;
        }

        internal async Task<bool> SetHitCountAsync(uint hitCount) {
            SyncCounts();

            if (HitCount == hitCount) {
                return true;
            }

            if (_breakOn.Kind != BreakOnKind.Always) {
                // When BreakOn (not BreakOnKind.Always), handle change to hit count by resetting ignore count 
                bool engineEnabled = GetEngineEnabled(_enabled, _breakOn, hitCount);
                bool? enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

                int engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true);

                _engineEnabled = engineEnabled;
                _engineIgnoreCount = engineIgnoreCount;
            }

            _hitCountDelta = _engineHitCount - hitCount;

            return true;
        }

        internal async Task<bool> SetConditionAsync(string condition) {
            await Process.UpdateBreakpointBindingAsync(_breakpointId, condition: condition, validateSuccess: true);
            _condition = condition;
            return true;
        }

        private void UpdatedEngineState(bool engineEnabled, uint engineHitCount, int engineIgnoreCount) {
            _engineEnabled = engineEnabled;
            _engineHitCount = engineHitCount;
            _engineIgnoreCount = engineIgnoreCount;
        }

        internal async Task ProcessBreakpointHitAsync(CancellationToken cancellationToken = new CancellationToken()) {
            Debug.Assert(GetEngineEnabled(_enabled, _breakOn, HitCount));

            // Compose followup handler
            uint engineHitCount = _engineHitCount + (uint)_engineIgnoreCount + 1;
            int engineIgnoreCount = 0;

            // Handle pass count
            switch (_breakOn.Kind) {
                case BreakOnKind.Always:
                case BreakOnKind.GreaterThanOrEqual:
                    UpdatedEngineState(true, engineHitCount, engineIgnoreCount);
                    break;
                case BreakOnKind.Equal:
                    await Process.UpdateBreakpointBindingAsync(_breakpointId, false, cancellationToken: cancellationToken).ConfigureAwait(false);
                    UpdatedEngineState(false, engineHitCount, engineIgnoreCount);
                    break;
                case BreakOnKind.Mod:
                    uint hitCount = engineHitCount - _hitCountDelta;
                    engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                    await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, cancellationToken: cancellationToken).ConfigureAwait(false);
                    UpdatedEngineState(true, engineHitCount, engineIgnoreCount);
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

        private async Task<bool> TestHitAsync() {
            // Not hit if any ignore count
            if (GetEngineIgnoreCount() > 0) {
                return false;
            }

            // Not hit if false condition
            if (!string.IsNullOrEmpty(_condition)) {
                return await Process.TestPredicateAsync(_condition);
            }

            // Otherwise, hit
            return true;
        }

        internal async Task<bool> TestAndProcessHitAsync() {
            // Process based on whether hit (based on hit count and/or condition predicates)
            if (await TestHitAsync()) {
                // Fixup hit count
                _hitCountDelta = _engineHitCount - 1;
                return true;
            }

            // Process as not hit
            return false;
        }

        private void SyncCounts() {
            if (_engineIgnoreCount <= 0) {
                return;
            }

            int? hitCount = Process.GetBreakpointHitCountAsync(_breakpointId).Result;
            if (hitCount == null) {
                return;
            }

            _engineIgnoreCount -= hitCount.Value - (int)_engineHitCount;
            _engineHitCount = (uint)hitCount;
        }
    }
}