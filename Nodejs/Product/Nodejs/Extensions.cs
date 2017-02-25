//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

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
            var isPlatformAware = false;
            bool.TryParse(platAwarePropStr, out isPlatformAware);
            return isPlatformAware;
        }

        public static IEnumerable<IVsProject> EnumerateLoadedProjects(this IVsSolution solution, bool onlyNodeProjects = true)
        {
            var flags =
                onlyNodeProjects ?
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_MATCHTYPE) :
                (uint)(__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION | __VSENUMPROJFLAGS.EPF_ALLVIRTUAL);
            var guid = new Guid(Guids.NodejsBaseProjectFactoryString);
            IEnumHierarchies hierarchies;
            ErrorHandler.ThrowOnFailure((solution.GetProjectEnum(
                flags,
                ref guid,
                out hierarchies)));
            var hierarchy = new IVsHierarchy[1];
            uint fetched;
            while (ErrorHandler.Succeeded(hierarchies.Next(1, hierarchy, out fetched)) && fetched == 1)
            {
                var project = hierarchy[0] as IVsProject;
                if (project != null)
                {
                    yield return project;
                }
            }
        }

        internal static IEnumerable<uint> EnumerateProjectItems(this IVsProject project)
        {
            var enumHierarchyItemsFactory = Package.GetGlobalService(typeof(SVsEnumHierarchyItemsFactory)) as IVsEnumHierarchyItemsFactory;
            var hierarchy = (IVsHierarchy)project;
            if (enumHierarchyItemsFactory != null && project != null)
            {
                IEnumHierarchyItems enumHierarchyItems;
                if (ErrorHandler.Succeeded(
                    enumHierarchyItemsFactory.EnumHierarchyItems(
                        hierarchy,
                        (uint)(__VSEHI.VSEHI_Leaf | __VSEHI.VSEHI_Nest | __VSEHI.VSEHI_OmitHier),
                        (uint)VSConstants.VSITEMID_ROOT,
                        out enumHierarchyItems)))
                {
                    if (enumHierarchyItems != null)
                    {
                        var rgelt = new VSITEMSELECTION[1];
                        uint fetched;
                        while (VSConstants.S_OK == enumHierarchyItems.Next(1, rgelt, out fetched) && fetched == 1)
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