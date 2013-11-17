using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmExceptionEventArgs : EventArgs
    {
        public NpmExceptionEventArgs(Exception cause){
            Exception = cause;
        }

        public Exception Exception { get; private set; }
    }
}
