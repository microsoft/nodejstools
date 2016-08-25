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
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    /// <summary>
    /// A unique value which gets created for util.inherits.  This wraps the value
    /// which we're inheriting from and forwards all of it's accesses to it but
    /// keeps member assignments amongst it's self.  This means that the value
    /// we inherit from won't pick up values assigned to the new prototype.
    /// </summary>
    [Serializable]
    class InheritsPrototypeValue : ObjectValue {
        private IAnalysisSet _prototypes;

        public InheritsPrototypeValue(ProjectEntry projectEntry, IAnalysisSet values)
            : base(projectEntry) {
            _prototypes = values;
            projectEntry.Analyzer.AnalysisValueCreated(typeof(InheritsPrototypeValue));
        }

        public void AddPrototypes(IAnalysisSet values) {
            _prototypes = _prototypes.Union(values);
        }

        public override IAnalysisSet Get(Node node, AnalysisUnit unit, string name, bool addRef = true) {
            var res = base.Get(node, unit, name, addRef);
            foreach (var value in _prototypes) {
                if (value.Value.Push()) {
                    try {
                        res = res.Union(value.Get(node, unit, name, addRef));
                    } finally {
                        value.Value.Pop();
                    }
                }
            }
            return res;
        }

        new class MergedPropertyDescriptor : IPropertyDescriptor {
            private readonly InheritsPrototypeValue _instance;
            private readonly string _name;

            public MergedPropertyDescriptor(InheritsPrototypeValue instance, string name) {
                _instance = instance;
                _name = name;
            }

            public IAnalysisSet GetValue(Node node, AnalysisUnit unit, ProjectEntry declaringScope, IAnalysisSet @this, bool addRef) {
                IAnalysisSet res = AnalysisSet.Empty;
                foreach (var prototype in _instance._prototypes) {
                    if (prototype.Value.Push()) {
                        try {
                            var value = prototype.Value.GetProperty(node, unit, _name);
                            if (value != null) {
                                res = res.Union(value.GetValue(node, unit, declaringScope, @this, addRef));
                            }
                        } finally {
                            prototype.Value.Pop();
                        }
                    }
                }
                return res;
            }

            public bool IsEphemeral {
                get {
                    return true;
                }
            }
        }

        internal override IPropertyDescriptor GetProperty(Node node, AnalysisUnit unit, string name) {
            if (_prototypes.Count == 0) {
                return null;
            } else if (_prototypes.Count == 1) {
                return _prototypes.First().Value.GetProperty(node, unit, name);
            }

            return new MergedPropertyDescriptor(this, name);
        }

        internal override Dictionary<string, IAnalysisSet> GetAllMembers(ProjectEntry accessor) {
            var res = base.GetAllMembers(accessor);
            foreach (var value in _prototypes) {
                if (value.Value.Push()) {
                    try {
                        foreach (var keyValue in value.Value.GetAllMembers(accessor)) {
                            MergeTypes(res, keyValue.Key, keyValue.Value);
                        }
                    } finally {
                        value.Value.Pop();
                    }
                }
            }
            return res;
        }
    }
}
