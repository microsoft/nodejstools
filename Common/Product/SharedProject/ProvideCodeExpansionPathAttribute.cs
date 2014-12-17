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
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools {

    /// <summary>
    /// This attribute registers an additional path for code snippets to live
    /// in for a particular language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711")]
    sealed class ProvideCodeExpansionPathAttribute : RegistrationAttribute {
        private readonly string _languageStringId;
        private readonly string _description;
        private readonly string _paths;

        /// <summary>
        /// Creates a new RegisterSnippetsAttribute.
        /// </summary>
        public ProvideCodeExpansionPathAttribute(string languageStringId, string description,
                                          string paths) {
            _languageStringId = languageStringId;
            _description = description;
            _paths = paths;
        }

        /// <summary>
        /// Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId {
            get { return _languageStringId; }
        }

        /// <summary>
        /// Returns the paths to look for snippets.
        /// </summary>
        public string Paths {
            get { return _paths; }
        }

        /// <summary>
        /// The reg key name of the project.
        /// </summary>
        private string LanguageName() {
            return string.Format(CultureInfo.InvariantCulture, "Languages\\CodeExpansions\\{0}", LanguageStringId);
        }

        /// <summary>
        /// Called to register this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Register(RegistrationContext context) {
            using (Key childKey = context.CreateKey(LanguageName())) {
                string snippetPaths = context.ComponentPath;
                snippetPaths = System.IO.Path.Combine(snippetPaths, Paths);
                snippetPaths = context.EscapePath(System.IO.Path.GetFullPath(snippetPaths));

                using (Key pathsSubKey = childKey.CreateSubkey("Paths")) {
                    pathsSubKey.SetValue(_description, snippetPaths);
                }
            }
        }

        /// <summary>
        /// Called to unregister this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Unregister(RegistrationContext context) {
        }
    }
}

