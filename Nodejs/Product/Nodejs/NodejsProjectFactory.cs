// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace Microsoft.NodejsTools
{
    [Guid(Guids.NodejsProjectFactoryString)]
    internal class NodejsProjectFactory : FlavoredProjectFactoryBase
    {
        private NodejsPackage _package;

        public NodejsProjectFactory(NodejsPackage package)
        {
            this._package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            var res = new NodejsProject();
            res._package = this._package;
            return res;
        }
    }
}
