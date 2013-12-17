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
using Newtonsoft.Json;

namespace Microsoft.NodejsTools.Npm {
    public class ReaderPackageJsonSource : IPackageJsonSource {
        public ReaderPackageJsonSource(TextReader reader) {
            try {
                Package = JsonConvert.DeserializeObject(reader.ReadToEnd());
            } catch (JsonReaderException jre) {
                WrapExceptionAndRethrow(jre);
            } catch (JsonSerializationException jse) {
                WrapExceptionAndRethrow(jse);
            } catch (FormatException fe) {
                WrapExceptionAndRethrow(fe);
            } catch (ArgumentException ae) {
                throw new PackageJsonException(
                    string.Format(@"Error reading package.json. The file may be parseable JSON but may contain objects with duplicate properties.

The following error occurred:

{0}", ae.Message),
                    ae);
            }
        }

        private void WrapExceptionAndRethrow(
            Exception ex) {
            throw new PackageJsonException(
                string.Format(@"Unable to read package.json. Please ensure the file is valid JSON.

Reading failed because the following error occurred:

{0}", ex.Message),
                ex);
        }

        public dynamic Package { get; private set; }
    }
}