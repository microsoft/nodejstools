// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    internal class DeveloperActivityAttribute : RegistrationAttribute
    {
        private readonly Type _projectType;
        private readonly int _templateSet;
        private readonly string _developerActivity;

        public DeveloperActivityAttribute(string developerActivity, Type projectPackageType)
        {
            this._developerActivity = developerActivity;
            this._projectType = projectPackageType;
            this._templateSet = 1;
        }

        public DeveloperActivityAttribute(string developerActivity, Type projectPackageType, int templateSet)
        {
            this._developerActivity = developerActivity;
            this._projectType = projectPackageType;
            this._templateSet = templateSet;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context)
        {
            var key = context.CreateKey("NewProjectTemplates\\TemplateDirs\\" + this._projectType.GUID.ToString("B") + "\\/" + this._templateSet);
            key.SetValue("DeveloperActivity", this._developerActivity);
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context)
        {
        }
    }
}

