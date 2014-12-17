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
    class NullValue : AnalysisValue {
        private readonly JsAnalyzer _analyzer;

        public NullValue(JsAnalyzer analyzer)
            : base(analyzer._builtinEntry) {
            _analyzer = analyzer;
        }

        public override string Description {
            get {
                return "null";
            }
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Null;
            }
        }

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Null;
            }
        }
    }
}
