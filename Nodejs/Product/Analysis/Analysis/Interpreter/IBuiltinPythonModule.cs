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
    /// <summary>
    /// Represents a built-in Python module.  The built-in module needs to respond to
    /// some extra requests for members by name which supports getting hidden members
    /// such as "NoneType" which logically live in the built-in module but don't actually
    /// exist there by name.
    /// 
    /// The full list of types which will be accessed through GetAnyMember but don't exist
    /// in the built-in module includes:
    ///     NoneType
    ///     generator
    ///     builtin_function
    ///     builtin_method_descriptor
    ///     function
    ///     ellipsis
    ///     
    /// These are the addition types in BuiltinTypeId which do not exist in __builtin__.
    /// </summary>
    public interface IBuiltinPythonModule : IPythonModule {
        IMember GetAnyMember(string name);
    }
}
