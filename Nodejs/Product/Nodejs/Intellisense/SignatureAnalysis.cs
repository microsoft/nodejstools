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
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.NodejsTools.Intellisense {
    public class SignatureAnalysis {
        private readonly string _text;
        private readonly int _paramIndex;
        private readonly ISignature[] _signatures;
        private readonly string _lastKeywordArgument;

        internal SignatureAnalysis(string text, int paramIndex, IList<ISignature> signatures, string lastKeywordArgument = null) {
            _text = text;
            _paramIndex = paramIndex;
            _signatures = new ISignature[signatures.Count];
            signatures.CopyTo(_signatures, 0);
            _lastKeywordArgument = lastKeywordArgument;
            Array.Sort(_signatures, (x, y) => x.Parameters.Count - y.Parameters.Count);
        }

        public string Text {
            get {
                return _text;
            }
        }

        public int ParameterIndex {
            get {
                return _paramIndex;
            }
        }

        public string LastKeywordArgument {
            get {
                return _lastKeywordArgument;
            }
        }

        public IList<ISignature> Signatures {
            get {
                return _signatures;
            }
        }
    }
}
