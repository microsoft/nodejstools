/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal abstract class NpmCommand{
        private readonly string _fullPathToRootPackageDirectory;
        private string _pathToNpm;
        private bool _useFallbackIfNpmNotFound;
        private Process _process;

        protected NpmCommand(
            string fullPathToRootPackageDirectory,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true){
            _fullPathToRootPackageDirectory = fullPathToRootPackageDirectory;
            _pathToNpm = pathToNpm;
            _useFallbackIfNpmNotFound = useFallbackIfNpmNotFound;
        }

        protected string Arguments { get; set; }

        private string GetPathToNpm(){
            if (null == _pathToNpm || ! File.Exists(_pathToNpm)){
                if (_useFallbackIfNpmNotFound){
                    string match = null;
                    foreach (var potential in Environment.GetEnvironmentVariable("path").Split(Path.PathSeparator))
                    {
                        var path = Path.Combine(potential, "npm.cmd");
                        if (File.Exists(path))
                        {
                            if (null == match ||
                                path.Contains(
                                    string.Format(
                                        "{0}nodejs{1}",
                                        Path.DirectorySeparatorChar,
                                        Path.DirectorySeparatorChar)))
                            {
                                match = path;
                            }
                        }
                    }

                    if (null != match){
                        _pathToNpm = match;
                    }

                    //  That second condition deals with the situation where no match is found.
                    if (null == _pathToNpm || ! File.Exists(_pathToNpm)){
                        throw new NpmNotFoundException(
                            string.Format(
                                "Cannot find npm.cmd at '{0}' nor on your system PATH. Ensure node.js is installed.",
                                _pathToNpm));
                    }
                } else{
                    throw new NpmNotFoundException(
                        string.Format("Cannot find npm.cmd at specified path: {0}", _pathToNpm));
                }
            }
            return _pathToNpm;
        }

        private void CopyEnvironmentVariables(ProcessStartInfo target){
            foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables()){
                target.EnvironmentVariables[(string) kvp.Key] = (string) kvp.Value;
            }
        }

        private ProcessStartInfo BuildStartInfo(){
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

        public void CancelCurrentTask(){
            if (null != _process){
                try{
                    _process.Kill();
                } catch (Win32Exception){} catch (InvalidOperationException){}
            }
        }

        public virtual async Task<bool> ExecuteAsync(){
            using (_process = new Process()){
                _process.StartInfo = BuildStartInfo();

                try{
                    _process.Start();
                } catch (Win32Exception we){
                    throw new NpmExecutionException(
                        string.Format("Error executing npm - unable to start the npm process: {0}", we.Message),
                        we);
                }

                var stdout = _process.StandardOutput;
                var stderr = _process.StandardError;

                await Task.Run(() => StandardOutput = stdout.ReadToEnd());
                await Task.Run(() => StandardError = stderr.ReadToEnd());
                await Task.Run(() => _process.WaitForExit());
            }

            return true;
        }
    }
}