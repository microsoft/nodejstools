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

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class SignatureHelpSource : ISignatureHelpSource {
        private readonly ITextBuffer _textBuffer;

        public SignatureHelpSource(SignatureHelpSourceProvider provider, ITextBuffer textBuffer) {
            _textBuffer = textBuffer;
        }

        public ISignature GetBestMatch(ISignatureHelpSession session) {
            return null;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, System.Collections.Generic.IList<ISignature> signatures) {
            var span = CompletionSource.GetApplicableSpan(session, _textBuffer);

            var sigs = VsProjectAnalyzer.GetSignatures(_textBuffer.CurrentSnapshot, span);

            ISignature curSig = null;

            foreach (var sig in sigs.Signatures) {
                if (sigs.ParameterIndex == 0 || sig.Parameters.Count > sigs.ParameterIndex) {
                    curSig = sig;
                    break;
                }
            }

            foreach (var sig in sigs.Signatures) {
                signatures.Add(sig);
            }

            if (curSig != null) {
                // save the current sig so we don't need to recalculate it (we can't set it until
                // the signatures are added by our caller).
                session.Properties.AddProperty(typeof(NodejsSignature), curSig);
            }
        }

        public void Dispose() {
        }
    }
}
