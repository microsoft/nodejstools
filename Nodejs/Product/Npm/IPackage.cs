using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackage : IRootPackage
    {
        bool IsListedInParentPackageJson { get; }
        bool IsMissing { get; }
        bool IsDevDependency { get; }
        bool IsOptionalDependency { get; }
        bool IsBundledDependency { get; }
    }
}
