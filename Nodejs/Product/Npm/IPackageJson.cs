using System;
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
        string Homepage { get; }
        IBugs Bugs { get; }
        ILicenses Licenses { get; }
        IFiles Files { get; }
        IMan Man { get; }
        IDependencies Dependencies { get; }
    }
}
