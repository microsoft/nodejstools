using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;

namespace NpmTests
{
    public abstract class AbstractPackageJsonTests
    {
        protected const string PkgEmpty = "{}";

        protected const string PkgSimple = @"{
    ""name"": ""TestPkg"",
    ""version"": ""0.1.0""
}";

        protected IPackageJson LoadFrom(string json)
        {
            return PackageJsonFactory.Create(new MockPackageJsonSource(json));
        }
    }
}
