using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class Licenses : ILicenses
    {

        private dynamic m_Package;

        public Licenses(dynamic package)
        {
            m_Package = package;
        }

        public int Count
        {
            get
            {
                if (m_Package.license != null)
                {
                    return 1;
                }

                var json = m_Package.licenses;
                if (null == json)
                {
                    return 0;
                }

                JArray array = json;
                return array.Count;
            }
        }

        public ILicense this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException("Cannot retrieve license for index less than 0.");
                }

                if (index == 0 && m_Package.license != null)
                {
                    return new License(m_Package.license.ToString());
                }

                var json = m_Package.licenses;
                if (null == json)
                {
                    throw new IndexOutOfRangeException("Cannot retrieve license from empty license collection.");
                }

                var lic = json[index];
                return new License(lic.type.ToString(), lic.url.ToString());
            }
        }
    }
}
