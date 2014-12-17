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

namespace Microsoft.NodejsTools.Debugger.Serialization {
    interface IEvaluationResultFactory {
        /// <summary>
        /// Creates a new <see cref="NodeEvaluationResult" />.
        /// </summary>
        /// <param name="variable">Variable provider.</param>
        /// <returns>Result.</returns>
        NodeEvaluationResult Create(INodeVariable variable);
    }
}