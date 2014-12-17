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
using System.Text;
using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class QuickInfoSource : IQuickInfoSource {
        private readonly ITextBuffer _textBuffer;
        private readonly QuickInfoSourceProvider _provider;
        private IQuickInfoSession _curSession;

        public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
            _provider = provider;
        }

        #region IQuickInfoSource Members

        public void AugmentQuickInfoSession(IQuickInfoSession session, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            if (_curSession != null && !_curSession.IsDismissed) {
                _curSession.Dismiss();
                _curSession = null;
            }

            _curSession = session;
            _curSession.Dismissed += CurSessionDismissed;

            var vars = AnalyzeExpression(session);

            AugmentQuickInfoWorker(vars, quickInfoContent, out applicableToSpan);
        }

        private ExpressionAnalysis AnalyzeExpression(IQuickInfoSession session) {
            return VsProjectAnalyzer.AnalyzeExpression(
                _textBuffer.CurrentSnapshot,
                session.CreateTrackingSpan(_textBuffer),
                false
            );
        }

        private void CurSessionDismissed(object sender, EventArgs e) {
            _curSession = null;
        }

        internal static void AugmentQuickInfoWorker(ExpressionAnalysis vars, System.Collections.Generic.IList<object> quickInfoContent, out ITrackingSpan applicableToSpan) {
            applicableToSpan = vars.Span;
            if (applicableToSpan == null || String.IsNullOrWhiteSpace(vars.Expression)) {
                return;
            }

            bool first = true;
            var result = new StringBuilder();
            int count = 0;
            List<AnalysisValue> listVars = new List<AnalysisValue>(vars.Values);
            HashSet<string> descriptions = new HashSet<string>();
            bool multiline = false;
            foreach (var v in listVars) {
                string description = null;
                if (listVars.Count == 1) {
                    if (!String.IsNullOrWhiteSpace(v.Description)) {
                        description = v.Description;
                    }
                } else {
                    if (!String.IsNullOrWhiteSpace(v.ShortDescription)) {
                        description = v.ShortDescription;
                    }
                }

                description = description.LimitLines();

                if (description != null && descriptions.Add(description)) {
                    if (first) {
                        first = false;
                    } else {
                        if (result.Length == 0 || result[result.Length - 1] != '\n') {
                            result.Append(", ");
                        } else {
                            multiline = true;
                        }
                    }
                    result.Append(description);
                    count++;
                }
            }

            string expr = vars.Expression;
            if (expr.Length > 4096) {
                expr = expr.Substring(0, 4093) + "...";
            }
            if (multiline) {
                result.Insert(0, expr + ": " + Environment.NewLine);
            } else if (result.Length > 0) {
                result.Insert(0, expr + ": ");
            } else {
                result.Append(expr);
                result.Append(": ");
                result.Append("<unknown type>");
            }

            quickInfoContent.Add(result.ToString());
        }

        #endregion

        public void Dispose() {
        }

    }
}
