using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class TokenEventArgs : EventArgs
    {

        public TokenEventArgs(
            string value,
            TokenFlags flags,
            int leadingEqualsCount){
            Value = value;
            Flags = flags;
            LeadingEqualsCount = leadingEqualsCount;
        }

        public string Value { get; private set; }
        public TokenFlags Flags { get; private set; }
        public int LeadingEqualsCount { get; private set; }
    }
}
