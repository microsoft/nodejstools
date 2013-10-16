using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Newtonsoft.Json;

namespace NpmTests
{
    internal class MockPackageJsonSource : IPackageJsonSource
    {

        private dynamic m_PackageJson;

        public MockPackageJsonSource( string packageJsonString )
        {
            m_PackageJson = JsonConvert.DeserializeObject( packageJsonString );
        }
    }
}
