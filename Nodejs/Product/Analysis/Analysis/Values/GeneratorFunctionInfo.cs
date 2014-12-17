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

using System.Collections.Generic;
using Microsoft.NodejsTools.Parsing;


namespace Microsoft.NodejsTools.Analysis.Values {
#if FALSE
    /// <summary>
    /// Represents a function which is a generator (it contains yield expressions)
    /// </summary>
    class GeneratorFunctionInfo : FunctionInfo {
        private readonly GeneratorInfo _generator;

        internal GeneratorFunctionInfo(AnalysisUnit unit)
            : base(unit) {
            _generator = new GeneratorInfo(unit);
        }

        public GeneratorInfo Generator {
            get {
                return _generator;
            }
        }

        public override ISet<Namespace> Call(Node node, AnalysisUnit unit, ISet<Namespace>[] args) {
            _generator.Callers.AddDependency(unit);

            _generator.AddReturn(node, unit, base.Call(node, unit, args, keywordArgNames));
            
            return _generator.SelfSet;
        }
    }
#endif
}
