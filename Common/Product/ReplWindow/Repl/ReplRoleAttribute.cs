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
using System.ComponentModel.Composition;

#if NTVS_FEATURE_INTERACTIVEWINDOW
namespace Microsoft.NodejsTools.Repl {
#else
namespace Microsoft.VisualStudio.Repl {
#endif
    /// <summary>
    /// Represents an interactive window role.
    /// 
    /// This attribute is a MEF contract and can be used to associate a REPL provider with its commands.
    /// This is new in 1.5.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#if INTERACTIVE_WINDOW
    public sealed class InteractiveWindowRoleAttribute : Attribute {
#else
    public sealed class ReplRoleAttribute : Attribute {
#endif
        private readonly string _name;

#if INTERACTIVE_WINDOW
        public InteractiveWindowRoleAttribute(string name) {
#else
        public ReplRoleAttribute(string name) {
#endif
            if (name.Contains(","))
                throw new ArgumentException("ReplRoleAttribute name cannot contain any commas. Apply multiple attributes if you want to support multiple roles.", "name");

            _name = name;
        }

        public string Name {
            get { return _name; }
        }
    }
}
