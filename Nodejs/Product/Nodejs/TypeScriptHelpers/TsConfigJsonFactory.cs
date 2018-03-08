// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.TypeScript
{
    internal static class TsConfigJsonFactory
    {
        public static async Task<TsConfigJson> CreateAsync(string fullPathToFile)
        {
            if (!TypeScriptHelpers.IsTsJsConfigJsonFile(fullPathToFile))
            {
                throw new ArgumentException("Expected full path to 'tsconfig.json' file.", nameof(fullPathToFile));
            }

            if (File.Exists(fullPathToFile))
            {
                var retryInterval = 500;

                // populate _source with retries for recoverable errors.
                for(var attempts = 5; attempts >=0; attempts--)
                {
                    try
                    {
                        using (var fin = new FileStream(fullPathToFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new StreamReader(fin))
                        {
                            object source = null;

                            var text = await reader.ReadToEndAsync();
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

                            return source == null ? null : new TsConfigJson(fullPathToFile, source);
                        }
                    }
                    catch (Exception exc) when (attempts > 0 && (exc is IOException || exc is UnauthorizedAccessException))
                    {
                        // only retry on IO related exceptions
                    }

                    Thread.Sleep(retryInterval);
                    retryInterval *= 2; // exponential backoff
                }
            }

            return null;
        }
    }
}
