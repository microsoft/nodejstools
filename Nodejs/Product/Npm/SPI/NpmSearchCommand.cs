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

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NpmSearchCommand : NpmCommand{
        public NpmSearchCommand(
            string fullPathToRootPackageDirectory,
            string searchText,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true)
            : base(fullPathToRootPackageDirectory, pathToNpm, useFallbackIfNpmNotFound)
        {
            Arguments = string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(searchText.Trim())
                            ? "search"
                            : string.Format("search {0}", searchText);
        }

        public override async Task<bool> ExecuteAsync(){
            bool success = await base.ExecuteAsync();

            if (success){
                IList<IPackage> results = new List<IPackage>();

                var lexer = NpmSearchParserFactory.CreateLexer();
                var parser = NpmSearchParserFactory.CreateParser(lexer);
                parser.Package += (sender, args) => {
                    results.Add(args.Package);
                };
                using (var reader = new StringReader(StandardOutput)){
                    lexer.Lex(reader);
                }
                Results = results;
            }

            return success;
        }

        public IList<IPackage> Results { get; private set; }
    }
}