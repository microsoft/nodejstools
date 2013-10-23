using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPkgStringArray : IEnumerable<string>
    {
        int Count { get; }
        string this[int index] { get; }
    }
}
