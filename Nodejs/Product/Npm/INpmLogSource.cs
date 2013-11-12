using System;

namespace Microsoft.NodejsTools.Npm{
    public interface INpmLogSource{
        event EventHandler<NpmLogEventArgs> OutputLogged;
        event EventHandler<NpmLogEventArgs> ErrorLogged;
    }
}