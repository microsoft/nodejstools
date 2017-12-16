// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm.SPI
{
    public sealed class PackageJsonScript : IPackageJsonScript
    {
        public PackageJsonScript(string name, string cmdLine)
        {
            this.CommandName = name;
            this.CommandLine = cmdLine;
        }

        public string CommandName { get; }
        public string CommandLine { get; }
    }
}
