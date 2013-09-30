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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Microsoft.NodejsTools {
    [Guid(NodeProjectGuid)]
    class NodejsProjectFactory : FlavoredProjectFactoryBase {
        internal const string NodeProjectGuid = "3AF33F2E-1136-4D97-BBB7-1795711AC8B8";
        private NodejsPackage _package;

        public NodejsProjectFactory(NodejsPackage package) {
            _package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown) {
            var res = new NodejsProject();
            res._package = _package;
            return res;
        }
#if FALSE
        protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled) {
            project = IntPtr.Zero;
            canceled = 0;

            // TODO: Get the list of GUIDs from the project/template
            string guidsList = "{3AF33F2E-1136-4D97-BBB7-1795711AC8B8};{349c5851-65df-11da-9384-00065b846f21};{262852c6-cd72-467d-83fe-5eeb1973a190}";

            CreateInner(
                guidsList.Split(';').Select(Guid.Parse).ToArray(),
                IntPtr.Zero,
                IntPtr.Zero,
                fileName,
                location,
                name,
                (__VSCREATEPROJFLAGS)flags,
                ref projectGuid,
                out project,
                out canceled
            );

            ErrorHandler.ThrowOnFailure(
                ((IVsAggregatableProjectCorrected)Marshal.GetObjectForIUnknown(project)).OnAggregationComplete()
            );
        }

        private void CreateInner(Guid[] guids, IntPtr outer, IntPtr pOwner, string filename, string location, string name, __VSCREATEPROJFLAGS flags, ref Guid projectGuid, out IntPtr inner, out int cancelled) {
            var sln = (IVsSolution)NodePackage.GetGlobalService(typeof(SVsSolution));

            IVsProjectFactory curFact;
            ErrorHandler.ThrowOnFailure(
                sln.GetProjectFactory(
                    0,
                    new[] { guids[0] },
                    filename,
                    out curFact
                )
            );

            if (guids.Length == 1) {
                ErrorHandler.ThrowOnFailure(
                    curFact.CreateProject(
                        filename,
                        location,
                        name,
                        (uint)flags,
                        ref projectGuid,
                        out inner,
                        out cancelled
                    )
                );

                var aggProj = new ProjectWrapper(Marshal.GetObjectForIUnknown(inner));
                inner = Marshal.GetIUnknownForObject(aggProj);

                if (pOwner != IntPtr.Zero) {
                    ErrorHandler.ThrowOnFailure(
                        ((IVsAggregatableProjectCorrected)Marshal.GetObjectForIUnknown(pOwner)).SetInnerProject(
                            inner
                        )
                    );
                }

            } else {
                var aggProjFact = curFact as IVsAggregatableProjectFactoryCorrected;
                if (aggProjFact == null) {
                    throw new InvalidOperationException();
                }

                IntPtr newInner;
                ErrorHandler.ThrowOnFailure(
                    aggProjFact.PreCreateForOuter(
                        outer,
                        out newInner
                    )
                );

                if (outer == IntPtr.Zero) {
                    outer = newInner;
                }


                if (pOwner != IntPtr.Zero) {
                    ErrorHandler.ThrowOnFailure(
                        ((IVsAggregatableProjectCorrected)Marshal.GetObjectForIUnknown(pOwner)).SetInnerProject(newInner)
                    );
                }

                var guid = typeof(IVsAggregatableProjectCorrected).GUID;
                IntPtr innerAgg;
                ErrorHandler.ThrowOnFailure(
                    Marshal.QueryInterface(
                        newInner,
                        ref guid,
                        out innerAgg
                    )
                );

                IntPtr nextInner;
                CreateInner(
                    guids.Skip(1).ToArray(),
                    outer,
                    innerAgg,
                    filename,
                    location,
                    name,
                    flags,
                    ref projectGuid,
                    out nextInner,
                    out cancelled
                );

                ErrorHandler.ThrowOnFailure(
                    ((IVsAggregatableProjectCorrected)Marshal.GetObjectForIUnknown(innerAgg)).InitializeForOuter(
                        filename,
                        location,
                        name,
                        (uint)flags,
                        ref projectGuid,
                        out inner,
                        out cancelled
                    )
                );

            }

        }
#endif
    }
}
