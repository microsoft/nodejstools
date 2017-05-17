// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ProvideDebugPortPickerAttribute : RegistrationAttribute
    {
        private readonly Type _portPicker;

        public ProvideDebugPortPickerAttribute(Type portPicker)
        {
            this._portPicker = portPicker;
        }

        public override void Register(RegistrationContext context)
        {
            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(this._portPicker.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", this._portPicker.Assembly.FullName);
            clsidGuidKey.SetValue("Class", this._portPicker.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, this._portPicker.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}

