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
using System.Text;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmLsCommand : NpmCommand {
        private string _listBaseDirectory;

        public NpmLsCommand(
            string fullPathToRootPackageDirectory,
            bool global,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm, useFallbackIfNpmNotFound) {
            var buff = new StringBuilder("ls");
            if (global) {
                buff.Append(" -g");
            }
            Arguments = buff.ToString();
        }

        public string ListBaseDirectory {
            get {
                if (null == _listBaseDirectory) {
                    var temp = StandardOutput;
                    if (null != temp) {
                        temp.Trim();
                        if (temp.Length > 0) {
                            // The standard output contains an informational 
                            // message added by the base command class through 
                            // the redirector.  We must trim it to get the output 
                            // of the ls command.
                            if (temp.StartsWith("====")) {
                                int index = temp.IndexOf("\n");
                                if (index >= 0) {
                                    temp = temp.Substring(index).Trim();
                                }
                            }

                            var splits = temp.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length > 0) {
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