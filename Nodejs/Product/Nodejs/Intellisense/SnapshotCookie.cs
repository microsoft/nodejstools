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
using Microsoft.VisualStudio.Text;

namespace Microsoft.NodejsTools.Intellisense {
    class SnapshotCookie : IAnalysisCookie, IAnalysisSerializeAsNull {
        private readonly ITextSnapshot _snapshot;
        
        public SnapshotCookie(ITextSnapshot snapshot) {
            _snapshot = snapshot;
        }

        public ITextSnapshot Snapshot {
            get {
                return _snapshot;
            }
        }

        #region IAnalysisCookie Members

        public string GetLine(int lineNo) {
            return _snapshot.GetLineFromLineNumber(lineNo - 1).GetText();
        }

        #endregion
    }
}
