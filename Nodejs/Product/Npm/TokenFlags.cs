using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    [Flags]
    public enum TokenFlags{
        None            = 0x0000,
        Digits          = 0x0001,
        Letters         = 0x0002,
        Dots            = 0x0004,
        Colons          = 0x0008,
        Dashes          = 0x0010,
        Whitespace      = 0x0020,
        Newline         = 0x0040,
        LeadingEquals   = 0x0080,
        Other           = 0x0100,
    }
}
