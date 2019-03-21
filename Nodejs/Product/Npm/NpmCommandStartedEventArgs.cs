using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class NpmCommandStartedEventArgs : EventArgs
    {
        public NpmCommandStartedEventArgs(string arguments)
        {
            this.Arguments = arguments;
        }

        public string Arguments { get; }

        public string CommandText
        {
            get { return string.IsNullOrEmpty(this.Arguments) ? "npm" : string.Format(CultureInfo.InvariantCulture, "npm {0}", this.Arguments); }
        }
    }
}
