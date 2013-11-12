namespace Microsoft.NodejsTools.Npm.SPI{
    internal class Dependency : IDependency{
        private string _versionRangeUrlText;

        public Dependency(string name, string retreivalInfo){
            Name = name;
            _versionRangeUrlText = retreivalInfo;
        }

        public string Name { get; private set; }

        private bool IsVersionRange{
            get { return _versionRangeUrlText.IndexOf('/') < 0; }
        }

        public IDependencyUrl Url{
            get { return IsVersionRange ? null : new DependencyUrl(_versionRangeUrlText); }
        }

        public string VersionRangeText{
            get { return IsVersionRange ? _versionRangeUrlText : null; }
        }
    }
}