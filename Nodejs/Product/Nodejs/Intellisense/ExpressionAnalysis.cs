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
using Microsoft.NodejsTools.Analysis;
using Microsoft.NodejsTools.Analysis.Values;
using Microsoft.NodejsTools.Parsing;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    /// <summary>
    /// Provides the results of analyzing a simple expression.  Returned from Analysis.AnalyzeExpression.
    /// </summary>
    internal class ExpressionAnalysis {
        private readonly string _expr;
        private readonly ModuleAnalysis _analysis;
        private readonly ITrackingSpan _span;
        private readonly int _index;
        private readonly VsProjectAnalyzer _analyzer;
        private readonly ITextSnapshot _snapshot;
        public static readonly ExpressionAnalysis Empty = new ExpressionAnalysis(null, String.Empty, null, 0, null, null);

        internal ExpressionAnalysis(VsProjectAnalyzer analyzer, string expression, ModuleAnalysis analysis, int index, ITrackingSpan span, ITextSnapshot snapshot) {
            _expr = expression;
            _analysis = analysis;
            _index = index;
            _span = span;
            _analyzer = analyzer;
            _snapshot = snapshot;
        }

        /// <summary>
        /// The expression which this is providing information about.
        /// </summary>
        public string Expression {
            get {
                return _expr;
            }
        }

        /// <summary>
        /// The span of the expression being analyzed.
        /// </summary>
        public ITrackingSpan Span {
            get {
                return _span;
            }
        }

        /// <summary>
        /// Gets all of the variables (storage locations) associated with the expression.
        /// </summary>
        public IEnumerable<IAnalysisVariable> Variables {
            get {
                if (_analysis != null) {
                    lock (_analyzer) {
                        return _analysis.GetVariablesByIndex(_expr, TranslatedIndex);
                    }
                }
                return new IAnalysisVariable[0];
            }
        }

        /// <summary>
        /// The possible values of the expression (types, constants, functions, modules, etc...)
        /// </summary>
        public IEnumerable<AnalysisValue> Values {
            get {
                if (_analysis != null) {
                    lock (_analyzer) {
                        return _analysis.GetValuesByIndex(_expr, TranslatedIndex);
                    }
                }
                return new AnalysisValue[0];
            }
        }

        public Expression GetEvaluatedExpression() {
            return Statement.GetExpression(_analysis.GetAstFromTextByIndex(_expr, TranslatedIndex).Block);
        }

        /// <summary>
        /// Returns the complete PythonAst for the evaluated expression.  Calling Statement.GetExpression on the Body
        /// of the AST will return the same expression as GetEvaluatedExpression.
        /// 
        /// New in 1.1.
        /// </summary>
        /// <returns></returns>
        public JsAst GetEvaluatedAst() {
            return _analysis.GetAstFromTextByIndex(_expr, TranslatedIndex);
        }

        private int TranslatedIndex {
            get {
                return VsProjectAnalyzer.TranslateIndex(_index, _snapshot, _analysis);
            }
        }
    }
}
