// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NodeModulesProxy : AbstractNodeModules
    {
        public new void AddModule(IPackage package)
        {
            base.AddModule(package);
        }

        public override int GetDepth(string filepath) => throw new NotImplementedException();
    }
}

