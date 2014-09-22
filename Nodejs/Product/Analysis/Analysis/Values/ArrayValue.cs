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
        private TypedDef _unionType;        // all types that have been seen
        private TypedDef[] _indexTypes;     // types for known indices
        private readonly Node _node;

        public ArrayValue(TypedDef[] indexTypes, ProjectEntry entry, Node node)
            : base(entry, entry.Analyzer._arrayPrototype) {
            _indexTypes = indexTypes;
            _unionType = new TypedDef();
            _node = node;
            entry.Analyzer.AnalysisValueCreated(GetType());
        }

        public override string ToString() {
            return String.Format(
                "Array literal: {0} - {1}\r\n{2}",
                _node.GetStart(ProjectEntry.Tree.LocationResolver),
                _node.GetEnd(ProjectEntry.Tree.LocationResolver),
                ProjectEntry.FilePath
            );
        }

        public virtual void ForEach(Node node, AnalysisUnit unit, IAnalysisSet @this, IAnalysisSet[] args) {
            for (int i = 0; i < IndexTypes.Length; i++) {
                foreach (var indexType in IndexTypes) {
                    args[0].Call(
                        node,
                        unit,
                        @this,
                        new IAnalysisSet[] { 
                            indexType.GetTypes(unit, ProjectEntry), 
                            AnalysisSet.Empty, 
                            @this ?? AnalysisSet.Empty
                        }
                    );
                }
            }
        }

        public override IAnalysisSet GetEnumerationValues(Node node, AnalysisUnit unit) {
            return this.ProjectState._zeroIntValue.SelfSet;
        }

        public override IAnalysisSet GetIndex(Node node, AnalysisUnit unit, IAnalysisSet index) {
            double? constIndex = GetConstantIndex(index);

            if (constIndex != null && constIndex.Value < IndexTypes.Length) {
                // TODO: Warn if outside known index and no appends?
                IndexTypes[(int)constIndex.Value].AddDependency(unit);
                return IndexTypes[(int)constIndex.Value].GetTypes(unit, ProjectEntry);
            }

            _unionType.AddDependency(unit);
            return _unionType.GetTypes(unit, ProjectEntry);
        }

        public override void SetIndex(Node node, AnalysisUnit unit, IAnalysisSet index, IAnalysisSet value) {
            double? constIndex = GetConstantIndex(index);

            if (constIndex != null && constIndex.Value < IndexTypes.Length) {
                // TODO: Warn if outside known index and no appends?
                IndexTypes[(int)constIndex.Value].AddTypes(unit, value, declaringScope: ProjectEntry);
            }

            _unionType.AddTypes(unit, value, declaringScope: ProjectEntry);

            base.SetIndex(node, unit, index, value);
        }

        public void PushValue(Node node, AnalysisUnit unit, IAnalysisSet value) {
            _unionType.AddTypes(unit, value, true, ProjectEntry);
        }

        public IAnalysisSet PopValue(Node node, AnalysisUnit unit) {
            return _unionType.GetTypes(unit, ProjectEntry);
        }

        public override string ObjectDescription {
            get {
                return "array";
            }
        }

        public TypedDef[] IndexTypes {
            get { return _indexTypes; }
            set { _indexTypes = value; }
        }

        internal static double? GetConstantIndex(IAnalysisSet index) {
            double? constIndex = null;
            int typeCount = 0;
            foreach (var type in index) {
                constIndex = type.Value.GetNumberValue();
                typeCount++;
            }
            if (typeCount != 1 ||
                (constIndex != null && (constIndex % 1) != 0)) {
                constIndex = null;
            }
            return constIndex;
        }

        internal void AddTypes(AnalysisUnit unit, int index, IAnalysisSet types) {
            _indexTypes[index].MakeUnionStrongerIfMoreThan(ProjectState.Limits.IndexTypes, types);
            _indexTypes[index].AddTypes(unit, types, declaringScope: DeclaringModule);

            _unionType.MakeUnionStrongerIfMoreThan(ProjectState.Limits.IndexTypes, types);
            _unionType.AddTypes(unit, types, declaringScope: DeclaringModule);
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
