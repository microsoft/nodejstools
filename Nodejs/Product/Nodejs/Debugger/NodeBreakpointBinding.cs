// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Debugger
{
    internal sealed class NodeBreakpointBinding
    {
        private readonly NodeBreakpoint _breakpoint;
        private readonly int _breakpointId;
        private readonly bool _fullyBould;
        private readonly FilePosition _position;
        private readonly int? _scriptId;
        private readonly FilePosition _target;
        private BreakOn _breakOn;
        private string _condition;
        private bool _enabled;
        private bool _engineEnabled;
        private uint _engineHitCount;
        private int _engineIgnoreCount;
        private uint _hitCountDelta;

        public NodeBreakpointBinding(NodeBreakpoint breakpoint, FilePosition target, FilePosition position, int breakpointId, int? scriptId, bool fullyBound)
        {
            this._breakpoint = breakpoint;
            this._target = target;
            this._position = position;
            this._breakpointId = breakpointId;
            this._scriptId = scriptId;
            this._enabled = breakpoint.Enabled;
            this._breakOn = breakpoint.BreakOn;
            this._condition = breakpoint.Condition;
            this._engineEnabled = GetEngineEnabled();
            this._engineIgnoreCount = GetEngineIgnoreCount();
            this._fullyBould = fullyBound;
        }

        public NodeDebugger Process => this._breakpoint.Process;
        public NodeBreakpoint Breakpoint => this._breakpoint;
        /// <summary>
        /// Line and column number that corresponds with the actual JavaScript code
        /// </summary>
        public FilePosition Position => this._position;
        /// <summary>
        /// Line and column number that corresponds to the file the breakpoint was requested in
        /// </summary>
        public FilePosition Target => this._target;
        public bool Enabled => this._enabled;
        public BreakOn BreakOn => this._breakOn;
        public string Condition => this._condition;
        public int BreakpointId => this._breakpointId;
        internal int? ScriptId => this._scriptId;
        private uint HitCount => this._engineHitCount - this._hitCountDelta;
        internal bool FullyBound => this._fullyBould;
        public bool Unbound { get; set; }

        public Task Remove()
        {
            return this.Process.RemoveBreakpointAsync(this).WaitAsync(System.TimeSpan.FromSeconds(2));
        }

        public uint GetHitCount()
        {
            SyncCounts();
            return this.HitCount;
        }

        internal async Task<bool> SetEnabledAsync(bool enabled)
        {
            if (this._enabled == enabled)
            {
                return true;
            }

            SyncCounts();

            var engineEnabled = GetEngineEnabled(enabled, this._breakOn, this.HitCount);
            if (this._engineEnabled != engineEnabled)
            {
                await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, engineEnabled, validateSuccess: true).ConfigureAwait(false);
                this._engineEnabled = engineEnabled;
            }

            this._enabled = enabled;
            return true;
        }

        internal async Task<bool> SetBreakOnAsync(BreakOn breakOn, bool force = false)
        {
            if (!force && this._breakOn.Kind == breakOn.Kind && this._breakOn.Count == breakOn.Count)
            {
                return true;
            }

            SyncCounts();

            var engineEnabled = GetEngineEnabled(this._enabled, breakOn, this.HitCount);
            var enabled = (this._engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

            var engineIgnoreCount = GetEngineIgnoreCount(breakOn, this.HitCount);
            await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).ConfigureAwait(false);

            this._engineEnabled = engineEnabled;
            this._engineIgnoreCount = engineIgnoreCount;
            this._breakOn = breakOn;

            return true;
        }

        internal async Task<bool> SetHitCountAsync(uint hitCount)
        {
            SyncCounts();

            if (this.HitCount == hitCount)
            {
                return true;
            }

            if (this._breakOn.Kind != BreakOnKind.Always)
            {
                // When BreakOn (not BreakOnKind.Always), handle change to hit count by resetting ignore count 
                var engineEnabled = GetEngineEnabled(this._enabled, this._breakOn, hitCount);
                var enabled = (this._engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

                var engineIgnoreCount = GetEngineIgnoreCount(this._breakOn, hitCount);
                await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).ConfigureAwait(false);

                this._engineEnabled = engineEnabled;
                this._engineIgnoreCount = engineIgnoreCount;
            }

            this._hitCountDelta = this._engineHitCount - hitCount;

            return true;
        }

        internal async Task<bool> SetConditionAsync(string condition)
        {
            await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, condition: condition, validateSuccess: true).ConfigureAwait(false);
            this._condition = condition;
            return true;
        }

        private void UpdatedEngineState(bool engineEnabled, uint engineHitCount, int engineIgnoreCount)
        {
            this._engineEnabled = engineEnabled;
            this._engineHitCount = engineHitCount;
            this._engineIgnoreCount = engineIgnoreCount;
        }

        internal async Task ProcessBreakpointHitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Debug.Assert(GetEngineEnabled(this._enabled, this._breakOn, this.HitCount));

            // Compose followup handler
            var engineHitCount = this._engineHitCount + (uint)this._engineIgnoreCount + 1;
            var engineIgnoreCount = 0;

            // Handle pass count
            switch (this._breakOn.Kind)
            {
                case BreakOnKind.Always:
                case BreakOnKind.GreaterThanOrEqual:
                    UpdatedEngineState(true, engineHitCount, engineIgnoreCount);
                    break;
                case BreakOnKind.Equal:
                    await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, false, cancellationToken: cancellationToken).ConfigureAwait(false);
                    UpdatedEngineState(false, engineHitCount, engineIgnoreCount);
                    break;
                case BreakOnKind.Mod:
                    var hitCount = engineHitCount - this._hitCountDelta;
                    engineIgnoreCount = GetEngineIgnoreCount(this._breakOn, hitCount);
                    await this.Process.UpdateBreakpointBindingAsync(this._breakpointId, ignoreCount: engineIgnoreCount, cancellationToken: cancellationToken).ConfigureAwait(false);
                    UpdatedEngineState(true, engineHitCount, engineIgnoreCount);
                    break;
            }
        }

        internal bool GetEngineEnabled()
        {
            return GetEngineEnabled(this._enabled, this._breakOn, this.HitCount);
        }

        internal static bool GetEngineEnabled(bool enabled, BreakOn breakOn, uint hitCount)
        {
            if (enabled && breakOn.Kind == BreakOnKind.Equal && hitCount >= breakOn.Count)
            {
                // Disable BreakOnKind.Equal breakpoints if hit count "exceeds" pass count 
                return false;
            }
            return enabled;
        }

        internal int GetEngineIgnoreCount()
        {
            return GetEngineIgnoreCount(this._breakOn, this.HitCount);
        }

        internal static int GetEngineIgnoreCount(BreakOn breakOn, uint hitCount)
        {
            var count = 0;
            switch (breakOn.Kind)
            {
                case BreakOnKind.Always:
                    count = 0;
                    break;
                case BreakOnKind.Equal:
                case BreakOnKind.GreaterThanOrEqual:
                    count = (int)breakOn.Count - (int)hitCount - 1;
                    if (count < 0)
                    {
                        count = 0;
                    }
                    break;
                case BreakOnKind.Mod:
                    count = (int)(breakOn.Count - hitCount % breakOn.Count - 1);
                    break;
            }
            return count;
        }

        private async Task<bool> TestHitAsync()
        {
            // Not hit if any ignore count
            if (GetEngineIgnoreCount() > 0)
            {
                return false;
            }

            // Not hit if false condition
            if (!string.IsNullOrEmpty(this._condition))
            {
                return await this.Process.TestPredicateAsync(this._condition).ConfigureAwait(false);
            }

            // Otherwise, hit
            return true;
        }

        /// <summary>
        /// Process based on whether hit (based on hit count and/or condition predicates)
        /// </summary>
        /// <returns>Whether break should be handled.</returns>
        internal async Task<bool> TestAndProcessHitAsync()
        {
            if (await TestHitAsync().ConfigureAwait(false))
            {
                // Fixup hit count
                this._hitCountDelta = this._engineHitCount > 0 ? this._engineHitCount - 1 : 0;
                return true;
            }

            // Process as not hit
            return false;
        }

        /// <summary>
        /// Called after we auto resume a breakpoint because it wasn't really hit.
        /// </summary>
        internal void FixupHitCount()
        {
            this._hitCountDelta++;
        }

        private void SyncCounts()
        {
            if (this._engineIgnoreCount <= 0)
            {
                return;
            }

            var hitCount = this.Process.GetBreakpointHitCountAsync(this._breakpointId).Result;
            if (hitCount == null)
            {
                return;
            }

            this._engineIgnoreCount -= hitCount.Value - (int)this._engineHitCount;
            this._engineHitCount = (uint)hitCount;
        }
    }
}

