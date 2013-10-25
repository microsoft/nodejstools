using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private Bugs m_Bugs;

        public PackageJson( dynamic package )
        {
            m_Package = package;

            Keywords = new Keywords(m_Package);
            Licenses = new Licenses(m_Package);
            Files = new PkgFiles(m_Package);
            Man = new Man(m_Package);
            Dependencies = new Dependencies(m_Package, "dependencies");
            DevDependencies = new Dependencies(m_Package, "devDependencies");
            BundledDependencies = new BundledDependencies(m_Package);
            OptionalDependencies = new Dependencies(m_Package, "optionalDependencies");
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

        public IKeywords Keywords { get; private set; }

        public string Homepage
        {
            get
            {
                return null == m_Package.homepage ? null : m_Package.homepage.ToString();
            }
        }

        public IBugs Bugs
        {
            get
            {
                if (null == m_Bugs && null != m_Package.bugs)
                {
                    m_Bugs = new Bugs(m_Package);
                }
                return m_Bugs;
            }
        }

        public ILicenses Licenses { get; private set; }

        public IFiles Files { get; private set; }

        public IMan Man { get; private set; }

        public IDependencies Dependencies { get; private set; }
        public IDependencies DevDependencies { get; private set; }
        public IBundledDependencies BundledDependencies { get; private set; }
        public IDependencies OptionalDependencies { get; private set; }
    }
}
