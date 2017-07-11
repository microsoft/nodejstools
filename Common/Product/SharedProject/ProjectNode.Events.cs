// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudioTools.Project
{
    internal partial class ProjectNode
    {
        public event EventHandler<ProjectPropertyChangedArgs> OnProjectPropertyChanged;

        protected virtual void RaiseProjectPropertyChanged(string propertyName, string oldValue, string newValue)
        {
            var onPropChanged = OnProjectPropertyChanged;
            if (onPropChanged != null)
            {
                onPropChanged(this, new ProjectPropertyChangedArgs(propertyName, oldValue, newValue));
            }
        }
    }
}
