// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Repl
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Description("Node.js Tools - Interactive Window")]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideKeyBindingTable(ReplWindow.TypeGuid, 200)]        // Resource ID: "Interactive Console"
    [ProvideToolWindow(typeof(ReplWindow), Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.none, Window = ToolWindowGuids80.Outputwindow, MultiInstances = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.guidReplWindowPkgString)]
    internal sealed class ReplWindowPackage : Package, IVsToolWindowFactory
    {
        int IVsToolWindowFactory.CreateToolWindow(ref Guid toolWindowType, uint id)
        {
            if (toolWindowType == typeof(ReplWindow).GUID)
            {
                var model = (IComponentModel)GetService(typeof(SComponentModel));
                var replProvider = (ReplWindowProvider)model.GetService<IReplWindowProvider>();

                return replProvider.CreateFromRegistry(model, (int)id) ? VSConstants.S_OK : VSConstants.E_FAIL;
            }

            return VSConstants.E_FAIL;
        }
    }
}
