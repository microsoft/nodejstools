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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SQLiteTables;
using Microsoft.VisualStudioTools.Project;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmGetCatalogCommand : NpmCommand, IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
        private readonly bool _forceDownload;
        private string _cachePath;

        public NpmGetCatalogCommand(
            string fullPathToRootPackageDirectory,
            string cachePath,
            bool forceDownload,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true
        )
            : base(
                fullPathToRootPackageDirectory,
                pathToNpm,
                useFallbackIfNpmNotFound
        ) {
            _cachePath = cachePath;
            Arguments = "search";
            _forceDownload = forceDownload;
            LastRefreshed = DateTime.MinValue;
        }

        internal void ParseResultsAndAddToDatabase(TextReader reader, string dbFilename) {

            using (var db = new SQLiteConnection(dbFilename)) {

                db.RunInTransaction(() => {
                    db.CreateTable<CatalogEntry>();
                    db.CreateTable<DbVersion>();
                });

                db.RunInTransaction(() => {
                    using (var jsonReader = new Newtonsoft.Json.JsonTextReader(reader)) {
                        var token = JObject.ReadFrom(jsonReader);

                        db.InsertOrReplace(new DbVersion() {
                            Id = 1,
                            Revision = long.Parse((string)token["_updated"]),
                            UpdatedOn = DateTime.Now
                        });

                        var builder = new NodeModuleBuilder();

                        foreach (var module in token.Values()) {
                            try {
                                builder.Name = (string)module["name"];
                                if (string.IsNullOrEmpty(builder.Name)) {
                                    continue;
                                }

                                builder.AppendToDescription((string)module["description"] ?? string.Empty);

                                var time = module["time"];
                                if (time != null) {
                                    builder.AppendToDate((string)time["modified"]);
                                }

                                var distTags = module["dist-tags"];
                                if (distTags != null) {
                                    var latestVersion = distTags
                                        .OfType<JProperty>()
                                        .Where(v => (string)v.Name == "latest")
                                        .Select(v => (string)v.Value)
                                        .FirstOrDefault();

                                    if (!string.IsNullOrEmpty(latestVersion)) {
                                        builder.LatestVersion = SemverVersion.Parse(latestVersion);
                                    }
                                }

                                var versions = module["versions"];
                                if (versions != null) {
                                    builder.AvailableVersions = GetVersions(versions);
                                }

                                AddKeywords(builder, module["keywords"]);

                                AddAuthor(builder, module["author"]);

                                AddHomepage(builder, module["homepage"]);

                                var package = builder.Build();

                                InsertCatalogEntry(db, package);
                            } catch (InvalidOperationException) {
                                // Occurs if a JValue appears where we expect JProperty
                            } catch (ArgumentException) {
                                OnOutputLogged(string.Format(Resources.ParsingError, builder.Name));
                                if (!string.IsNullOrEmpty(builder.Name)) {
                                    var package = builder.Build();
                                    InsertCatalogEntry(db, package);
                                }
                            } finally {
                                builder.Reset();
                            }
                        }
                    }
                });
            }
        }

        private static void InsertCatalogEntry(SQLiteConnection db, IPackage package) {
            db.InsertOrReplace(new CatalogEntry() {
                Name = package.Name,
                Description = package.Description,
                Author = JsonConvert.SerializeObject(package.Author),
                Version = JsonConvert.SerializeObject(package.Version),
                AvailableVersions = JsonConvert.SerializeObject(package.AvailableVersions),
                Keywords = JsonConvert.SerializeObject(package.Keywords),
                Homepage = JsonConvert.SerializeObject(package.Homepages),
                PublishDateTimeString = package.PublishDateTimeString
            });
        }

        private IEnumerable<SemverVersion> GetVersions(JToken versionsToken) {
            IEnumerable<string> versionStrings = versionsToken.OfType<JProperty>().Select(v=>(string)v.Name);
            foreach (var versionString in versionStrings) {
                if (!string.IsNullOrEmpty(versionString)) {
                    yield return SemverVersion.Parse(versionString);
                }
            }

        }

        private static void AddKeywords(NodeModuleBuilder builder, JToken keywords) {
            if (keywords != null) {
                foreach (var keyword in keywords.Select(v => (string)v)) {
                    builder.AddKeyword(keyword);
                }
            }
        }

        private static string GetJsonStringFromToken(JToken token) {
            string keywords = token != null ? token.ToString() : null;
            return keywords;
        }

        private static void AddHomepage(NodeModuleBuilder builder, JToken homepage) {
            JArray homepageArray;
            string homepageString;

            if ((homepageArray = homepage as JArray) != null) {
                foreach (var subHomepage in homepageArray) {
                    AddHomepage(builder, subHomepage);
                }
            } else if (!string.IsNullOrEmpty(homepageString = (string)homepage)) {
                builder.AddHomepage(homepageString);
            }
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
                return _cachePath;
            }
        }

        private async Task<string> DownloadPackageJsonCache(Uri registry, string cachePath, long refreshStartKey = 0) {
            registry = registry ?? new Uri("https://registry.npmjs.org/");

            string relativeUri, filename;
            if (refreshStartKey > 0) {
                relativeUri = String.Format("/-/all/since?stale=update_after&startkey={0}", refreshStartKey);
                filename = Path.Combine(cachePath, "since_packages.json");
            }  else {
                relativeUri = "/-/all";
                filename = Path.Combine(cachePath, "all_packages.json");
            }

            OnOutputLogged(string.Format(Resources.InfoPackageCacheWriteLocation, filename));


            Uri packageUri;
            if (!Uri.TryCreate(registry, relativeUri, out packageUri)) {
                return null;
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

            return filename;
        }

        public override async Task<bool> ExecuteAsync() {
            var dbFilename = Path.Combine(CachePath, "packagecache.sqlite");

            string filename = null;

            List<IPackage> results = null;
            bool catalogUpdated = false;

            // Use a semaphore instead of a mutex because await may return to a thread other than the calling thread.
            using (var semaphore = new Semaphore(1, 1, dbFilename.Replace('\\', '/'))) {
                // Wait until file is downloaded/parsed if another download is already in session.
                // Allows user to open multiple npm windows and show progress bar without file-in-use errors.
                bool success = await Task.Run(() => semaphore.WaitOne(TimeSpan.FromMinutes(5)));
                if (!success) {
                    // Return immediately so that the user can explicitly decide to refresh on failure.
                    return false;
                }

                try {
                    if (!File.Exists(dbFilename)) {
                        filename = await UpdatePackageCache();
                        catalogUpdated = true;
                    } else if (_forceDownload) {
                        DbVersion version;
                        using (var db = new SQLiteConnection(dbFilename)) {
                            version = db.Table<DbVersion>().First();
                        }

                        filename = await UpdatePackageCache(version.Revision);
                        catalogUpdated = true;
                    }

                    if (catalogUpdated) {
                        var fileInfo = new FileInfo(filename);
                        OnOutputLogged(String.Format(Resources.InfoReadingBytesFromPackageCache, fileInfo.Length, filename, fileInfo.LastWriteTime));

                        using (var reader = new StreamReader(filename)) {
                            await Task.Run(() => ParseResultsAndAddToDatabase(reader, dbFilename));
                        }
                    }

                    results = await Task.Run(() => ReadResultsFromDatabase(dbFilename));

                } catch (Exception ex) {
                    if (ex is StackOverflowException ||
                        ex is OutOfMemoryException ||
                        ex is ThreadAbortException ||
                        ex is AccessViolationException) {
                        throw;
                    }
                    // assume the results are corrupted
                    OnOutputLogged(ex.ToString());
                } finally {
                    if (results == null) {
                        OnOutputLogged(string.Format(Resources.DownloadOrParsingFailed, CachePath));
                    } else if (!results.Any()) {
                        // Database file exists, but is corrupt. Delete database, so that we can download the file next time arround.
                        OnOutputLogged(string.Format(Resources.DatabaseCorrupt, dbFilename));
                        SafeDeleteFile(dbFilename);
                    }
                    semaphore.Release(1);
                }
            }

            LastRefreshed = File.GetLastWriteTime(dbFilename);
            Results = new ReadOnlyCollection<IPackage>(results ?? new List<IPackage>());
            PopulateByName(results);

            OnOutputLogged(String.Format(Resources.InfoCurrentTime, DateTime.Now));
            OnOutputLogged(String.Format(Resources.InfoLastRefreshed, LastRefreshed));
            OnOutputLogged(String.Format(Resources.InfoNumberOfResults, Results.LongCount()));

            return true;
        }

        private void SafeDeleteFile(string filename) {
            try {
                OnOutputLogged(string.Format(Resources.InfoDeletingFile, filename));
                File.Delete(filename);
            } catch (DirectoryNotFoundException) {
                // File has already been deleted. Do nothing.
            } catch (IOException exception) {
                // files are in use or path is too long
                OnOutputLogged(exception.Message);
                OnOutputLogged(string.Format(Resources.FailedToDeleteFile, filename));
            } catch (Exception exception) {
                OnOutputLogged(exception.ToString());
                OnOutputLogged(string.Format(Resources.FailedToDeleteFile, filename));
            }
        }

        internal static List<IPackage> ReadResultsFromDatabase(string cacheFile) {
            // TODO: we shouldn't be loading all results from the database into memory.
            // We should query the database instead. https://nodejstools.codeplex.com/workitem/1438
            var results = new List<IPackage>();
            using (var db = new SQLiteConnection(cacheFile)) {
                db.RunInTransaction(() => {
                    var enumerator = db.Table<CatalogEntry>().OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).GetEnumerator();
                    while (enumerator.MoveNext()) {
                        var package = new PackageProxy();
                        CatalogEntry entry = enumerator.Current;

                        package.Name = entry.Name;
                        package.Description = entry.Description;
                        package.Author = JsonConvert.DeserializeObject<Person>(entry.Author);
                        package.Keywords = JsonConvert.DeserializeObject<IEnumerable<string>>(entry.Keywords) ?? new List<string>();
                        package.Version = JsonConvert.DeserializeObject<SemverVersion>(entry.Version);
                        package.AvailableVersions = JsonConvert.DeserializeObject<IEnumerable<SemverVersion>>(entry.AvailableVersions);
                        package.Homepages = JsonConvert.DeserializeObject<IEnumerable<string>>(entry.Homepage) ?? new List<string>();
                        package.PublishDateTimeString = entry.PublishDateTimeString;

                        results.Add(package);
                    }
                });
            }
            return results;
        }

        private async Task<string> UpdatePackageCache(long refreshStartKey = 0) {
            string filename;
            Uri registry = null;
            string pathToNpm = GetPathToNpm();
            OnOutputLogged(String.Format(Resources.InfoNpmPathLocation, pathToNpm));

            using (var proc = ProcessOutput.RunHiddenAndCapture(pathToNpm, "config", "get", "registry")) {
                if (await proc == 0) {
                    registry = proc.StandardOutputLines
                        .Select(s => {
                            Uri u;
                            return Uri.TryCreate(s, UriKind.Absolute, out u) ? u : null;
                        })
                        .FirstOrDefault(u => u != null);
                }
            }

            filename = await DownloadPackageJsonCache(registry, CachePath, refreshStartKey);

            return filename;
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
