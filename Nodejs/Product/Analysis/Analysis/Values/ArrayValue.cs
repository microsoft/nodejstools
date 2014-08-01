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
using System.Linq;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class ArrayValue : ObjectValue {
        private IAnalysisSet _unionType;        // all types that have been seen
        private TypedDef[] _indexTypes;     // types for known indices
        private readonly Node _node;

        public ArrayValue(TypedDef[] indexTypes, ProjectEntry entry, Node node)
            : base(entry, entry.Analyzer._arrayFunction) {
            _indexTypes = indexTypes;
            _node = node;
            entry.Analyzer.AnalysisValueCreated(typeof(ArrayValue));
        }

        public override string ToString() {
            return String.Format(
                "Array literal: {0} - {1}\r\n{2}",
                _node.GetStart(ProjectEntry.Tree),
                _node.GetEnd(ProjectEntry.Tree),
                ProjectEntry.FilePath
            );
        }

        public override IAnalysisSet GetEnumerationValues(Node node, AnalysisUnit unit) {
            return this.ProjectState._emptyStringValue.SelfSet;
        }

        public override IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            int? constIndex = GetConstantIndex(index);

            if (constIndex != null && constIndex.Value < IndexTypes.Length) {
                // TODO: Warn if outside known index and no appends?
                IndexTypes[constIndex.Value].AddDependency(unit);
                return IndexTypes[constIndex.Value].GetTypes(unit, ProjectEntry);
            }

            if (IndexTypes.Length == 0) {
                IndexTypes = new[] { new TypedDef() };
            }

            IndexTypes[0].AddDependency(unit);

            EnsureUnionType();
            return UnionType;
        }

        public override string ObjectDescription {
            get {
                return "array";
            }
        }

        public IAnalysisSet UnionType {
            get {
                EnsureUnionType();
                return _unionType;
            }
            set { _unionType = value; }
        }

        public TypedDef[] IndexTypes {
            get { return _indexTypes; }
            set { _indexTypes = value; }
        }

        protected void EnsureUnionType() {
            if (_unionType == null) {
                IAnalysisSet unionType = AnalysisSet.EmptyUnion;
                if (Push()) {
                    try {
                        foreach (var set in _indexTypes) {
                            unionType = unionType.Union(set.TypesNoCopy);
                        }
                    } finally {
                        Pop();
                    }
                }
                _unionType = unionType;
            }
        }

        internal static int? GetConstantIndex(IAnalysisSet index) {
            int? constIndex = null;
            int typeCount = 0;
            foreach (var type in index) {
                object constValue = type.Value.GetConstantValue();
                if (constValue != null && constValue is int) {
                    constIndex = (int)constValue;
                }

                typeCount++;
            }
            if (typeCount != 1) {
                constIndex = null;
            }
            return constIndex;
        }

        internal bool AddTypes(AnalysisUnit unit, IAnalysisSet[] types) {
            if (_indexTypes.Length < types.Length) {
                _indexTypes = _indexTypes.Concat(TypedDef.Generator).Take(types.Length).ToArray();
            }

            bool added = false;
            for (int i = 0; i < types.Length; i++) {
                added |= _indexTypes[i].MakeUnionStrongerIfMoreThan(ProjectState.Limits.IndexTypes, types[i]);
                added |= _indexTypes[i].AddTypes(unit, types[i]);
            }

            if (added) {
                _unionType = null;
            }

            return added;
        }

        internal override bool UnionEquals(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var array = av as ArrayValue;
                if (array != null) {
                    return true;
                }
            }

            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                return typeof(ArrayValue).GetHashCode();
            }

            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
            if (strength >= MergeStrength.ToBaseClass) {
                var array = av as ArrayValue;
                if (array != null) {
                    return this;
                }
            }

            return base.UnionMergeTypes(av, strength);
        }

    }
}
