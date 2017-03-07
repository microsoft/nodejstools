// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Microsoft.NodejsTools.Npm
{
    public class ReaderPackageJsonSource : IPackageJsonSource
    {
        public ReaderPackageJsonSource(TextReader reader)
        {
            try
            {
                var text = reader.ReadToEnd();
                try
                {
                    // JsonConvert and JObject.Parse exhibit slightly different behavior,
                    // so fall back to JObject.Parse if JsonConvert does not properly deserialize
                    // the object.
                    this.Package = JsonConvert.DeserializeObject(text);
                }
                catch (ArgumentException)
                {
                    this.Package = JObject.Parse(text);
                }
            }
            catch (JsonReaderException jre)
            {
                WrapExceptionAndRethrow(jre);
            }
            catch (JsonSerializationException jse)
            {
                WrapExceptionAndRethrow(jse);
            }
            catch (FormatException fe)
            {
                WrapExceptionAndRethrow(fe);
            }
            catch (ArgumentException ae)
            {
                throw new PackageJsonException(
                    string.Format(CultureInfo.CurrentCulture, @"Error reading package.json. The file may be parseable JSON but may contain objects with duplicate properties.

The following error occurred:

{0}", ae.Message),
                    ae);
            }
        }

        private void WrapExceptionAndRethrow(
            Exception ex)
        {
            throw new PackageJsonException(
                string.Format(CultureInfo.CurrentCulture, @"Unable to read package.json. Please ensure the file is valid JSON.

Reading failed because the following error occurred:

{0}", ex.Message),
                ex);
        }

        public dynamic Package { get; private set; }
    }
}

