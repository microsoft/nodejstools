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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Value that we promote instance values to when merging which doesn't allow any
    /// assignments.
    /// </summary>
    [Serializable]
    class ImmutableObjectValue : InstanceValue {
        public ImmutableObjectValue(ProjectEntry projectEntry)
            : base(projectEntry, null) {
        }

        public override void SetMember(Node node, AnalysisUnit unit, string name, IAnalysisSet value) {
        }

        public override void SetIndex(Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return base.Get(node, unit, name, false);
        }

        public override string ObjectDescription {
            get {
                return "object";
            }
        }
    }
}
