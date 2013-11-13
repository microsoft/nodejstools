using System;

namespace Microsoft.NodejsTools.Npm{
    public interface INpmSearchParser{
        event EventHandler<PackageEventArgs> PackageParsed;
    }
}