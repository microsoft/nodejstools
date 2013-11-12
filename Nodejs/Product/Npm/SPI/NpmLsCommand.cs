using System;
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NpmLsCommand : NpmCommand{
        private string _listBaseDirectory;

        public NpmLsCommand(
            string fullPathToRootPackageDirectory,
            bool global,
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm){
            var buff = new StringBuilder("ls");
            if (global){
                buff.Append(" -g");
            }
            Arguments = buff.ToString();
        }

        public string ListBaseDirectory{
            get{
                if (null == _listBaseDirectory){
                    var temp = StandardOutput;
                    if (null != temp){
                        temp.Trim();
                        if (temp.Length > 0){
                            var splits = temp.Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length > 0){
                                _listBaseDirectory = splits[0].Trim();
                            }
                        }
                    }
                }
                return _listBaseDirectory;
            }
        }
    }
}