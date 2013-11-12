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

        private dynamic _package;
        private Scripts _scripts;
        private Bugs _bugs;

        public PackageJson( dynamic package )
        {
            _package = package;

            Keywords = new Keywords(_package);
            Licenses = new Licenses(_package);
            Files = new PkgFiles(_package);
            Man = new Man(_package);
            Dependencies = new Dependencies(_package, "dependencies");
            DevDependencies = new Dependencies(_package, "devDependencies");
            BundledDependencies = new BundledDependencies(_package);
            OptionalDependencies = new Dependencies(_package, "optionalDependencies");
            AllDependencies = new Dependencies(_package, "dependencies", "devDependencies", "optionalDependencies");
        }

        public string Name
        {
            get { return null == _package.name ? null : _package.name.ToString(); }
        }

        public SemverVersion Version
        {
            get { return null == _package.version ? new SemverVersion() : SemverVersion.Parse( _package.version.ToString() ); }
        }

        public IScripts Scripts
        {
            get
            {
                if (null == _scripts)
                {
                    dynamic scriptsJson = _package.scripts;
                    if (null == scriptsJson)
                    {
                        scriptsJson = new JObject();
                        _package.scripts = scriptsJson;
                    }
                    _scripts   = new Scripts( scriptsJson );
                }

                return _scripts;
            }
        }

        public IPerson Author
        {
            get
            {
                var author = _package.author;
                return null == author ? null : new Person(author);
            }
        }

        public string Description
        {
            get
            {
                return null == _package.description ? null : _package.description.ToString();
            }
        }

        public IKeywords Keywords { get; private set; }

        public string Homepage
        {
            get
            {
                return null == _package.homepage ? null : _package.homepage.ToString();
            }
        }

        public IBugs Bugs
        {
            get
            {
                if (null == _bugs && null != _package.bugs)
                {
                    _bugs = new Bugs(_package);
                }
                return _bugs;
            }
        }

        public ILicenses Licenses { get; private set; }

        public IFiles Files { get; private set; }

        public IMan Man { get; private set; }

        public IDependencies Dependencies { get; private set; }
        public IDependencies DevDependencies { get; private set; }
        public IBundledDependencies BundledDependencies { get; private set; }
        public IDependencies OptionalDependencies { get; private set; }
        public IDependencies AllDependencies { get; private set; }
    }
}
