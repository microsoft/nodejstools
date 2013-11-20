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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmGetCatalogueCommand : NpmSearchCommand {

        private const string NpmCatalogueCacheGuid = "BDC4B648-84E1-4FA9-9AE8-20AF8795093F";

        private readonly bool _forceDownload;

        public NpmGetCatalogueCommand(
            string fullPathToRootPackageDirectory,
            bool forceDownload,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(
                fullPathToRootPackageDirectory,
                null,
                pathToNpm,
                useFallbackIfNpmNotFound) {
            _forceDownload = forceDownload;
        }

        public override async Task<bool> ExecuteAsync() {
            var filename = Path.Combine(
                Path.GetTempPath(),
                string.Format("npmcatalog{0}.txt", NpmCatalogueCacheGuid));
            if (!_forceDownload) {
                try {
                    if (File.Exists(filename)) {
                        using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                            using (var reader = new StreamReader(stream)) {
                                ParseResultsFromReader(reader);
                            }
                        }

                        return true;
                    }
                } catch (Exception) { }
            }

            var result = await base.ExecuteAsync();

            try {
                using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)) {
                    using (var writer = new StreamWriter(stream)) {
                        writer.Write(StandardOutput);
                    }
                }
            } catch (Exception) { }

            return result;
        }
    }
}
