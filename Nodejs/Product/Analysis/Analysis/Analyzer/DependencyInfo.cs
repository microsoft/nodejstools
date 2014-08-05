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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// contains information about dependencies.  Each DependencyInfo is 
    /// attached to a VariableDef in a dictionary keyed off of the ProjectEntry.
    /// 
    /// DependentUnits -> What needs to change if this VariableDef is updated.
    /// </summary>
    [Serializable]
    internal class DependencyInfo {
        private int _version;   // not readonly for serialization perf
        private ISet<AnalysisUnit> _dependentUnits;
        public DependencyInfo(int version) {
            _version = version;
        }

        public ISet<AnalysisUnit> DependentUnits {
            get {
                return _dependentUnits; 
            }
        }

        public bool AddDependentUnit(AnalysisUnit unit) {
            return AddValue(ref _dependentUnits, unit);
        }

        public int Version {
            get {
                return _version;
            }
        }

        internal static bool AddValue(ref ISet<AnalysisUnit> references, AnalysisUnit value) {
            AnalysisUnit prevNs;
            SetOfTwo<AnalysisUnit> prevSetOfTwo;
            if (references == null) {
                references = value;
                return true;
            } else if ((prevNs = references as AnalysisUnit) != null) {
                if (references != value) {
                    references = new SetOfTwo<AnalysisUnit>(prevNs, value);
                    return true;
                }
            } else if ((prevSetOfTwo = references as SetOfTwo<AnalysisUnit>) != null) {
                if (value != prevSetOfTwo.Value1 && value != prevSetOfTwo.Value2) {
                    references = new HashSet<AnalysisUnit>(prevSetOfTwo);
                    references.Add(value);
                    return true;
                }
            } else {
                return references.Add(value);
            }
            return false;
        }
    }

    [Serializable]
    internal class KeyValueDependencyInfo : DependencyInfo {
        internal AnalysisDictionary<AnalysisProxy, IAnalysisSet> KeyValues = new AnalysisDictionary<AnalysisProxy, IAnalysisSet>();

        public KeyValueDependencyInfo(int version)
            : base(version) {
        }

        internal void MakeUnionStronger() {
            var cmp = KeyValues.Comparer as UnionComparer;
            if (cmp != null && cmp.Strength == UnionComparer.MAX_STRENGTH) {
                return;
            }
            if (cmp == null) {
                cmp = UnionComparer.Instances[0];
            } else {
                cmp = UnionComparer.Instances[cmp.Strength + 1];
            }

            var matches = new Dictionary<AnalysisProxy, List<KeyValuePair<AnalysisProxy, IAnalysisSet>>>(cmp);
            foreach (var keyValue in KeyValues) {
                List<KeyValuePair<AnalysisProxy, IAnalysisSet>> values;
                if (!matches.TryGetValue(keyValue.Key, out values)) {
                    values = matches[keyValue.Key] = new List<KeyValuePair<AnalysisProxy, IAnalysisSet>>();
                }
                values.Add(keyValue);
            }

            KeyValues = new AnalysisDictionary<AnalysisProxy, IAnalysisSet>(cmp);
            foreach (var list in matches.Values) {
                bool dummy;
                var key = list[0].Key;
                var value = list[0].Value.AsUnion(cmp, out dummy);

                foreach (var item in list.Skip(1)) {
                    key = cmp.MergeTypes(key, item.Key, out dummy);
                    value = value.Union(item.Value);
                }

                KeyValues[key] = value;
            }
        }
    }

    /// <summary>
    /// Types -> Types that the VariableDef has received from the Module.
    /// </summary>
    [Serializable]
    internal class TypedDependencyInfo : DependencyInfo {
        private IAnalysisSet _types;
#if FULL_VALIDATION
        internal int _changeCount = 0;
#endif

        public TypedDependencyInfo(int version)
            : this(version, AnalysisSet.Empty) { }

        public TypedDependencyInfo(int version, IAnalysisSet emptySet)
            : base(version) {
            _types = emptySet;
        }

        static bool TAKE_COPIES = false;

        public bool AddType(AnalysisProxy proxy) {
            bool wasChanged;
            IAnalysisSet prev;
            if (TAKE_COPIES) {
                prev = _types.Clone();
            } else {
                prev = _types;
            }
            _types = prev.Add(proxy, out wasChanged);
#if FULL_VALIDATION
            _changeCount += wasChanged ? 1 : 0;
            // The value doesn't mean anything, we just want to know if a variable is being
            // updated too often.
            Validation.Assert<ChangeCountExceededException>(_changeCount < 10000);
#endif
            return wasChanged;
        }

        internal bool MakeUnion(int strength) {
            bool wasChanged;
            _types = _types.AsUnion(strength, out wasChanged);
            return wasChanged;
        }

        public IAnalysisSet ToImmutableTypeSet() {
            return _types.Clone();
        }

        public IAnalysisSet Types {
            get {
                return _types;
            }
        }

    }

    [Serializable]
    internal class ReferenceableDependencyInfo : TypedDependencyInfo {
        public ISet<EncodedLocation> _references, _assignments;

        public ReferenceableDependencyInfo(int version)
            : base(version) { }

        public ReferenceableDependencyInfo(int version, IAnalysisSet emptySet)
            : base(version, emptySet) {
        }

        public bool AddReference(EncodedLocation location) {
            return HashSetExtensions.AddValue(ref _references, location);
        }

        public IEnumerable<EncodedLocation> References {
            get {
                return _references;
            }
        }

        public bool AddAssignment(EncodedLocation location) {
            return HashSetExtensions.AddValue(ref _assignments, location);
        }

        public IEnumerable<EncodedLocation> Assignments {
            get {
                return _assignments;
            }
        }
    }
}
