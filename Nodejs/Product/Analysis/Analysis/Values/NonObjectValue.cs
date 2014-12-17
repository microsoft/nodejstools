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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents a value which is not an object (number, string, bool)
    /// </summary>
    [Serializable]
    abstract class NonObjectValue : AnalysisValue, IReferenceableContainer {
        public NonObjectValue(ProjectEntry projectEntry)
            : base(projectEntry) {
        }

        public abstract AnalysisValue Prototype {
            get;
        }

        internal override Dictionary<string, IAnalysisSet> GetAllMembers(ProjectEntry accessor) {
            return Prototype.GetAllMembers(accessor);
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            return Prototype.Get(node, unit, name);
        }

        internal override IPropertyDescriptor GetProperty(Node node, AnalysisUnit unit, string name) {
            return Prototype.GetProperty(node, unit, name);
        }

        internal override Dictionary<string, IAnalysisSet> GetOwnProperties(ProjectEntry accessor) {
            return Prototype.GetOwnProperties(accessor);
        }

        public IEnumerable<IReferenceable> GetDefinitions(string name) {
            var proto = Prototype as IReferenceableContainer;
            if (proto != null) {
                return proto.GetDefinitions(name);
            }
            return new IReferenceable[0];
        }
    }
}
