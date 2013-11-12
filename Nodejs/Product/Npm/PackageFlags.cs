using System;

namespace Microsoft.NodejsTools.Npm{
    [Flags]
    public enum PackageFlags{
        NotListedAsDependency = 0x0001,
        Missing = 0x0002,
        Dev = 0x0004,
        Optional = 0x0008,
        Bundled = 0x0010,
        VersionMismatch = 0x0100,
        Installed = 0x1000,
    }
}