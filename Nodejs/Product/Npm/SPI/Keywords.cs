using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Keywords : PkgStringArray, IKeywords
    {
        public Keywords(JObject package) : base(package, "keywords")
        {
        }
    }
}