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

namespace Microsoft.NodejsTools.Npm{
    public class FilePackageJsonSource : IPackageJsonSource{

        private readonly ReaderPackageJsonSource _source;

        public FilePackageJsonSource(string fullPathToFile){
            if (File.Exists(fullPathToFile)){
                try{
                    using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)){
                        using (var reader = new StreamReader(fin)){
                            _source = new ReaderPackageJsonSource(reader);
                        }
                    }
                } catch (PackageJsonException pje){
                    WrapExceptionAndRethrow(fullPathToFile, pje);
                }
            }
        }

        private void WrapExceptionAndRethrow(
            string fullPathToFile,
            Exception ex){
            throw new PackageJsonException(
                        string.Format(@"Error reading package.json at '{0}': {1}", fullPathToFile, ex.Message),
                        ex);
        }

        public dynamic Package { get { return null == _source ? null : _source.Package; } }
    }
}