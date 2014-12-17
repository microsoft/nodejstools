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

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Like a VariableDef, but used for tracking key/value pairs in a dictionary.
    /// 
    /// The full key/value pair represenets the dependent data, and just like a VariableDef
    /// it's added based upon the project entry which is being analyzed.
    /// 
    /// This ultimately enables us to resolve an individual key back to a specific
    /// value when individual keys are being used.  That lets us preserve strong type
    /// information across dictionaries which are polymorphic based upon their keys.
    /// 
    /// This works best for dictionaries whoses keys are objects that we closel track.
    /// Currently that includes strings, types, small integers, etc...
    /// </summary>
    [Serializable]
    class DependentKeyValue : DependentData<KeyValueDependencyInfo> {
        private static Dictionary<AnalysisProxy, IAnalysisSet> EmptyDict = new Dictionary<AnalysisProxy, IAnalysisSet>();
#if FALSE   // Currently unused but could come back
        private IAnalysisSet _allValues;
#endif

        protected override KeyValueDependencyInfo NewDefinition(int version) {
            return new KeyValueDependencyInfo(version);
        }

        public bool AddTypes(AnalysisUnit unit, IAnalysisSet keyTypes, IAnalysisSet valueTypes, bool enqueue = true) {
            return AddTypes(unit.ProjectEntry, unit.Analyzer, keyTypes, valueTypes, enqueue);
        }

        public bool AddTypes(ProjectEntry projectEntry, JsAnalyzer projectState, IAnalysisSet keyTypes, IAnalysisSet valueTypes, bool enqueue = true) {
            bool anyAdded = false;
            if (keyTypes.Count > 0) {
                var dependencies = GetDependentItems(projectEntry);

                if (dependencies.KeyValues.Count > projectState.Limits.DictKeyTypes) {
                    dependencies.MakeUnionStronger();
                }

                foreach (var key in keyTypes) {
                    IAnalysisSet values;
                    if (!dependencies.KeyValues.TryGetValue(key, out values)) {
                        values = AnalysisSet.Create(valueTypes);
                        anyAdded = true;
                    } else {
                        bool added;
                        values = values.Union(valueTypes, out added);
                        anyAdded |= added;
                    }
                    if (anyAdded && values.Count > projectState.Limits.DictValueTypes) {
                        values = values.AsStrongerUnion();
                    }
                    dependencies.KeyValues[key] = values;
                }

#if FALSE   // Currently unused but could come back
                if (anyAdded) {
                    _allValues = null;
                }
#endif
                if (anyAdded && enqueue) {
                    EnqueueDependents();
                }
            }
            return anyAdded;
        }

        public IAnalysisSet KeyTypes {
            get {
                if (_dependencies.Count == 0) {
                    return AnalysisSet.Empty;
                }

                var res = AnalysisSet.Empty; ;
                foreach (var keyValue in _dependencies.Values) {
                    res = res.Union(keyValue.KeyValues.Keys);
                }

                return res;
            }
        }

#if FALSE   // Currently unused but could come back
        public IAnalysisSet AllValueTypes {
            get {
                if (_allValues != null) {
                    return _allValues;
                }
                if (_dependencies.Count == 0) {
                    return AnalysisSet.Empty;
                }

                var res = AnalysisSet.Empty;
                foreach (var dependency in _dependencies.Values) {
                    foreach (var keyValue in dependency.KeyValues) {
                        res = res.Union(keyValue.Value);
                    }
                }

                _allValues = res;
                return res;
            }
        }
#endif

        public IAnalysisSet GetValueType(IAnalysisSet keyTypes, AnalysisUnit accessor, ProjectEntry declaringScope) {
            var res = AnalysisSet.Empty;
            if (_dependencies.Count != 0) {
                AnalysisProxy ns = keyTypes as AnalysisProxy;
                foreach (var keyValue in _dependencies) {
                    if (!IsVisible(accessor.ProjectEntry, declaringScope, keyValue.Key)) {
                        continue;
                    }
                    IAnalysisSet union;
                    if (ns != null) {
                        // optimize for the case where we're just looking up
                        // a single AnalysisValue object which hasn't been copied into
                        // a set
                        if (keyValue.Value.KeyValues.TryGetValue(ns, out union)) {
                            res = res.Union(union);
                        }
                    } else {
                        foreach (var keyType in keyTypes) {
                            if (keyValue.Value.KeyValues.TryGetValue(keyType, out union)) {
                                res = res.Union(union);
                            }
                        }
                    }
                }

                if (res == null || res.Count == 0) {
                    // This isn't ideal, but it's the best we can do for now.  We could
                    // later receive a key which would satisfy getting this value type.  If that
                    // happens we will re-analyze the code which is doing this get.  But currently 
                    // we have no way to either remove the types that were previously returned, and 
                    // we have no way to schedule the re-analysis of the code which is doing the get
                    // after we've completed the analysis.  So we simply don't return AllValueTypes
                    // here.
                    return AnalysisSet.Empty;
                }
            }

            return res ?? AnalysisSet.Empty;
        }

#if FALSE   // Currently unused but could come back...
        public Dictionary<AnalysisProxy, IAnalysisSet> KeyValueTypes {
            get {
                if (_dependencies.Count != 0) {
                    Dictionary<AnalysisProxy, IAnalysisSet> res = null;
                    foreach (var mod in _dependencies.Values) {
                        if (res == null) {
                            res = new Dictionary<AnalysisProxy, IAnalysisSet>();
                            foreach (var keyValue in mod.KeyValues) {
                                res[keyValue.Key] = keyValue.Value;
                            }
                        } else {
                            foreach (var keyValue in mod.KeyValues) {
                                IAnalysisSet existing;
                                if (!res.TryGetValue(keyValue.Key, out existing)) {
                                    res[keyValue.Key] = keyValue.Value;
                                } else {
                                    res[keyValue.Key] = existing.Union(keyValue.Value, canMutate: false);
                                }
                            }
                        }
                    }
                    return res ?? EmptyDict;
                }

                return EmptyDict;
            }
        }

        /// <summary>
        /// Copies the key/value types from the provided DependentKeyValue into this
        /// DependentKeyValue.
        /// </summary>
        internal bool CopyFrom(DependentKeyValue dependentKeyValue, bool enqueue = true) {
            bool anyAdded = false;
            foreach (var otherDependency in dependentKeyValue._dependencies) {
                var deps = GetDependentItems(otherDependency.Key);

                // TODO: Is this correct?
                if (deps == otherDependency.Value) {
                    continue;
                }

                foreach (var keyValue in otherDependency.Value.KeyValues) {
                    IAnalysisSet union;
                    if (!deps.KeyValues.TryGetValue(keyValue.Key, out union)) {
                        deps.KeyValues[keyValue.Key] = union = keyValue.Value;
                        anyAdded = true;
                    } else {
                        bool added;
                        deps.KeyValues[keyValue.Key] = union.Union(keyValue.Value, out added, canMutate: false);
                        anyAdded |= added;
                    }
                }
            }

            if (anyAdded && enqueue) {
                EnqueueDependents();
            }
            return anyAdded;
        }

        /// <summary>
        /// Copies all of our key types into the provided VariableDef.
        /// </summary>
        internal bool CopyKeysTo(VariableDef to) {
            bool added = false;
            foreach (var dependency in _dependencies) {
                added |= to.AddTypes(dependency.Key, dependency.Value.KeyValues.Keys);
            }

            if (added) {
                EnqueueDependents();
            }

            return added;
        }

        /// <summary>
        /// Copies all of our value types into the provided VariableDef.
        /// </summary>
        internal bool CopyValuesTo(VariableDef to) {
            bool added = false;
            foreach (var dependency in _dependencies) {
                foreach (var value in dependency.Value.KeyValues) {
                    added |= to.AddTypes(dependency.Key, value.Value);
                }
            }

            if (added) {
                EnqueueDependents();
            }

            return added;
        }
#endif
    }
}
