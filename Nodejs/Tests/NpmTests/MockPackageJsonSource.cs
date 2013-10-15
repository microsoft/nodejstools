using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;

namespace NpmTests
{
    internal class MockPackageJsonSource : IPackageJsonSource
    {
        public MockPackageJsonSource( string packageJsonString )
        {
            throw new NotImplementedException();
        }
    }
}
