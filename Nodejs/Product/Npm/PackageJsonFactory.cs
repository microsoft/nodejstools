// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.NodejsTools.Npm.SPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm
{
    public static class PackageJsonFactory
    {
        public static IPackageJson Create(string fullPathToFile)
        {
            if (string.IsNullOrEmpty(fullPathToFile) && !fullPathToFile.EndsWith("package.json"))
            {
                throw new ArgumentException("Expected full path to 'package.json' file.", nameof(fullPathToFile));
            }

            if (File.Exists(fullPathToFile))
            {
                var retryInterval = 500;
                var attempts = 5;

                // populate _source with retries for recoverable errors.
                while (--attempts >= 0)
                {
                    try
                    {
                        using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(fin))
                        {
                            return Create(reader);
                        }
                    }
                    catch (PackageJsonException pje)
                    {
                        WrapExceptionAndRethrow(pje);
                    }
                    catch (Exception exc) when (exc is IOException || exc is UnauthorizedAccessException)
                    {
                        if (attempts <= 0) { throw; }
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }

            return null;

            void WrapExceptionAndRethrow(Exception ex)
            {
                throw new PackageJsonException(
                    string.Format(CultureInfo.CurrentCulture, @"Error reading package.json at '{0}': {1}", fullPathToFile, ex.Message),
                    ex);
            }
        }

        public static IPackageJson Create(TextReader reader)
        {
            object source = null;
            try
            {
                var text = reader.ReadToEnd();
                try
                {
                    // JsonConvert and JObject.Parse exhibit slightly different behavior,
                    // so fall back to JObject.Parse if JsonConvert does not properly deserialize
                    // the object.
                    source = JsonConvert.DeserializeObject(text);
                }
                catch (ArgumentException)
                {
                    source = JObject.Parse(text);
                }
            }
            catch (Exception jre) when (jre is JsonReaderException || jre is JsonSerializationException || jre is FormatException)
            {
                WrapExceptionAndRethrow(jre);
            }
            catch (ArgumentException ae)
            {
                throw new PackageJsonException(
                    string.Format(CultureInfo.CurrentCulture, @"Error reading package.json. The file may be parseable JSON but may contain objects with duplicate properties.

The following error occurred:

{0}", ae.Message),
                    ae);
            }

            return source == null ? null : new PackageJson(source);

            void WrapExceptionAndRethrow(Exception ex)
            {
                throw new PackageJsonException(
                    string.Format(CultureInfo.CurrentCulture, @"Unable to read package.json. Please ensure the file is valid JSON.

Reading failed because the following error occurred:

{0}", ex.Message),
                    ex);
            }
        }
    }
}
