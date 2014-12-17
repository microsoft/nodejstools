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
    /// Specifies the kind of reference.  Currently we support references to .NET
    /// assemblies for IronPython and .pyds for C Python.
    /// </summary>
    public enum ProjectReferenceKind {
        None,
        /// <summary>
        /// The reference is to a .NET assembly.  The name is a fully qualified path to
        /// the assembly.
        /// </summary>
        Assembly,
        /// <summary>
        /// The reference is to a Python extension module.  The name is a fully qualified
        /// path to the .pyd file.
        /// </summary>
        ExtensionModule
    }
#endif
}
