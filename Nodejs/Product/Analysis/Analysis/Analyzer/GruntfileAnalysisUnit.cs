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

namespace Microsoft.NodejsTools.Analysis.Analyzer {
    /// <summary>
    /// Provides an extra step to analyze the exported function as if it was
    /// called back with the grunt object.
    /// </summary>
    class GruntfileAnalysisUnit : AnalysisUnit {
        public GruntfileAnalysisUnit(Parsing.JsAst tree, ModuleEnvironmentRecord environment)
            : base(tree, environment) {
        }

        internal override void AnalyzeWorker(DDG ddg, System.Threading.CancellationToken cancel) {
            base.AnalyzeWorker(ddg, cancel);

            // perform the callback for the exported function so
            // we provide intellisense against the grunt parameter.
            var grunt = Analyzer.Modules.RequireModule(
                Ast,
                this,
                "grunt",
                DeclaringModuleEnvironment.Name
            );
            ProjectEntry.GetModule(this).Get(
                Ast,
                this,
                "exports",
                false
            ).Call(
                Ast,
                this,
                null,
                new[] { grunt }
            );
        }
    }
}
