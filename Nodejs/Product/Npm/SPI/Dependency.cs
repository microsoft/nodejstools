using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependency : IDependency
    {
        public Dependency(string name, string retreivalInfo)
        {
            Name = name;
            VersionRangeText = retreivalInfo;
        }

        public string Name { get; private set; }

        public IDependencyUrl Url
        {
            get
            {
                return null;
            }
        }

        public string VersionRangeText { get; private set; }
    }
}
