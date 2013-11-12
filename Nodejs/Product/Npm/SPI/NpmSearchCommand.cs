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
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.NodejsTools.Npm.SPI{
    internal class NpmSearchCommand : NpmCommand{
        public NpmSearchCommand(
            string fullPathToRootPackageDirectory,
            string searchText,
            string pathToNpm = null) : base(fullPathToRootPackageDirectory, pathToNpm){
            Arguments = string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(searchText.Trim())
                            ? "search"
                            : string.Format("search {0}", searchText);
        }

        public override async Task<bool> ExecuteAsync(){
            bool success = await base.ExecuteAsync();

            if (success){
                IList<IPackage> results = new List<IPackage>();
                bool firstLine = true;
                foreach (
                    var line in
                        StandardOutput.Split(
                            new string[]{Environment.NewLine, "\n", "\r\n"},
                            StringSplitOptions.RemoveEmptyEntries)){
                    if (firstLine){
                        firstLine = false;
                        continue;
                    }

                    var moduleBuilder = new NodeModuleBuilder();

                    var split = line.Split(new[]{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                    moduleBuilder.Name = split[0];

                    split = split[1].Split(new[]{'='}, 2, StringSplitOptions.RemoveEmptyEntries);
                    string rest = "=" + split[0];

                    if (split.Length >= 2){
                        moduleBuilder.Description = split[0].TrimEnd(' ');

                        rest = "=" + split[1];
                    }

                    split = rest.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

                    var authors = split.Where(s => s.StartsWith("=")).Select(s => s.TrimStart('='));

                    moduleBuilder.Author = new Person(String.Join(", ", authors));

                    var remainder = split.Where(s => !s.StartsWith("=")).ToArray();

                    if (remainder.Length >= 3){
                        try{
                            moduleBuilder.Version = SemverVersion.Parse(remainder[2]);
                        } catch (SemverVersionFormatException){
                            moduleBuilder.Version = SemverVersion.UnknownVersion;
                        }
                    }

                    results.Add(moduleBuilder.Build());
                }

                Results = results;
            }

            return success;
        }

        public IList<IPackage> Results { get; private set; }
    }
}