// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.MockVsTests;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudioTools.VSTestHost;
using TestUtilities.SharedProject;

namespace TestUtilities.UI
{
    public static class TestExtensions
    {
        public static IVisualStudioInstance ToVs(this SolutionFile self)
        {
            if (VSTestContext.IsMock)
            {
                return self.ToMockVs();
            }
            return new VisualStudioInstance(self);
        }

        public static string[] GetDisplayTexts(this ICompletionSession completionSession)
        {
            return completionSession.CompletionSets.First().Completions.Select(x => x.DisplayText).ToArray();
        }

        public static string[] GetInsertionTexts(this ICompletionSession completionSession)
        {
            return completionSession.CompletionSets.First().Completions.Select(x => x.InsertionText).ToArray();
        }

        public static bool GetIsFolderExpanded(this EnvDTE.Project project, string folder)
        {
            return GetNodeState(project, folder, __VSHIERARCHYITEMSTATE.HIS_Expanded);
        }

        public static bool GetIsItemBolded(this EnvDTE.Project project, string item)
        {
            return GetNodeState(project, item, __VSHIERARCHYITEMSTATE.HIS_Bold);
        }

        public static bool GetNodeState(this EnvDTE.Project project, string item, __VSHIERARCHYITEMSTATE state)
        {
            IVsHierarchy hier = null;
            uint id = 0;
            ThreadHelper.Generic.Invoke((Action)(() =>
            {
                hier = ((dynamic)project).Project as IVsHierarchy;
                object projectDir;
                ErrorHandler.ThrowOnFailure(
                    hier.GetProperty(
                        (uint)VSConstants.VSITEMID.Root,
                        (int)__VSHPROPID.VSHPROPID_ProjectDir,
                        out projectDir
                    )
                );

                string itemPath = Path.Combine((string)projectDir, item);
                if (ErrorHandler.Failed(hier.ParseCanonicalName(itemPath, out id)))
                {
                    ErrorHandler.ThrowOnFailure(
                        hier.ParseCanonicalName(itemPath + "\\", out id)
                    );
                }
            }));

            // make sure we're still expanded.
            var solutionWindow = UIHierarchyUtilities.GetUIHierarchyWindow(
                VSTestContext.ServiceProvider,
                new Guid(ToolWindowGuids80.SolutionExplorer)
            );

            uint result;
            ErrorHandler.ThrowOnFailure(
                solutionWindow.GetItemState(
                    hier as IVsUIHierarchy,
                    id,
                    (uint)state,
                    out result
                )
            );
            return (result & (uint)state) != 0;
        }
    }
}

