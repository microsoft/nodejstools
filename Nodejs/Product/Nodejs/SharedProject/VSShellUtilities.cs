// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    ///This class provides some useful static shell based methods. 
    /// </summary>

    internal static class UIHierarchyUtilities
    {
        /// <summary>
        /// Get reference to IVsUIHierarchyWindow interface from guid persistence slot.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="persistenceSlot">Unique identifier for a tool window created using IVsUIShell::CreateToolWindow. 
        /// The caller of this method can use predefined identifiers that map to tool windows if those tool windows 
        /// are known to the caller. </param>
        /// <returns>A reference to an IVsUIHierarchyWindow interface.</returns>
        public static IVsUIHierarchyWindow GetUIHierarchyWindow(IServiceProvider serviceProvider, Guid persistenceSlot)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }

            var shell = serviceProvider.GetService(typeof(SVsUIShell)) as IVsUIShell;
            if (shell == null)
            {
                throw new InvalidOperationException("Could not get the UI shell from the project");
            }

            if (ErrorHandler.Succeeded(shell.FindToolWindow(0, ref persistenceSlot, out var frame)) &&
                ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var pvar)))
            {
                return pvar as IVsUIHierarchyWindow;
            }

            return null;
        }
    }

    internal static class VsUtilities
    {
        internal static void NavigateTo(IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int line, int col)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            if (docViewGuidType != Guid.Empty)
            {
                OpenDocument(serviceProvider, filename, docViewGuidType, out viewAdapter, out pWindowFrame);
            }
            else
            {
                OpenDocument(serviceProvider, filename, out viewAdapter, out pWindowFrame);
            }

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());

            // Set the cursor at the beginning of the declaration.
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        internal static void NavigateTo(IServiceProvider serviceProvider, string filename, Guid docViewGuidType, int pos)
        {
            IVsTextView viewAdapter;
            IVsWindowFrame pWindowFrame;
            if (docViewGuidType != Guid.Empty)
            {
                OpenDocument(serviceProvider, filename, docViewGuidType, out viewAdapter, out pWindowFrame);
            }
            else
            {
                OpenDocument(serviceProvider, filename, out viewAdapter, out pWindowFrame);
            }

            ErrorHandler.ThrowOnFailure(pWindowFrame.Show());
            ErrorHandler.ThrowOnFailure(viewAdapter.GetLineAndColumn(pos, out var line, out var col));
            ErrorHandler.ThrowOnFailure(viewAdapter.SetCaretPos(line, col));
            // Make sure that the text is visible.
            viewAdapter.CenterLines(line, 1);
        }

        internal static void OpenDocument(IServiceProvider serviceProvider, string filename, out IVsTextView viewAdapter, out IVsWindowFrame pWindowFrame)
        {
            var textMgr = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
            VsShellUtilities.OpenDocument(
                serviceProvider,
                filename,
                Guid.Empty,
                out var hierarchy,
                out var itemid,
                out pWindowFrame,
                out viewAdapter);
        }

        internal static void OpenDocument(IServiceProvider serviceProvider, string filename, Guid docViewGuid, out IVsTextView viewAdapter, out IVsWindowFrame pWindowFrame)
        {
            VsShellUtilities.OpenDocumentWithSpecificEditor(
                serviceProvider,
                filename,
                docViewGuid,
                Guid.Empty,
                out var hierarchy,
                out var itemid,
                out pWindowFrame
            );
            viewAdapter = VsShellUtilities.GetTextView(pWindowFrame);
        }
    }
}
