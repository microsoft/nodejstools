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

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    public interface IPythonInterpreterWithProjectReferences {
        /// <summary>
        /// Asynchronously loads the assocated project reference into the interpreter.
        /// 
        /// Returns a new task which can be waited upon for completion of the reference being added.
        /// </summary>
        /// <remarks>New in 2.0.</remarks>
        Task AddReferenceAsync(ProjectReference reference, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Removes the associated project reference from the interpreter.
        /// </summary>
        /// <remarks>New in 2.0.</remarks>
        void RemoveReference(ProjectReference reference);
    }
#endif
}
