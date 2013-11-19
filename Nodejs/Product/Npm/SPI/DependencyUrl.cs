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

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class DependencyUrl : IDependencyUrl{
        public DependencyUrl(string address){
            Address = address;
        }

        public string Address { get; private set; }

        public DependencyUrlType Type{
            get{
                var index = Address.IndexOf("://");
                if (index < 0){
                    return DependencyUrlType.GitHub;
                } else{
                    var prefix = Address.Substring(0, index);
                    switch (prefix){
                        case "http":
                            return DependencyUrlType.Http;

                        case "git":
                            return DependencyUrlType.Git;

                        case "git+ssh":
                            return DependencyUrlType.GitSsh;

                        case "git+http":
                            return DependencyUrlType.GitHttp;

                        case "git+https":
                            return DependencyUrlType.GitHttps;

                        default:
                            return DependencyUrlType.UnsupportedProtocol;
                    }
                }
            }
        }
    }
}