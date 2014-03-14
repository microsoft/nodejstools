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
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmSearchCommand : NpmCommand {
        public NpmSearchCommand(
            string fullPathToRootPackageDirectory,
            string searchText,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm, useFallbackIfNpmNotFound) {
            Arguments = string.IsNullOrWhiteSpace(searchText) ? "search" : string.Format("search {0}", searchText);
            Results = new List<IPackage>();
        }

        private IList<IPackage> DeduplicateResults(IList<IPackage> preliminary) {
            var results = new List<IPackage>();
            var deduplicate = new HashSet<string>();
            foreach (var pkg in preliminary) {
                if (!deduplicate.Contains(pkg.Name)) {
                    results.Add(pkg);
                    deduplicate.Add(pkg.Name);
                }
            }
            return results;
        } 

        protected void ParseResultsFromReader(TextReader source) {
            var temp     = new List<IPackage>();
            var lexer = NpmSearchParserFactory.CreateLexer();
            var parser = NpmSearchParserFactory.CreateParser(lexer);
            parser.Package += (sender, args) => temp.Add(args.Package);
            lexer.Lex(source);

            //  We do the deduplication afterwards because package names
            //  can change after the packages are created where the names
            //  are split across more than one line in the npm search
            //  output.
            Results = DeduplicateResults(temp);
        }

        public override async Task<bool> ExecuteAsync() {
            bool success = await base.ExecuteAsync();

            if (success) {
                using (var reader = new StringReader(StandardOutput)) {
                    ParseResultsFromReader(reader);
                }
            }

            return success;
        }

        public IList<IPackage> Results { get; protected set; }
    }
}