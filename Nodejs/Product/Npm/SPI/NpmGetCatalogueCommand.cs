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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmGetCatalogueCommand : NpmCommand, IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>(); 
        private readonly bool _forceDownload;
        private string _cachePath;

        public NpmGetCatalogueCommand(
            string fullPathToRootPackageDirectory,
            bool forceDownload,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true
        )
            : base(
                fullPathToRootPackageDirectory,
                pathToNpm,
                useFallbackIfNpmNotFound
        ) {
            Arguments = "search";
            _forceDownload = forceDownload;
            LastRefreshed = DateTime.MinValue;
        }

        private static List<IPackage> ParseResultsFromReader(TextReader reader) {
            var builder = new NodeModuleBuilder();
            var results = new List<IPackage>();
            using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader)) {
                var token = JObject.ReadFrom(jsonReader);
                foreach (var module in token.Values()) {
                    try {
                        builder.Name = (string)module["name"];
                        if (string.IsNullOrEmpty(builder.Name)) {
                            continue;
                        }

                        builder.AppendToDescription((string)module["description"] ?? string.Empty);

                        var versions = module["versions"];
                        if (versions != null) {
                            var latestVersion = versions
                                .OfType<JProperty>()
                                .Where(v => (string)v == "latest")
                                .Select(v => v.Name)
                                .FirstOrDefault();

                            if (!string.IsNullOrEmpty(latestVersion)) {
                                builder.Version = SemverVersion.Parse(latestVersion);
                            }
                        }

                        var keywords = module["keywords"];
                        if (keywords != null) {
                            foreach (var keyword in keywords.Select(v => (string)v)) {
                                builder.AddKeyword(keyword);
                            }
                        }

                        AddAuthor(builder, module["author"]);
                        

                        results.Add(builder.Build());
                    } catch (InvalidOperationException) {
                        // Occurs if a JValue appears where we expect JProperty
                    } finally {
                        builder.Reset();
                    }
                }
            }

            return results;
        }

        private static void AddAuthor(NodeModuleBuilder builder, JToken author) {
            JArray authorArray;
            JObject authorObject;
            string name;
            if ((authorArray = author as JArray) != null) {
                foreach (var subAuthor in authorArray) {
                    AddAuthor(builder, subAuthor);
                }
            } else if ((authorObject = author as JObject) != null) {
                AddAuthor(builder, authorObject["name"]);
            } else if (!string.IsNullOrEmpty(name = (string)author)) {
                builder.AddAuthor(name);
            }
        }

        private string CachePath {
            get {
                if (_cachePath == null) {
                    _cachePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "Microsoft",
                        "Node.js Tools",
                        "packagecache.json"
                    );
                }
                return _cachePath;
            }
        }

        private async Task UpdateCache(Uri registry, string filename) {
            registry = registry ?? new Uri("https://registry.npmjs.org/");

            Uri packageUri;
            if (!Uri.TryCreate(registry, "/-/all", out packageUri)) {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(filename));

            var request = WebRequest.Create(packageUri);
            using (var response = await request.GetResponseAsync())
            using (var cache = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                const long ONE_MB = 1024L * 1024L;
                int nextNotification = 1;

                long totalDownloaded = 0, totalLength;
                try {
                    totalLength = response.ContentLength;
                } catch (NotSupportedException) {
                    totalLength = -1;
                }

                OnOutputLogged(string.Format(Resources.PackagesDownloadStarting, packageUri.AbsoluteUri));

                int bytesRead;
                var buffer = new byte[4096];
                while ((bytesRead = await response.GetResponseStream().ReadAsync(buffer, 0, buffer.Length)) > 0) {
                    totalDownloaded += bytesRead;
                    if (totalDownloaded > nextNotification * ONE_MB) {
                        if (totalLength > 0) {
                            OnOutputLogged(string.Format(
                                Resources.PackagesDownloadedXOfYMB,
                                nextNotification,
                                totalLength / ONE_MB + 1
                            ));
                        } else {
                            OnOutputLogged(string.Format(
                                Resources.PackagesDownloadedXMB,
                                nextNotification
                            ));
                        }
                        nextNotification += 1;
                    }

                    await cache.WriteAsync(buffer, 0, bytesRead);
                }
                OnOutputLogged(Resources.PackagesDownloadComplete);
            }
        }

        public override async Task<bool> ExecuteAsync() {
            var filename = CachePath;
            List<IPackage> newResults = null;

            for (int attempt = 0; attempt < 2; attempt++) {
                if (_forceDownload || !File.Exists(filename)) {
                    Uri registry = null;
                    using (var proc = ProcessOutput.RunHiddenAndCapture(GetPathToNpm(), "config", "get", "registry")) {
                        if (await proc == 0) {
                            registry = proc.StandardOutputLines
                                .Select(s => {
                                    Uri u;
                                    return Uri.TryCreate(s, UriKind.Absolute, out u) ? u : null;
                                })
                                .FirstOrDefault(u => u != null);
                        }
                    }

                    await UpdateCache(registry, filename);
                }


                try {
                    if (File.Exists(filename)) {
                        using (var reader = new StreamReader(filename)) {
                            newResults = await Task.Run(() => ParseResultsFromReader(reader));
                        }
                    }
                    break;
                } catch (Exception) {
                    // assume the results are corrupted and try again...
                    File.Delete(filename);
                    continue;
                }
            }

            if (newResults == null || !newResults.Any()) {
                throw new NpmCatalogEmptyException(Resources.ErrNpmCatalogEmpty);
            }

            LastRefreshed = File.GetLastWriteTime(filename);
            Results = new ReadOnlyCollection<IPackage>(newResults);
            PopulateByName(newResults);

            return true;
        }

        private void PopulateByName(IEnumerable<IPackage> source) {
            var target = new Dictionary<string, IPackage>();
            foreach (var package in source) {
                target[package.Name] = package;
            }
            _byName = target;
        }

        public DateTime LastRefreshed { get; private set; }

        public IPackageCatalog Catalog { get { return this; } }

        public IPackage this[string name] {
            get {
                var temp = _byName;
                if (null == temp) {
                    return null;
                }

                IPackage match;
                temp.TryGetValue(name, out match);
                return match;
            }
        }

        public IList<IPackage> Results { get; private set; }
    }
}
