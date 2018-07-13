// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
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
            if (fullPathToFile == null || !IsPackageJsonFile(fullPathToFile) || !Path.IsPathRooted(fullPathToFile))
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
                            object source = null;

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

                            return source == null ? null : new PackageJson(source);
                        }
                    }
                    catch (Exception exc) when (attempts > 0 && (exc is IOException || exc is UnauthorizedAccessException))
                    {
                        // only retry on IO related exceptions
                    }
                    catch (Exception exc) when (exc is JsonReaderException || exc is JsonSerializationException || exc is FormatException || exc is ArgumentException)
                    {
                        // on deserialization exceptions wrap in specific exception which the callers will handle
                        throw new PackageJsonException($"Error reading '{fullPathToFile}'. The file may be parseable JSON but may contain objects with duplicate properties. " +
                                    $"The following error occurred: '{exc.Message}'", exc);
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }

            return null;
        }

        public static bool IsPackageJsonFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var fileName = Path.GetFileName(filePath);
            return StringComparer.OrdinalIgnoreCase.Equals(fileName, NodejsConstants.PackageJsonFile);
        }
    }
}
