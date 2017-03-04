// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependency : IDependency
    {
        private string _versionRangeUrlText;

        public Dependency(string name, string retreivalInfo)
        {
            Name = name;
            _versionRangeUrlText = retreivalInfo;
        }

        public string Name { get; private set; }

        private bool IsVersionRange
        {
            get { return _versionRangeUrlText.IndexOf('/') < 0; }
        }

        public IDependencyUrl Url
        {
            get { return IsVersionRange ? null : new DependencyUrl(_versionRangeUrlText); }
        }

        public string VersionRangeText
        {
            get { return IsVersionRange ? _versionRangeUrlText : null; }
        }
    }
}

