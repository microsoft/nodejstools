using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageJson
    {
        string Name { get; }
        SemverVersion Version { get; }
        IScripts Scripts { get; }
        string Description { get; }
        IKeywords Keywords { get; }
    }
}
