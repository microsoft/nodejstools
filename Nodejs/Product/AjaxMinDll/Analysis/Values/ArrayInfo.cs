using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    class ArrayInfo : ObjectInfo {
        private IAnalysisSet _unionType;        // all types that have been seen
        private VariableDef[] _indexTypes;     // types for known indices

        public ArrayInfo(VariableDef[] indexTypes, ProjectEntry entry)
            : base(entry) {
            _indexTypes = indexTypes;
        }

        public override IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            int? constIndex = GetConstantIndex(index);

            if (constIndex != null && constIndex.Value < IndexTypes.Length) {
                // TODO: Warn if outside known index and no appends?
                IndexTypes[constIndex.Value].AddDependency(unit);
                return IndexTypes[constIndex.Value].Types;
            }

            if (IndexTypes.Length == 0) {
                IndexTypes = new[] { new VariableDef() };
            }

            IndexTypes[0].AddDependency(unit);

            EnsureUnionType();
            return UnionType;
        }

        public IAnalysisSet UnionType {
            get {
                EnsureUnionType();
                return _unionType;
            }
            set { _unionType = value; }
        }

        public VariableDef[] IndexTypes {
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
                object constValue = type.GetConstantValue();
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
                _indexTypes = _indexTypes.Concat(VariableDef.Generator).Take(types.Length).ToArray();
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
    }
}
