//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm {
    public interface IPackageCatalog {
        DateTime LastRefreshed { get; }

        Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText);

        long? ResultsCount { get; }
    }

    // This class is a wrapper catalog to directly query the NPM repo
    // instead of downloading the entire catalog (which is no longer supported).
    internal sealed class EmptyPackageCatalog : IPackageCatalog {

        private static readonly Uri defaultRegistryUri = new Uri("https://registry.npmjs.org/");

        public static readonly IPackageCatalog Instance = new EmptyPackageCatalog();

        private EmptyPackageCatalog() { }

        public DateTime LastRefreshed => DateTime.Now;

        public long? ResultsCount => 0;

        public async Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText) {

            // All exceptions thrown here and in the called methods are handled by the
            // NPM search dialog, so we don't have to do any exception handling here.
            var relativeUri = string.Format("/-/v1/search?text={0}", WebUtility.UrlEncode(filterText));
            var searchUri = new Uri(defaultRegistryUri, relativeUri);

            var request = WebRequest.Create(searchUri);
            using (var response = await request.GetResponseAsync()) {
                var reader = new StreamReader(response.GetResponseStream());
                using (var jsonReader = new JsonTextReader(reader)) {
                    while (jsonReader.Read()) {
                        switch (jsonReader.TokenType) {
                            case JsonToken.StartObject:
                            case JsonToken.PropertyName:
                                continue;
                            case JsonToken.StartArray:
                                return ReadPackagesFromArray(jsonReader);
                            default:
                                throw new InvalidOperationException("Unexpected json token.");
                        }
                    }
                }
            }

            // should never get here
            throw new InvalidOperationException("Unexpected json token.");
        }

        private IEnumerable<IPackage> ReadPackagesFromArray(JsonTextReader jsonReader) {
            var pkgList = new List<IPackage>();

            // Inside the array, each object is an NPM package
            var builder = new NodeModuleBuilder();
            while (jsonReader.Read()) {
                switch (jsonReader.TokenType) {
                    case JsonToken.PropertyName:
                        if (StringComparer.OrdinalIgnoreCase.Equals(jsonReader.Value, "package")) {
                            var token = (JProperty)JToken.ReadFrom(jsonReader);
                            var package = ReadPackage(token.Value, builder);
                            if (package != null) {
                                pkgList.Add(package);
                            }
                        }
                        continue;
                    case JsonToken.EndArray:
                        // This is the spot the function should always exit on valid data
                        return pkgList;
                    default:
                        continue;
                }
            }
            throw new JsonException("Unexpected end of stream reading the NPM catalog data array");
        }

        private IPackage ReadPackage(JToken package, NodeModuleBuilder builder) {
            builder.Reset();

            try {
                builder.Name = (string)package["name"];
                if (string.IsNullOrEmpty(builder.Name)) {
                    // I don't believe this should ever happen if the data returned is
                    // well formed. Could throw an exception, but just skip instead for
                    // resiliency on the NTVS side.
                    return null;
                }

                builder.AppendToDescription((string)package["description"] ?? string.Empty);

                var date = package["date"];
                if (date != null) {
                    builder.AppendToDate((string)date);
                }

                var version = package["version"];
                if (version != null) {
                    var semver = SemverVersion.Parse((string)version);
                    builder.AddVersion(semver);
                }

                AddKeywords(builder, package["keywords"]);
                AddAuthor(builder, package["author"]);
                AddHomepage(builder, package["links"]);

                return builder.Build();
            } catch (InvalidOperationException) {
                // Occurs if a JValue appears where we expect JProperty
                return null;
            } catch (ArgumentException) {
                return null;
            }
        }

        private static void AddKeywords(NodeModuleBuilder builder, JToken keywords) {
            if (keywords != null) {
                foreach (var keyword in keywords.Select(v => (string)v)) {
                    builder.AddKeyword(keyword);
                }
            }
        }

        private static void AddHomepage(NodeModuleBuilder builder, JToken links) {
            var homepage = links?["homepage"];
            if (homepage != null) {
                builder.AddHomepage((string)homepage);
            }
        }

        private static void AddAuthor(NodeModuleBuilder builder, JToken author) {
            var name = author?["name"];
            if (author != null) {
                builder.AddAuthor((string)name);
            }
        }
    }
}
