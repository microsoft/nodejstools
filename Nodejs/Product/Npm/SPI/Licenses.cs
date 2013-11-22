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
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class Licenses : ILicenses {
        private dynamic _package;

        public Licenses(dynamic package) {
            _package = package;
        }

        public int Count {
            get {
                if (_package.license != null) {
                    return 1;
                }

                var json = _package.licenses;
                if (null == json) {
                    return 0;
                }

                JArray array = json;
                return array.Count;
            }
        }

        public ILicense this[int index] {
            get {
                if (index < 0) {
                    throw new IndexOutOfRangeException("Cannot retrieve license for index less than 0.");
                }

                if (index == 0 && _package.license != null) {
                    return new License(_package.license.ToString());
                }

                var json = _package.licenses;
                if (null == json) {
                    throw new IndexOutOfRangeException("Cannot retrieve license from empty license collection.");
                }

                var lic = json[index];
                return new License(lic.type.ToString(), lic.url.ToString());
            }
        }
    }
}