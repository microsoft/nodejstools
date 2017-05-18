// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ProvideDebugPortSupplierAttribute : RegistrationAttribute
    {
        private readonly string _id, _name;
        private readonly Type _portSupplier, _portPicker;

        public ProvideDebugPortSupplierAttribute(string name, Type portSupplier, string id, Type portPicker = null)
        {
            this._name = name;
            this._portSupplier = portSupplier;
            this._id = id;
            this._portPicker = portPicker;
        }

        public override void Register(RegistrationContext context)
        {
            var engineKey = context.CreateKey("AD7Metrics\\PortSupplier\\" + this._id);
            engineKey.SetValue("Name", this._name);
            engineKey.SetValue("CLSID", this._portSupplier.GUID.ToString("B"));
            if (this._portPicker != null)
            {
                engineKey.SetValue("PortPickerCLSID", this._portPicker.GUID.ToString("B"));
            }

            var clsidKey = context.CreateKey("CLSID");
            var clsidGuidKey = clsidKey.CreateSubkey(this._portSupplier.GUID.ToString("B"));
            clsidGuidKey.SetValue("Assembly", this._portSupplier.Assembly.FullName);
            clsidGuidKey.SetValue("Class", this._portSupplier.FullName);
            clsidGuidKey.SetValue("InprocServer32", context.InprocServerPath);
            clsidGuidKey.SetValue("CodeBase", Path.Combine(context.ComponentPath, this._portSupplier.Module.Name));
            clsidGuidKey.SetValue("ThreadingModel", "Free");
        }

        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
