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
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools {
    /// <summary>
    /// Provides access to the dte.get_Properties("TextEditor", "languagename") automation 
    /// object.  This object is provided by the text editor for all languages but needs
    /// to be registered by the individual language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class ProvideTextEditorAutomationAttribute : RegistrationAttribute {
        private readonly string _categoryName;
        private readonly short _categoryResourceId;
        private readonly short _descriptionResourceId;
        private readonly ProfileMigrationType _migrationType;

        public ProvideTextEditorAutomationAttribute(string categoryName, short categoryResourceId,
            short descriptionResourceId, ProfileMigrationType migrationType) {
            Utilities.ArgumentNotNull(categoryName, "categoryName");

            _categoryName = categoryName;
            _categoryResourceId = categoryResourceId;
            _descriptionResourceId = descriptionResourceId;
            _migrationType = migrationType;
        }

        public override object TypeId {
            get { return this; }
        }

        public string CategoryName {
            get { return this._categoryName; }
        }

        public short CategoryResourceId {
            get { return this._categoryResourceId; }
        }

        public short DescriptionResourceId {
            get { return this._descriptionResourceId; }
        }

        public ProfileMigrationType MigrationType {
            get { return this._migrationType; }
        }

        private string AutomationTextEditorRegKey {
            get { return "AutomationProperties\\TextEditor"; }
        }

        private string AutomationCategoryRegKey {
            get { return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", AutomationTextEditorRegKey, CategoryName); }
        }

        public override void Register(RegistrationContext context) {
            using (Key automationKey = context.CreateKey(AutomationCategoryRegKey)) {
                automationKey.SetValue(null, "#" + CategoryResourceId);
                automationKey.SetValue("Description", "#" + DescriptionResourceId);
                automationKey.SetValue("Name", CategoryName);
                automationKey.SetValue("Package", CommonConstants.TextEditorPackage);
                automationKey.SetValue("ProfileSave", 1);
                automationKey.SetValue("ResourcePackage", context.ComponentType.GUID.ToString("B"));
                automationKey.SetValue("VsSettingsMigration", (int)MigrationType);
            }
        }

        public override void Unregister(RegistrationContext context) {
        }
    }
}
