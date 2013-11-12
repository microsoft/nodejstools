using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm
{
    public class DirectoryPackageJsonSource : IPackageJsonSource
    {

        private readonly FilePackageJsonSource _source;

        public DirectoryPackageJsonSource(string fullDirectoryPath)
        {
            _source = new FilePackageJsonSource(Path.Combine(fullDirectoryPath, "package.json"));
        }

        public dynamic Package { get { return _source.Package;  } }
    }
}
