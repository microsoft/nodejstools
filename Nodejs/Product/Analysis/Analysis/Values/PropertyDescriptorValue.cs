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
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// Represents the descriptor state for a given property.
    /// 
    /// We track all of Values, Get, Set and merge them together,
    /// so if a property is changing between the two we'll see
    /// the union.
    /// 
    /// We don't currently track anything like writable/enumerable/configurable but
    /// we do return a boolean value when accessing them as 1st class objects.
    /// 
    /// We also don't support handing out property descriptors from property
    /// descriptors.  That is:
    /// 
    /// Object.getOwnPropertyDescriptor(Object.getOwnPropertyDescriptor(x, 'abc'), 'value')
    /// 
    /// Will fail.  This prevents us introducing an infinite amount of objects
    /// into the analysis system by recursive property descriptor creation.
    /// </summary>
    [Serializable]
    class PropertyDescriptorValue : AnalysisValue, IPropertyDescriptor {
        public VariableDef Values, Getter, Setter;
        public ProjectEntry _projectEntry;

        public PropertyDescriptorValue(ProjectEntry projectEntry)
            : base(projectEntry) {
            _projectEntry = projectEntry;
        }

        internal override ProjectEntry DeclaringModule {
            get {
                return _projectEntry;
            }
        }

        public IAnalysisSet GetValue(Node node, AnalysisUnit unit, ProjectEntry declaringScope, IAnalysisSet @this, bool addRef) {
            if (Values == null) {
                Values = new EphemeralVariableDef();
            }

            var res = Values.GetTypes(unit, declaringScope);

            if (res.Count > 0) {
                // Don't add references to ephemeral values...  If they
                // gain types we'll re-enqueue and the reference will be
                // added then.
                if (addRef && !Values.IsEphemeral) {
                    Values.AddReference(node, unit);
                }
            }

            if (Getter != null) {
                res = res.Union(Getter.GetTypesNoCopy(unit, declaringScope).Call(node, unit, @this, ExpressionEvaluator.EmptySets));
            }

            return res;
        }

        public bool IsEphemeral {
            get {
                return Values == null || Values.IsEphemeral;
            }
        }


        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            switch (name) {
                case "value":
                    if (Values != null) {
                        return Values.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "get":
                    if (Getter != null) {
                        return Getter.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "set":
                    if (Setter != null) {
                        return Setter.GetTypes(unit, _projectEntry);
                    }
                    break;
                case "writable":
                case "enumerable":
                case "configurable":
                    return unit.Analyzer._trueInst.SelfSet;
            }
            return AnalysisSet.Empty;
        }
    }
}
