using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NpmTests{
    public abstract class AbstractFilesystemPackageJsonTests : AbstractPackageJsonTests{
        protected TemporaryFileManager TempFileManager { get; private set; }

        [TestInitialize]
        public void Init(){
            TempFileManager = new TemporaryFileManager();
        }

        [TestCleanup]
        public void Cleanup(){
            TempFileManager.Dispose();
        }

        protected void CreatePackageJson(string filename, string json){
            using (var fout = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)){
                using (var writer = new StreamWriter(fout)){
                    writer.Write(json);
                }
            }
        }

        protected string CreateRootPackage(string json){
            var dir = TempFileManager.GetNewTempDirectory();
            var path = Path.Combine(dir.FullName, "package.json");
            CreatePackageJson(path, json);
            return dir.FullName;
        }
    }
}