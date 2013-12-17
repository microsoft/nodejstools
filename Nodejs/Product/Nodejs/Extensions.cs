/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudioTools;

namespace Microsoft.NodejsTools {
    public static class Extensions {
        internal static IEnumerable<IVsProject> EnumerateLoadedProjects(this IVsSolution solution, bool onlyNodeProjects = true) {
            var flags =
                onlyNodeProjects ?
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_MATCHTYPE) :
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_ALLVIRTUAL);
            var guid = new Guid(GuidList.NodejsProjectFactoryString);
            IEnumHierarchies hierarchies;
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                flags,
                ref guid,
                out hierarchies)));
            IVsHierarchy[] hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1) {
                var project = hierarchy[0] as IVsProject;
                if (project != null) {
                    yield return project;
                }
            }
        }

        internal static IEnumerable<uint> EnumerateProjectItems(this IVsProject project) {
            var enumHierarchyItemsFactory = Package.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;
            var hierarchy = (IVsHierarchy)project;
            if (enumHierarchyItemsFactory != null && project != null) {
                IEnumHierarchyItems enumHierarchyItems;
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier),
                        (uint)VSConstants.VSITEMID_ROOT,
                        out enumHierarchyItems))) {
                    if (enumHierarchyItems != null) {
                        VSITEMSELECTION[] rgelt = new VSITEMSELECTION[1];
                        uint fetched;
                        while (VSConstants.S_OK == enumHierarchyItems.Next(1, rgelt, out fetched) && fetched == 1) {
                            yield return rgelt[0].itemid;
                        }
                    }
                }
            }
        }

        internal static NodejsProjectNode GetNodeProject(this EnvDTE.Project project) {
            return project.GetCommonProject() as NodejsProjectNode;
        }

        internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy) {
            object project;

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out project
                )
            );

            return (project as EnvDTE.Project);
        }

        internal static string GetFilePath(this ITextBuffer textBuffer) {
            ITextDocument textDocument;
            if (textBuffer.Properties.TryGetProperty<ITextDocument>(typeof(ITextDocument), out textDocument)) {
                return textDocument.FilePath;
            } else {
                return null;
            }
        }

        internal static T[] Append<T>(this T[] list, T item) {
            T[] res = new T[list.Length + 1];
            list.CopyTo(res, 0);
            res[res.Length - 1] = item;
            return res;
        }

    }
}
