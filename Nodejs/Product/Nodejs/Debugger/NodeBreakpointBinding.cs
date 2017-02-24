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
            _breakpoint = breakpoint;
            _target = target;
            _position = position;
            _breakpointId = breakpointId;
            _scriptId = scriptId;
            _enabled = breakpoint.Enabled;
            _breakOn = breakpoint.BreakOn;
            _condition = breakpoint.Condition;
            _engineEnabled = GetEngineEnabled();
            _engineIgnoreCount = GetEngineIgnoreCount();
            _fullyBould = fullyBound;
        }

        public NodeDebugger Process
        {
            get { return _breakpoint.Process; }
        }

        public NodeBreakpoint Breakpoint
        {
            get { return _breakpoint; }
        }

        /// <summary>
        /// Line and column number that corresponds with the actual JavaScript code
        /// </summary>
        public FilePosition Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Line and column number that corresponds to the file the breakpoint was requested in
        /// </summary>
        public FilePosition Target
        {
            get { return _target; }
        }

        public bool Enabled
        {
            get { return _enabled; }
        }

        public BreakOn BreakOn
        {
            get { return _breakOn; }
        }

        public string Condition
        {
            get { return _condition; }
        }

        public int BreakpointId
        {
            get { return _breakpointId; }
        }

        internal int? ScriptId
        {
            get { return _scriptId; }
        }

        private uint HitCount
        {
            get { return _engineHitCount - _hitCountDelta; }
        }

        internal bool FullyBound
        {
            get { return _fullyBould; }
        }

        public bool Unbound { get; set; }

        public Task Remove()
        {
            return Process.RemoveBreakpointAsync(this).WaitAsync(System.TimeSpan.FromSeconds(2));
        }

        public uint GetHitCount()
        {
            SyncCounts();
            return HitCount;
        }

        internal async Task<bool> SetEnabledAsync(bool enabled)
        {
            if (_enabled == enabled)
            {
                return true;
            }

            SyncCounts();

            bool engineEnabled = GetEngineEnabled(enabled, _breakOn, HitCount);
            if (_engineEnabled != engineEnabled)
            {
                await Process.UpdateBreakpointBindingAsync(_breakpointId, engineEnabled, validateSuccess: true).ConfigureAwait(false);
                _engineEnabled = engineEnabled;
            }

            _enabled = enabled;
            return true;
        }

        internal async Task<bool> SetBreakOnAsync(BreakOn breakOn, bool force = false)
        {
            if (!force && _breakOn.Kind == breakOn.Kind && _breakOn.Count == breakOn.Count)
            {
                return true;
            }

            SyncCounts();

            bool engineEnabled = GetEngineEnabled(_enabled, breakOn, HitCount);
            bool? enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

            int engineIgnoreCount = GetEngineIgnoreCount(breakOn, HitCount);
            await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).ConfigureAwait(false);

            _engineEnabled = engineEnabled;
            _engineIgnoreCount = engineIgnoreCount;
            _breakOn = breakOn;

            return true;
        }

        internal async Task<bool> SetHitCountAsync(uint hitCount)
        {
            SyncCounts();

            if (HitCount == hitCount)
            {
                return true;
            }

            if (_breakOn.Kind != BreakOnKind.Always)
            {
                // When BreakOn (not BreakOnKind.Always), handle change to hit count by resetting ignore count 
                bool engineEnabled = GetEngineEnabled(_enabled, _breakOn, hitCount);
                bool? enabled = (_engineEnabled != engineEnabled) ? (bool?)engineEnabled : null;

                int engineIgnoreCount = GetEngineIgnoreCount(_breakOn, hitCount);
                await Process.UpdateBreakpointBindingAsync(_breakpointId, ignoreCount: engineIgnoreCount, enabled: enabled, validateSuccess: true).ConfigureAwait(false);

                _engineEnabled = engineEnabled;
                _engineIgnoreCount = engineIgnoreCount;
            }

            _hitCountDelta = _engineHitCount - hitCount;

            return true;
        }

        internal async Task<bool> SetConditionAsync(string condition)
        {
            await Process.UpdateBreakpointBindingAsync(_breakpointId, condition: condition, validateSuccess: true).ConfigureAwait(false);
            _condition = condition;
            return true;
        }

        private void UpdatedEngineState(bool engineEnabled, uint engineHitCount, int engineIgnoreCount)
        {
            _engineEnabled = engineEnabled;
            _engineHitCount = engineHitCount;
            _engineIgnoreCount = engineIgnoreCount;
        }

        internal async Task ProcessBreakpointHitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Debug.Assert(GetEngineEnabled(_enabled, _breakOn, HitCount));

            // Compose followup handler
            uint engineHitCount = _engineHitCount + (uint)_engineIgnoreCount + 1;
            int engineIgnoreCount = 0;

            // Handle pass count
            switch (_breakOn.Kind)
            {
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

        internal bool GetEngineEnabled()
        {
            return GetEngineEnabled(_enabled, _breakOn, HitCount);
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
            return GetEngineIgnoreCount(_breakOn, HitCount);
        }

        internal static int GetEngineIgnoreCount(BreakOn breakOn, uint hitCount)
        {
            int count = 0;
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
            if (!string.IsNullOrEmpty(_condition))
            {
                return await Process.TestPredicateAsync(_condition).ConfigureAwait(false);
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
                _hitCountDelta = _engineHitCount > 0 ? _engineHitCount - 1 : 0;
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
            _hitCountDelta++;
        }

        private void SyncCounts()
        {
            if (_engineIgnoreCount <= 0)
            {
                return;
            }

            int? hitCount = Process.GetBreakpointHitCountAsync(_breakpointId).Result;
            if (hitCount == null)
            {
                return;
            }

            _engineIgnoreCount -= hitCount.Value - (int)_engineHitCount;
            _engineHitCount = (uint)hitCount;
        }
    }
}