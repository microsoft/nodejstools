using Microsoft.NodejsTools.Npm;
using Newtonsoft.Json;

namespace NpmTests{
    internal class MockPackageJsonSource : IPackageJsonSource{
        public MockPackageJsonSource(string packageJsonString){
            Package = JsonConvert.DeserializeObject(packageJsonString);
        }

        public dynamic Package { get; private set; }
    }
}