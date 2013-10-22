using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class PackageJson : IPackageJson
    {

        private dynamic m_Package;
        private Scripts m_Scripts;
        private Keywords m_Keywords;

        public PackageJson( dynamic package )
        {
            m_Package = package;
        }

        public string Name
        {
            get { return null == m_Package.name ? null : m_Package.name.ToString(); }
        }

        public SemverVersion Version
        {
            get { return null == m_Package.version ? new SemverVersion() : SemverVersion.Parse( m_Package.version.ToString() ); }
        }

        public IScripts Scripts
        {
            get
            {
                if (null == m_Scripts)
                {
                    dynamic scriptsJson = m_Package.scripts;
                    if (null == scriptsJson)
                    {
                        scriptsJson = new JObject();
                        m_Package.scripts = scriptsJson;
                    }
                    m_Scripts   = new Scripts( scriptsJson );
                }

                return m_Scripts;
            }
        }

        public string Description
        {
            get
            {
                return null == m_Package.description ? null : m_Package.description.ToString();
            }
        }

        public IKeywords Keywords
        {
            get
            {
                if (null == m_Keywords)
                {
                    dynamic json = m_Package.keywords;
                    if (null == json)
                    {
                        json = new JArray();
                        m_Package.keywords = json;
                    }
                    m_Keywords = new Keywords(json);
                }
                return m_Keywords;
            }
        }

        public string Homepage
        {
            get
            {
                return null == m_Package.homepage ? null : m_Package.homepage.ToString();
            }
        }
    }
}
