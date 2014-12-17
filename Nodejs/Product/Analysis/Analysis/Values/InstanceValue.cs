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

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class InstanceValue : ObjectValue {
        private readonly string _name;

        public InstanceValue(ProjectEntry projectEntry, AnalysisValue prototype, string name = null)
            : base(projectEntry, prototype, "instance of " + name) {
            _name = name;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(InstanceValue));
        }

        public override string ObjectDescription {
            get {
                if (_name != null) {
                    return _name + " object";
                }

                return base.ObjectDescription;
            }
        }

        internal override bool UnionEquals(AnalysisValue ns, int strength) {
            if (strength >= MergeStrength.ToObject) {
                if (ns is InstanceValue) {
                    return true;
                }
            }

            return base.UnionEquals(ns, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToObject) {
                return ProjectState._immutableObject.GetHashCode();
            }

            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue ns, int strength) {
            if (strength >= MergeStrength.ToObject) {
                if (ns is InstanceValue) {
                    return ProjectState._immutableObject;
                }
            }

            return base.UnionMergeTypes(ns, strength);
        }

    }
}
