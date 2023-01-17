// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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

        internal static IEnumerable<uint> EnumerateProjectItems(this IVsProject project, uint grfItems = (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier))
        {
            var hierarchy = (IVsHierarchy)project;
            if (Package.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) is IVsEnumHierarchyItemsFactory enumHierarchyItemsFactory && project != null)
            {
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        grfItems,
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

        //internal static IEnumerable<HierarchyItem> EnumerateHierarchyItems(this IVsSolution solution)
        //{
        //    var loadedProjects = solution.EnumerateLoadedProjects(onlyNodeProjects: false);
        //    foreach (var project in loadedProjects)
        //    {
        //        var hierarchy = (IVsHierarchy)project;
        //        foreach (var projectItem in project.EnumerateProjectItems((uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier | __VSEHI.VSEHI_Branch)))
        //        {
        //            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(projectItem, (int)__VSHPROPID.VSHPROPID_Name, out var name));
        //            ErrorHandler.ThrowOnFailure(hierarchy.GetProperty(projectItem, (int)__VSHPROPID.VSHPROPID_Parent, out var parentId));

        //            yield return new HierarchyItem()
        //            {
        //                ItemId = projectItem,
        //                Name = (string)name,
        //                ParentId = (int)parentId
        //            };
        //        }
        //    }
        //}

        //internal static NodejsProjectNode GetNodeProject(this EnvDTE.Project project)
        //{
        //    return project.GetCommonProject() as NodejsProjectNode;
        //}

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
    }
}
