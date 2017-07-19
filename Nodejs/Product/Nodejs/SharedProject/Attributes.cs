// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.VisualStudioTools.Project
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class SRDisplayNameAttribute : DisplayNameAttribute
    {
        private string _name;

        public SRDisplayNameAttribute(string name)
        {
            this._name = name;
        }

        public override string DisplayName => SR.GetString(this._name);
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRDescriptionAttribute : DescriptionAttribute
    {
        private bool _replaced;

        public SRDescriptionAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if (!this._replaced)
                {
                    this._replaced = true;
                    this.DescriptionValue = SR.GetString(base.Description);
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    internal sealed class SRCategoryAttribute : CategoryAttribute
    {
        public SRCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return SR.GetString(value);
        }
    }
}
