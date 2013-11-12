using System.IO;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm{
    public class FilePackageJsonSource : IPackageJsonSource{
        public FilePackageJsonSource(string fullPathToFile){
            if (File.Exists(fullPathToFile)){
                using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read)){
                    using (var reader = new StreamReader(fin)){
                        Package = JsonConvert.DeserializeObject(reader.ReadToEnd());
                    }
                }
            }
        }

        public dynamic Package { get; private set; }
    }
}