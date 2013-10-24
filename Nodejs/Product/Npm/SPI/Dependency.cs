using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Dependency : IDependency
    {

        private string m_VersionRangeUrlText;

        public Dependency(string name, string retreivalInfo)
        {
            Name = name;
            m_VersionRangeUrlText = retreivalInfo;
        }

        public string Name { get; private set; }

        private bool IsVersionRange
        {
            get { return m_VersionRangeUrlText.IndexOf('/') < 0; }
        }

        public IDependencyUrl Url
        {
            get
            {
                return IsVersionRange ? null : new DependencyUrl(m_VersionRangeUrlText);
            }
        }

        public string VersionRangeText
        {
            get
            {
                return IsVersionRange ? m_VersionRangeUrlText : null;
            }
        }
    }
}
