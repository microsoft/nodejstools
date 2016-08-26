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

namespace Microsoft.NodejsTools.Repl {
    [Export(typeof(IReplEvaluatorProvider))]
    class NodejsReplEvaluatorProvider : IReplEvaluatorProvider {
        internal const string NodeReplId = "{E4AC36B7-EDC5-4AD2-B758-B5416D520705}";
        
        #region IAltReplEvaluatorProvider Members

        public IReplEvaluator GetEvaluator(string replId) {
            if (replId == NodeReplId) {
                return new NodejsReplEvaluator();
            }
            return null;
        }

        #endregion
    }
}
