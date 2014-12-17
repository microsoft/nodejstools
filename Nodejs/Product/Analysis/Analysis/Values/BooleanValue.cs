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

namespace Microsoft.NodejsTools.Analysis.Values {
    [Serializable]
    class BooleanValue : NonObjectValue {
        internal readonly bool _value;
        private readonly JsAnalyzer _analyzer;

        public BooleanValue(bool value, JsAnalyzer javaScriptAnalyzer)
            : base(javaScriptAnalyzer._builtinEntry) {
            _value = value;
            _analyzer = javaScriptAnalyzer;
            javaScriptAnalyzer.AnalysisValueCreated(typeof(BooleanValue));
        }

        public override string ShortDescription {
            get {
                return "boolean";
            }
        }

        public override string Description {
            get {
                return "boolean";
            }
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Boolean;
            }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Boolean;
            }
        }

        public override AnalysisValue Prototype {
            get { return _analyzer._booleanPrototype; }
        }
    }
}
