// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm
{
    public interface IPackageCatalog
    {
        DateTime LastRefreshed { get; }

        Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText, Uri registryUrl = null);

        IPackage this[string name] { get; }

        long? ResultsCount { get; }
    }

    public sealed class EmptyPackageCatalog : IPackageCatalog
    {
        private static readonly Uri defaultRegistryUri = new Uri("https://registry.npmjs.org/");

        public static readonly IPackageCatalog Instance = new EmptyPackageCatalog();

        private EmptyPackageCatalog() { }

        IPackage IPackageCatalog.this[string name] => default(IPackage);

        DateTime IPackageCatalog.LastRefreshed => DateTime.Now;

        long? IPackageCatalog.ResultsCount => 0;

        async Task<IEnumerable<IPackage>> IPackageCatalog.GetCatalogPackagesAsync(string filterText, Uri registryUrl)
        {
            var relativeUri = string.Format("/-/v1/search?text={0}", filterText);
            var searchUri = new Uri(registryUrl = registryUrl ?? defaultRegistryUri, relativeUri);

            var request = WebRequest.Create(searchUri);
            using (var response = await request.GetResponseAsync())
            {
                var reader = new StreamReader(response.GetResponseStream());
                using (var jsonReader = new JsonTextReader(reader))
                {
                    while (jsonReader.Read())
                    {
                        switch (jsonReader.TokenType)
                        {
                            case JsonToken.StartObject:
                                continue;
                            case JsonToken.PropertyName:
                                if( StringComparer.OrdinalIgnoreCase.Equals(jsonReader.Value, "objects"))
                                {
                                    continue;
                                }
                                continue;
//                                throw new InvalidOperationException("Unexpected json token.");
                            case JsonToken.StartArray:
                                // looks like the json we get back is different .. we get an 'objects' object which contains an array of packages, and 2 properties.
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

        private IEnumerable<IPackage> ReadPackagesFromArray(JsonTextReader jsonReader)
        {
            var pkgList = new List<IPackage>();

            // Inside the array, each object is an NPM package
            var builder = new NodeModuleBuilder();
            while (jsonReader.Read())
            {
                switch (jsonReader.TokenType)
                {
                    case JsonToken.PropertyName:
                        if (StringComparer.OrdinalIgnoreCase.Equals(jsonReader.Value, "package"))
                        {
                            var token = (JProperty)JToken.ReadFrom(jsonReader);
                            var package = ReadPackage(token.Value, builder);
                            if (package != null)
                            {
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

        private IPackage ReadPackage(JToken package, NodeModuleBuilder builder)
        {
            builder.Reset();

            try
            {
                builder.Name = (string)package["name"];
                if (string.IsNullOrEmpty(builder.Name))
                {
                    // I don't believe this should ever happen if the data returned is
                    // well formed. Could throw an exception, but just skip instead for
                    // resiliency on the NTVS side.
                    return null;
                }

                builder.AppendToDescription((string)package["description"] ?? string.Empty);

                var time = package["time"];
                if (time != null)
                {
                    builder.AppendToDate((string)time["modified"]);
                }

                var distTags = package["dist-tags"];
                if (distTags != null)
                {
                    var latestVersion = distTags
                        .OfType<JProperty>()
                        .Where(v => (string)v.Name == "latest")
                        .Select(v => (string)v.Value)
                        .FirstOrDefault();

                    if (!string.IsNullOrEmpty(latestVersion))
                    {
                        try
                        {
                            builder.LatestVersion = SemverVersion.Parse(latestVersion);
                        }
                        catch (SemverVersionFormatException)
                        {
                            //OnOutputLogged(string.Format(CultureInfo.InvariantCulture,
                            //    Resources.InvalidPackageSemVersion,
                            //    latestVersion,
                            //    builder.Name));
                        }
                    }
                }

                var versions = package["versions"];
                if (versions != null)
                {
                    builder.AvailableVersions = GetVersions(versions);
                }

                AddKeywords(builder, package["keywords"]);
                AddAuthor(builder, package["author"]);
                AddHomepage(builder, package["homepage"]);

                return builder.Build();
            }
            catch (InvalidOperationException)
            {
                // Occurs if a JValue appears where we expect JProperty
                return null;
            }
            catch (ArgumentException)
            {
                //OnOutputLogged(string.Format(CultureInfo.CurrentCulture, Resources.ParsingError, builder.Name));
                return null;
            }
        }

        private IEnumerable<SemverVersion> GetVersions(JToken versionsToken)
        {
            var versionStrings = versionsToken.OfType<JProperty>().Select(v => (string)v.Name);
            foreach (var versionString in versionStrings)
            {
                if (!string.IsNullOrEmpty(versionString))
                {
                    SemverVersion ver;
                    try
                    {
                        ver = SemverVersion.Parse(versionString);
                    }
                    catch (SemverVersionFormatException)
                    {
                        continue;
                    }
                    yield return ver;
                }
            }
        }

        private static void AddKeywords(NodeModuleBuilder builder, JToken keywords)
        {
            if (keywords != null)
            {
                foreach (var keyword in keywords.Select(v => (string)v))
                {
                    builder.AddKeyword(keyword);
                }
            }
        }

        private static void AddHomepage(NodeModuleBuilder builder, JToken homepage)
        {
            JArray homepageArray;
            string homepageString;

            if ((homepageArray = homepage as JArray) != null)
            {
                foreach (var subHomepage in homepageArray)
                {
                    AddHomepage(builder, subHomepage);
                }
            }
            else if (!string.IsNullOrEmpty(homepageString = (string)homepage))
            {
                builder.AddHomepage(homepageString);
            }
        }

        private static void AddAuthor(NodeModuleBuilder builder, JToken author)
        {
            JArray authorArray;
            JObject authorObject;
            string name;
            if ((authorArray = author as JArray) != null)
            {
                foreach (var subAuthor in authorArray)
                {
                    AddAuthor(builder, subAuthor);
                }
            }
            else if ((authorObject = author as JObject) != null)
            {
                AddAuthor(builder, authorObject["name"]);
            }
            else if (!string.IsNullOrEmpty(name = (string)author))
            {
                builder.AddAuthor(name);
            }
        }
    }
}

