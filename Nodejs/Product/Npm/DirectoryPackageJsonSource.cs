using System.IO;

namespace Microsoft.NodejsTools.Npm{
    public class DirectoryPackageJsonSource : IPackageJsonSource{
        private readonly FilePackageJsonSource _source;

        public DirectoryPackageJsonSource(string fullDirectoryPath){
            _source = new FilePackageJsonSource(Path.Combine(fullDirectoryPath, "package.json"));
        }

        public dynamic Package{
            get { return _source.Package; }
        }
    }
}