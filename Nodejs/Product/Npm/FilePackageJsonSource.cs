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
using System.IO;
using System.Threading;

namespace Microsoft.NodejsTools.Npm {
    public class FilePackageJsonSource : IPackageJsonSource {

        private readonly ReaderPackageJsonSource _source;

        public FilePackageJsonSource(string fullPathToFile) {
            if (File.Exists(fullPathToFile)) {
                int retryInterval = 500;
                int attempts = 5;

                // populate _source with retries for recoverable errors.
                while (--attempts >= 0) {
                    try {
                        using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(fin)) {
                            _source = new ReaderPackageJsonSource(reader);
                            break;
                        }
                    } catch (PackageJsonException pje) {
                        WrapExceptionAndRethrow(fullPathToFile, pje);
                    } catch (IOException) {
                        if (attempts <= 0) { throw; }
                    } catch (UnauthorizedAccessException) {
                        if (attempts <= 0) { throw; }
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }
        }

        private void WrapExceptionAndRethrow(
            string fullPathToFile,
            Exception ex) {
            throw new PackageJsonException(
                        string.Format(@"Error reading package.json at '{0}': {1}", fullPathToFile, ex.Message),
                        ex);
        }

        public dynamic Package { get { return null == _source ? null : _source.Package; } }
    }
}