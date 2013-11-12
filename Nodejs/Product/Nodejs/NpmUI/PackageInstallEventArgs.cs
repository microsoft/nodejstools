using System;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    internal class PackageInstallEventArgs : EventArgs{
        public PackageInstallEventArgs(
            string name,
            string version,
            DependencyType depType){
            Name = name;
            Version = version;
            DependencyType = depType;
        }

        public string Name { get; private set; }
        public string Version { get; private set; }
        public DependencyType DependencyType { get; private set; }
    }
}