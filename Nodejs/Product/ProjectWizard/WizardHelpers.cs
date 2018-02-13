// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Forms;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodejsTools.ProjectWizard
{
    internal static class WizardHelpers
    {
        public static IServiceProvider GetProvider(object automationObject)
        {
            if (automationObject is IOleServiceProvider oleProvider)
            {
                return new ServiceProvider(oleProvider);
            }
            MessageBox.Show(ProjectWizardResources.ErrorNoDte, SR.ProductName);
            return null;
        }
    }
}
