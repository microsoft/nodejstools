using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface INpmLogSource
    {
        event EventHandler< NpmLogEventArgs > OutputLogged;
        event EventHandler< NpmLogEventArgs > ErrorLogged;
    }
}
