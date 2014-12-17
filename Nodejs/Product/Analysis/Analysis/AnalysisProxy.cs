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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Analysis {
    /// <summary>
    /// Provides a wrapper around our AnalysisValue's.  Wrappers are invalidated
    /// when new versions of projects are defined effectively removing the old
    /// values from the system.  Our various hashing primitives will wipe out
    /// the old values but the old values also won't keep a large amount of
    /// state around.
    /// </summary>
    [Serializable]
    internal sealed class AnalysisProxy : IDetachableProxy, IImmutableAnalysisSet {
        private AnalysisValue _value;

        public AnalysisProxy(AnalysisValue value) {
            _value = value;
        }

        public AnalysisValue Value {
            get {
                return _value;
            }
        }

        public void NewVersion() {
            _value = null;
        }

        public bool IsAlive {
            get { return _value != null; }
        }

        IAnalysisSet IAnalysisSet.Add(AnalysisProxy item, out bool wasChanged) {
            if (((IAnalysisSet)this).Comparer.Equals(this, item)) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return new AnalysisSetDetails.AnalysisSetTwoObject(this, item);
        }

        IAnalysisSet IAnalysisSet.Union(IEnumerable<AnalysisProxy> items, out bool wasChanged) {
            if (items.All(av => ((IAnalysisSet)this).Comparer.Equals(this, av))) {
                wasChanged = false;
                return this;
            }
            wasChanged = true;
            return AnalysisSet.Create(items).Add(this);
        }

        IAnalysisSet IAnalysisSet.Clone() {
            return this;
        }

        bool IAnalysisSet.Contains(AnalysisProxy item) {
            return ((IAnalysisSet)this).Comparer.Equals(this, item);
        }

        int IAnalysisSet.Count {
            get {
                if (IsAlive) {
                    return 1;
                }
                return 0;
            }
        }

        IEqualityComparer<AnalysisProxy> IAnalysisSet.Comparer {
            get { return ObjectComparer.Instance; }
        }

        IEnumerator<AnalysisProxy> IEnumerable<AnalysisProxy>.GetEnumerator() {
            if (IsAlive) {
                yield return this;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return ((IEnumerable<AnalysisProxy>)this).GetEnumerator();
        }

        public override string ToString() {
            if (_value != null) {
                return _value.ToString();
            }
            return "<detached proxy>";
        }
    }
}
