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

namespace Microsoft.NodejsTools.Parsing {
    internal class CollectingErrorSink : ErrorSink {
        private readonly List<ErrorResult> _errors = new List<ErrorResult>();
        private readonly List<ErrorResult> _warnings = new List<ErrorResult>();

        public override void OnError(JScriptExceptionEventArgs error) {
            var span = new SourceSpan(
                new SourceLocation(
                    error.Exception.Span.Start,
                    error.Error.StartLine,
                    error.Error.StartColumn
                ),
                new SourceLocation(
                    error.Exception.Span.End,
                    error.Error.EndLine,
                    error.Error.EndColumn
                )
            );
            var result = new ErrorResult(error.Error.Message, span, error.Error.ErrorCode);

            if (error.Error.IsError) {
                Errors.Add(result);
            } else {
                Warnings.Add(result);
            }
        }

        public List<ErrorResult> Errors {
            get {
                return _errors;
            }
        }

        public List<ErrorResult> Warnings {
            get {
                return _warnings;
            }
        }
    }
}
