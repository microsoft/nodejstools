using System.Dynamic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageJsonSource
    {
        dynamic Package { get; }
    }
}