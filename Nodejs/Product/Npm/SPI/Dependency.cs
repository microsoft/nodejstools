// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.NodejsTools.Npm.SPI
{
    [DebuggerDisplay("{Name}")]
    internal class Dependency : IDependency
    {
        private readonly string versionRangeUrlText;

        public Dependency(string name, string retrievalInfo)
        {
            this.Name = name;
            this.versionRangeUrlText = retrievalInfo;
        }

        public string Name { get; }

        private bool IsVersionRange => this.versionRangeUrlText.IndexOf('/') < 0;

        public IDependencyUrl Url => this.IsVersionRange ? null : new DependencyUrl(this.versionRangeUrlText);

        public string VersionRangeText => this.IsVersionRange ? this.versionRangeUrlText : null;
    }
}
