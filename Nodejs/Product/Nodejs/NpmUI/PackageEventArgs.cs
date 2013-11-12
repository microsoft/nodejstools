using System;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI{
    internal class PackageEventArgs : EventArgs{
        public PackageEventArgs(IPackage package){
            Package = package;
        }

        public IPackage Package { get; private set; }
    }
}