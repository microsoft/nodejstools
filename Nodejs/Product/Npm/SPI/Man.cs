using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Man : PkgStringArray, IMan{
        public Man(JObject package) : base(package, "man"){}
    }
}