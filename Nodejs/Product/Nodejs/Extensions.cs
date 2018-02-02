// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.NodejsTools.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools
{
    public static class Extensions
    {
        internal static bool IsPlatformAware(this ProjectNode projectNode)
        {
            var platAwarePropStr = projectNode.BuildProject.GetPropertyValue(ProjectFileConstants.PlatformAware);
            bool.TryParse(platAwarePropStr, out var isPlatformAware);
            return isPlatformAware;
        }

        public static IEnumerable<IVsProject> EnumerateLoadedProjects(this IVsSolution solution, bool onlyNodeProjects = true)
        {
            var flags =
                onlyNodeProjects ?
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_MATCHTYPE) :
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_ALLVIRTUAL);
            var guid = new Guid(Guids.NodejsBaseProjectFactoryString);
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                flags,
                ref guid,
                out var hierarchies)));
            var hierarchy = new IVsHierarchy[1];
            while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out var fetched)) && fetched == 1)
            {
                if (hierarchy[0] is IVsProject project)
                {
                    yield return project;
                }
            }
        }

        internal static IEnumerable<uint> EnumerateProjectItems(this IVsProject project)
        {
            var hierarchy = (IVsHierarchy)project;
            if (Package.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) is IVsEnumHierarchyItemsFactory enumHierarchyItemsFactory && project != null)
            {
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier),
                        (uint)VSConstants.VSITEMID_ROOT,
                        out var enumHierarchyItems)))
                {
                    if (enumHierarchyItems != null)
                    {
                        var rgelt = new VSITEMSELECTION[1];
                        while (VSConstants.S_OK == enumHierarchyItems.Next(1, rgelt, out var fetched) && fetched == 1)
                        {
                            yield return rgelt[0].itemid;
                        }
                    }
                }
            }
        }

        internal static NodejsProjectNode GetNodeProject(this EnvDTE.Project project)
        {
            return project.GetCommonProject() as NodejsProjectNode;
        }

        internal static EnvDTE.Project GetProject(this IVsHierarchy hierarchy)
        {

            ErrorHandler.ThrowOnFailure(
                hierarchy.GetProperty(
                    VSConstants.VSITEMID_ROOT,
                    (int)__VSHPROPID.VSHPROPID_ExtObject,
                    out var project
                )
            );

            return (project as EnvDTE.Project);
        }

        internal static IComponentModel GetComponentModel(this IServiceProvider serviceProvider)
        {
            return (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
        }

        internal static T[] Append<T>(this T[] list, T item)
        {
            var res = new T[list.Length + 1];
            list.CopyTo(res, 0);
            res[res.Length - 1] = item;
            return res;
        }

        internal static NodejsProjectNode GetNodejsProject(this EnvDTE.Project project)
        {
            return project.GetCommonProject() as NodejsProjectNode;
        }
    }
}
