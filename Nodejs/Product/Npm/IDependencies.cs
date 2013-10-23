using System.Collections.Generic;

namespace Microsoft.NodejsTools.Npm
{
    public interface IDependencies : IEnumerable<IDependency>
    {
        int Count { get; }
        IDependency this[string name] { get; }
    }
}