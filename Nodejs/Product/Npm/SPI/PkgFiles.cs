using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PkgFiles : PkgStringArray, IFiles
    {
        public PkgFiles(JObject package) : base(package, "files")
        {
        }
    }
}
