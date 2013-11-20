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

using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Bugs : IBugs {
        private readonly dynamic _package;

        public Bugs(dynamic package) {
            _package = package;
        }


        public string Url {
            get {
                string url = null;
                var bugs = _package.bugs;
                if (null != bugs) {
                    var token = bugs as JToken;
                    if (token.Type == JTokenType.Object) {
                        var temp = bugs.url ?? bugs.web;
                        if (null != temp) {
                            url = temp.ToString();
                        }
                    } else {
                        url = token.Value<string>();
                    }
                }
                return url;
            }
        }

        public string Email {
            get {
                string email = null;
                var bugs = _package.bugs;
                if (null != bugs) {
                    var token = bugs as JToken;
                    if (token.Type == JTokenType.Object) {
                        var temp = bugs.email ?? bugs.mail;
                        if (null != temp) {
                            email = temp.ToString();
                        }
                    }
                }
                return email;
            }
        }
    }
}