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

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Utilities;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif

    [Export(typeof(IClassifierProvider)), ContentType(ReplConstants.ReplContentTypeName)]
    class ReplAggregateClassifierProvider : IClassifierProvider {
        [Import] private IBufferGraphFactoryService _bufferGraphFact = null; // set via MEF

        #region IClassifierProvider Members

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            ReplAggregateClassifier res;
            if (!textBuffer.Properties.TryGetProperty<ReplAggregateClassifier>(typeof(ReplAggregateClassifier), out res)) {
                res = new ReplAggregateClassifier(_bufferGraphFact, textBuffer);
                textBuffer.Properties.AddProperty(typeof(ReplAggregateClassifier), res);
            }
            return res;
        }

        #endregion
    }
}
