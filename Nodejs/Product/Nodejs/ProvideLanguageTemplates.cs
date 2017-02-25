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

namespace Microsoft.NodejsTools
{
    /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute"]' />
    /// <devdoc>
    ///     This attribute associates a file extension to a given editor factory.  
    ///     The editor factory may be specified as either a GUID or a type and 
    ///     is placed on a package.
    ///     
    /// This differs from the normal one in that more than one extension can be supplied and
    /// a linked editor GUID can be supplied.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class ProvideLanguageTemplatesAttribute : RegistrationAttribute
    {
        private readonly string _projectFactory, _languageName, _package, _languageGuid, _description,
            _codeFileExtension, _templateFolder, _webProjectGuid;

        public ProvideLanguageTemplatesAttribute(string projectFactory, string languageName, string package,
            string templateGroup, string description, string languageProjectGuid, string codeFileExtension, string templateFolder, string webProjectGuid)
        {
            this._projectFactory = projectFactory;
            this._languageName = languageName;
            this._package = package;
            this._description = description;
            this._languageGuid = languageProjectGuid;
            this._codeFileExtension = codeFileExtension;
            this._templateFolder = templateGroup;
            this._webProjectGuid = webProjectGuid;
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="Register"]' />
        /// <devdoc>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration inforomation should be placed.
        ///     it also contains such as the type being registered, and path information.
        ///
        ///     This method is called both for registration and unregistration.  The difference is
        ///     that unregistering just uses a hive that reverses the changes applied to it.
        /// </devdoc>
        public override void Register(RegistrationContext context)
        {
            var langTemplates = string.Format(CultureInfo.InvariantCulture, "Projects\\{0}\\LanguageTemplates", this._projectFactory);

            using (var projectKey = context.CreateKey(langTemplates))
            {
                projectKey.SetValue(this._languageGuid, this._webProjectGuid);
            }

            var newProject = string.Format(CultureInfo.InvariantCulture, "Projects\\{0}", this._webProjectGuid);
            using (var projectKey = context.CreateKey(newProject))
            {
                projectKey.SetValue(null, this._description);
                projectKey.SetValue("Language(VsTemplate)", this._languageName);
                projectKey.SetValue("Package", this._package);
                projectKey.SetValue("ShowOnlySpecifiedTemplates(VsTemplate)", 0);

                using (var propKey = projectKey.CreateSubkey("WebApplicationProperties"))
                {
                    propKey.SetValue("CodeFileExtension", this._codeFileExtension);
                    propKey.SetValue("TemplateFolder", this._templateFolder);
                }
            }
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}