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
using System.Text;
using Microsoft.NodejsTools.Analysis.Values;

namespace Microsoft.NodejsTools.Analysis {
    internal struct MemberResult {
        private readonly string _name;
        private string _completion;
        private string _documentation;
        private readonly Func<IEnumerable<AnalysisValue>> _vars;
        private readonly Func<JsMemberType> _type;

        internal MemberResult(string name, IEnumerable<AnalysisValue> vars) {
            _documentation = null;
            _name = _completion = name;
            _vars = () => vars;
            _type = null;
            _type = GetMemberType;            
        }

        public MemberResult(string name, JsMemberType type) {
            _name = _completion = name;
            _type = () => type;
            _vars = () => Empty;
            _documentation = null;
        }

        public MemberResult(string name, string documentation, JsMemberType type) {
            _name = _completion = name;
            _documentation = documentation;
            _type = () => type;
            _vars = () => Empty;
        }

        internal MemberResult(string name, string completion, IEnumerable<AnalysisValue> vars, JsMemberType? type) {
            _documentation = null;
            _name = name;
            _vars = () => vars;
            _completion = completion;
            if (type != null) {
                _type = () => type.Value;
            } else {
                _type = null;
                _type = GetMemberType;
            }
        }

        internal MemberResult(string name, Func<IEnumerable<AnalysisValue>> vars, Func<JsMemberType> type) {
            _documentation = null;
            _name = _completion = name;
            _vars = vars;
            _type = type;
        }

        public MemberResult FilterCompletion(string completion) {
            return new MemberResult(Name, completion, Namespaces, MemberType);
        }

        private static AnalysisValue[] Empty = new AnalysisValue[0];

        public string Name {
            get { return _name; }
        }

        public string Completion {
            get { return _completion; }
        }

        public string Documentation {
            get {
                if (_documentation != null) {
                    return _documentation;
                }

                var docSeen = new HashSet<string>();
                var typeSeen = new HashSet<string>();
                var docs = new List<string>();
                var types = new List<string>();

                var doc = new StringBuilder();

                foreach (var ns in _vars()) {
                    var docString = ns.Documentation;
                    if (docSeen.Add(docString)) {
                        docs.Add(docString);
                    }
                    var typeString = ns.ShortDescription;
                    if (typeSeen.Add(typeString)) {
                        types.Add(typeString);
                    }
                }

                if (types.Count == 0) {
                    doc.AppendLine("unknown type");
                } else if (types.Count == 1) {
                    doc.AppendLine(types[0]);
                } else {
                    var orStr = types.Count == 2 ? " or " : ", or ";
                    doc.AppendLine(string.Join(", ", types.Take(types.Count - 1)) + orStr + types.Last());
                }
                doc.AppendLine();
                foreach (var str in docs.OrderBy(s => s)) {
                    doc.AppendLine(str);
                    doc.AppendLine();
                }
                return Utils.CleanDocumentation(doc.ToString());
            }
        }

        public JsMemberType MemberType {
            get {
                return _type();
            }
        }

        private JsMemberType GetMemberType() {
            JsMemberType result = JsMemberType.Unknown;

            var allVars = _vars().SelectMany(ns => {
                return Enumerable.Repeat(ns, 1);
            });

            foreach (var ns in allVars) {
                var nsType = ns.MemberType;
                if (result == JsMemberType.Unknown &&
                    (ns.TypeId == BuiltinTypeId.Null || 
                    nsType == JsMemberType.Undefined)) {
                    result = nsType;
                } else if (result == JsMemberType.Unknown || 
                    result == JsMemberType.Null || 
                    result == JsMemberType.Undefined) {
                    result = nsType;
                } else if (result == nsType) {
                    // No change
                } else {
                    return JsMemberType.Multiple;
                }
            }
            if (result == JsMemberType.Unknown) {
                return JsMemberType.Object;
            }
            return result;
        }

        internal IEnumerable<AnalysisValue> Namespaces {
            get {
                return _vars();
            }
        }

        /// <summary>
        /// Gets the location(s) for the member(s) if they are available.
        /// 
        /// New in 1.5.
        /// </summary>
        public IEnumerable<LocationInfo> Locations {
            get {
                foreach (var ns in _vars()) {
                    foreach (var location in ns.Locations) {
                        yield return location;
                    }
                }
            }
        }

        public override bool Equals(object obj) {
            if (!(obj is MemberResult)) {
                return false;
            }

            return Name == ((MemberResult)obj).Name;
        }

        public override int GetHashCode() {
            return Name.GetHashCode();
        }
    }
}
