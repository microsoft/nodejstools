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

using Microsoft.NodejsTools.Analysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    internal class NodejsParameter : IParameter {
        private readonly ISignature _signature;
        private readonly ParameterResult _param;
        private readonly string _documentation;
        private readonly Span _locus, _ppLocus;

        public NodejsParameter(ISignature signature, ParameterResult param, Span locus, Span ppLocus, string documentation = null) {
            _signature = signature;
            _param = param;
            _locus = locus;
            _ppLocus = ppLocus;
            _documentation = (documentation ?? _param.Documentation).LimitLines(15, stopAtFirstBlankLine: true);
        }

        public string Documentation {
            get { return _documentation; }
        }

        public Span Locus {
            get { return _locus; }
        }

        public string Name {
            get { return _param.Name; }
        }

        public ISignature Signature {
            get { return _signature; }
        }

        public Span PrettyPrintedLocus {
            get { return _ppLocus; }
        }
    }
}
