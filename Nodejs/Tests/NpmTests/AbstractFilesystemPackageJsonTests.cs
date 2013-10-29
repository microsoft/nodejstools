using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests
{
    public abstract class AbstractFilesystemPackageJsonTests : AbstractPackageJsonTests
    {

        protected TemporaryFileManager TempFileManager { get; private set; }

        [TestInitialize]
        public void Init()
        {
            TempFileManager = new TemporaryFileManager();
        }

        [TestCleanup]
        public void Cleanup()
        {
            TempFileManager.Dispose();
        }

        protected void CreatePackageJson(string filename, string json)
        {
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var writer = new StreamWriter(fout))
                {
                    writer.Write(json);
                }
            }
        }
    }
}
