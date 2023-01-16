// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    /// Provides access to the dte.get_Properties("TextEditor", "languagename") automation 
    /// object.  This object is provided by the text editor for all languages but needs
    /// to be registered by the individual language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class ProvideTextEditorAutomationAttribute : RegistrationAttribute
    {
        internal const string TextEditorPackage = "{F5E7E720-1401-11D1-883B-0000F87579D2}";

        private readonly string _categoryName;
        private readonly short _categoryResourceId;
        private readonly short _descriptionResourceId;
        private readonly ProfileMigrationType _migrationType;

        public ProvideTextEditorAutomationAttribute(string categoryName, short categoryResourceId,
            short descriptionResourceId, ProfileMigrationType migrationType)
        {
            this._categoryName = categoryName;
            this._categoryResourceId = categoryResourceId;
            this._descriptionResourceId = descriptionResourceId;
            this._migrationType = migrationType;
        }

        public override object TypeId => this;
        public string CategoryName => this._categoryName;
        public short CategoryResourceId => this._categoryResourceId;
        public short DescriptionResourceId => this._descriptionResourceId;
        public ProfileMigrationType MigrationType => this._migrationType;
        private string AutomationTextEditorRegKey => "AutomationProperties\\TextEditor";
        private string AutomationCategoryRegKey => string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", this.AutomationTextEditorRegKey, this.CategoryName);
        public override void Register(RegistrationContext context)
        {
            using (var automationKey = context.CreateKey(this.AutomationCategoryRegKey))
            {
                automationKey.SetValue(null, "#" + this.CategoryResourceId);
                automationKey.SetValue("Description", "#" + this.DescriptionResourceId);
                automationKey.SetValue("Name", this.CategoryName);
                automationKey.SetValue("Package", TextEditorPackage);
                automationKey.SetValue("ProfileSave", 1);
                automationKey.SetValue("ResourcePackage", context.ComponentType.GUID.ToString("B"));
                automationKey.SetValue("VsSettingsMigration", (int)this.MigrationType);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
