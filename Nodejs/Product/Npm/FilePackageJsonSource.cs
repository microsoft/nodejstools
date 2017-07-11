// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.NodejsTools.Npm
{
    public class FilePackageJsonSource : IPackageJsonSource
    {
        private readonly ReaderPackageJsonSource source;

        public FilePackageJsonSource(string fullPathToFile)
        {
            if (File.Exists(fullPathToFile))
            {
                int retryInterval = 500;
                int attempts = 5;

                // populate _source with retries for recoverable errors.
                while (--attempts >= 0)
                {
                    try
                    {
                        using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(fin))
                        {
                            this.source = new ReaderPackageJsonSource(reader);
                            break;
                        }
                    }
                    catch (PackageJsonException pje)
                    {
                        WrapExceptionAndRethrow(fullPathToFile, pje);
                    }
                    catch (IOException)
                    {
                        if (attempts <= 0) { throw; }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (attempts <= 0) { throw; }
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }
        }

        private void WrapExceptionAndRethrow(
            string fullPathToFile,
            Exception ex)
        {
            throw new PackageJsonException(
                string.Format(CultureInfo.CurrentCulture, @"Error reading package.json at '{0}': {1}", fullPathToFile, ex.Message),
                ex);
        }

        public dynamic Package => this.source?.Package;
    }
}
