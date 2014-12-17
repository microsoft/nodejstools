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

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Analysis unit used for when we query the results of the analysis.
    /// 
    /// This analysis unit won't add references and side effects won't
    /// propagate types.
    /// </summary>
    [Serializable]
    sealed class EvalAnalysisUnit : AnalysisUnit {
        internal EvalAnalysisUnit(Statement ast, JsAst tree, EnvironmentRecord scope)
            : base(ast, tree, scope) {
        }
    }
}
