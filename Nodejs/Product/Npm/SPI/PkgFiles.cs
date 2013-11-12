using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class PkgFiles : PkgStringArray, IFiles{
        public PkgFiles(JObject package) : base(package, "files"){}
    }
}