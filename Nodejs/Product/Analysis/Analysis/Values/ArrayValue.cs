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
