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

        private readonly FilePackageJsonSource m_Source;

        public DirectoryPackageJsonSource(string fullDirectoryPath)
        {
            m_Source = new FilePackageJsonSource(Path.Combine(fullDirectoryPath, "package.json"));
        }

        public dynamic Package { get { return m_Source.Package;  } }
    }
}
