// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class NpmUpdateCommand : NpmCommand
    {
        public NpmUpdateCommand(
            string fullPathToRootPackageDirectory,
            IEnumerable<IPackage> packages,
            bool global,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm)
        {
            var buff = new StringBuilder("update");
            if (global)
            {
                buff.Append(" -g");
            }

            foreach (var package in packages)
            {
                buff.Append(' ');
                buff.Append(package.Name);
            }

            if (!global)
            {
                buff.Append(" --save");
            }
            this.Arguments = buff.ToString();
        }
    }
}

