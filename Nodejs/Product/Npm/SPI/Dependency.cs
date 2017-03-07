// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependency : IDependency
    {
        private string _versionRangeUrlText;

        public Dependency(string name, string retreivalInfo)
        {
            this.Name = name;
            this._versionRangeUrlText = retreivalInfo;
        }

        public string Name { get; private set; }

        private bool IsVersionRange
        {
            get { return this._versionRangeUrlText.IndexOf('/') < 0; }
        }

        public IDependencyUrl Url
        {
            get { return this.IsVersionRange ? null : new DependencyUrl(this._versionRangeUrlText); }
        }

        public string VersionRangeText
        {
            get { return this.IsVersionRange ? this._versionRangeUrlText : null; }
        }
    }
}

