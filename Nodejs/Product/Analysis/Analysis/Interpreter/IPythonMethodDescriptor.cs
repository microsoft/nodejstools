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


namespace Microsoft.NodejsTools.Interpreter {
#if FALSE
    /// <summary>
    /// Represents a method descriptor for an instance of a function.
    /// </summary>
    public interface IPythonMethodDescriptor : IMember {
        /// <summary>
        /// The built-in function that the method descriptor wraps.
        /// </summary>
        IPythonFunction Function {
            get;
        }

        /// <summary>
        /// True if the method is already bound to an instance.
        /// </summary>
        bool IsBound {
            get;
        }
    }
#endif
}
