// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm.SPI
{
    internal class DependencyUrl : IDependencyUrl
    {
        public DependencyUrl(string address)
        {
            Address = address;
        }

        public string Address { get; private set; }

        public DependencyUrlType Type
        {
            get
            {
                var index = Address.IndexOf("://", StringComparison.Ordinal);
                if (index < 0)
                {
                    return DependencyUrlType.GitHub;
                }
                else
                {
                    var prefix = Address.Substring(0, index);
                    switch (prefix)
                    {
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

