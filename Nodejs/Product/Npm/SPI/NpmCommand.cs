using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal abstract class NpmCommand
    {

        private readonly string _fullPathToRootPackageDirectory;
        private string _pathToNpm;

        protected NpmCommand(
            string fullPathToRootPackageDirectory,
            string pathToNpm = null)
        {
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _pathToNpm = pathToNpm;
        }

        protected string Arguments { get; set; }

        private string GetPathToNpm()
        {
            if (null == _pathToNpm)
            {
                string match = null;
                foreach (var potential in Environment.GetEnvironmentVariable("path").Split(Path.PathSeparator))
                {
                    var path = Path.Combine(potential, "npm.cmd");
                    if (File.Exists(path))
                    {
                        if (null == match ||
                            path.Contains(string.Format("{0}nodejs{1}", Path.DirectorySeparatorChar,
                                Path.DirectorySeparatorChar)))
                        {
                            match = path;
                        }
                    }
                    _pathToNpm = match;
                }
            }
            return _pathToNpm;
        }

        private void CopyEnvironmentVariables(ProcessStartInfo target)
        {
            foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables())
            {
                target.EnvironmentVariables[(string) kvp.Key] = (string) kvp.Value;
            }
        }

        private ProcessStartInfo BuildStartInfo()
        {
            var info = new ProcessStartInfo(GetPathToNpm(), Arguments);
            info.WorkingDirectory = _fullPathToRootPackageDirectory;
            //info.UseShellExecute = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.CreateNoWindow = true;

            CopyEnvironmentVariables(info);

            return info;
        }

        public string StandardOutput { get; private set; }
        public string StandardError { get; private set; }

        public virtual async Task<bool> ExecuteAsync()
        {
            
            using (var proc = new Process())
            {
                proc.StartInfo = BuildStartInfo();
                proc.Start();

                var stdout = proc.StandardOutput;
                var stderr = proc.StandardError;

                await Task.Run(() => StandardOutput = stdout.ReadToEnd());
                await Task.Run(() => StandardError = stderr.ReadToEnd());
                await Task.Run(() => proc.WaitForExit());
            }

            return true;
        }
    }
}
