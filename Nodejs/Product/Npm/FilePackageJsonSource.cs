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

using System.IO;
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm{
    public class FilePackageJsonSource : IPackageJsonSource{
        public FilePackageJsonSource(string fullPathToFile){
            if (File.Exists(fullPathToFile)){
                try{
                    using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read)){
                        using (var reader = new StreamReader(fin)){
                            Package = JsonConvert.DeserializeObject(reader.ReadToEnd());
                        }
                    }
                } catch (JsonReaderException jre){
                    throw new PackageJsonException(
                        string.Format(@"Unable to read package.json file at '{0}'. Please ensure the file is valid JSON.
Reading failed because the following error occurred:

{1}", fullPathToFile, jre.Message),
                        jre);
                }
            }
        }

        public dynamic Package { get; private set; }
    }
}