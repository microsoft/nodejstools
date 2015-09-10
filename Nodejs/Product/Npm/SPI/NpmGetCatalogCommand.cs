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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm.SQLiteTables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace Microsoft.NodejsTools.Npm.SPI {
    internal class NpmGetCatalogCommand : NpmCommand, IPackageCatalog {
        private IDictionary<string, IPackage> _byName = new Dictionary<string, IPackage>();
        private readonly bool _forceDownload;
        private readonly string _cachePath;
        private Uri _registryUrl;
        private readonly IProgress<string> _progress;

        private const int _databaseSchemaVersion = 3;

        public NpmGetCatalogCommand(
            string fullPathToRootPackageDirectory,
            string cachePath,
            bool forceDownload,
            string registryUrl = null,
            string pathToNpm = null,
            bool useFallbackIfNpmNotFound = true,
            IProgress<string> progress = null
        )
            : base(
                fullPathToRootPackageDirectory,
                pathToNpm) {
            _cachePath = cachePath;
            Arguments = "search";
            _forceDownload = forceDownload;
            if (registryUrl != null) {
                _registryUrl = new Uri(registryUrl);
            }
            LastRefreshed = DateTime.MinValue;
            _progress = progress;
        }

        internal void ParseResultsAndAddToDatabase(TextReader reader,
                                                   string dbFilename,
                                                   string registryUrl) {

            Directory.CreateDirectory(Path.GetDirectoryName(dbFilename));

            using (var db = new SQLiteConnection(dbFilename)) {
                db.RunInTransaction(() => {
                    db.CreateRegistryTableIfNotExists();

                    using (var jsonReader = new JsonTextReader(reader)) {
                        /*
                        The schema seems to have changed over time.

                        The first format we need to handle is an object literal. It
                        starts with an "_updated" property, with a value of the
                        timestamp it was retrived, and then a property for each
                        package, with a name of the package name, and a value which
                        is on object literal representing the package info. An example
                        downloaded may start:

{
"_updated": 1413573404788,
"unlink-empty-files": {
  "name": "unlink-empty-files",
  "description": "given a directory, unlink (remove) all files with a length of 0",
  "dist-tags": { "latest": "1.0.1" },
  "maintainers": [
    {
      "name": "kesla",
etc.

                        The other format is an array literal, where each element is an
                        object literal for a package, similar to the value of the
                        properties above, for example:

[
{"name":"008-somepackage","description":"Test Package","dist-tags":{"latest":"1.1.1"}..
,
{"name":"01-simple","description":"That is the first app in order to study the ..."
,
etc.

                        In this second format, there is no "_updated" property with a
                        timestamp, and the 'Date' timestamp from the HTTP request for
                        the data is used instead.

                        The NPM code that handles the payload seems to be written in
                        a way to handle both formats
                        See https://github.com/npm/npm/blob/2.x-release/lib/cache/update-index.js#L87
                        */
                        jsonReader.Read();
                        switch (jsonReader.TokenType) {
                            case JsonToken.StartObject:
                                ReadPackagesFromObject(db, jsonReader, registryUrl);
                                break;
                            case JsonToken.StartArray:
                                // The array format doesn't contain the "_update" field,
                                // so create a rough timestamp. Use the time from 30 mins
                                // ago (to set it before the download request started),
                                // converted to a JavaScript value (milliseconds since
                                // start of 1970)
                                var timestamp = DateTime.UtcNow
                                    .Subtract(new DateTime(1970, 1, 1, 0, 30, 0, DateTimeKind.Utc))
                                    .TotalMilliseconds;
                                ReadPackagesFromArray(db, jsonReader);
                                db.InsertOrReplace(new RegistryInfo() {
                                    RegistryUrl = registryUrl,
                                    Revision = (long)timestamp,
                                    UpdatedOn = DateTime.Now
                                });
                                break;
                            default:
                                throw new JsonException("Unexpected JSON token at start of NPM catalog data");
                        }
                    }

                    // FTS doesn't support INSERT OR REPLACE. This is the most efficient way to bypass that limitation.
                    db.Execute("DELETE FROM CatalogEntry WHERE docid NOT IN (SELECT MAX(docid) FROM CatalogEntry GROUP BY Name)");
                });
            }
        }

        private void ReadPackagesFromObject(SQLiteConnection db,
                                            JsonTextReader jsonReader,
                                            string registryUrl) {
            var builder = new NodeModuleBuilder();
            while (jsonReader.Read()) {
                if (jsonReader.TokenType == JsonToken.EndObject) {
                    // Reached the end of the object literal containing the data.
                    // This should be the normal exit point.
                    return;
                }

                // Every property should be either the "_updated" value, or a package
                if (jsonReader.TokenType != JsonToken.PropertyName) {
                    throw new JsonException("Unexpected JSON token in NPM catalog data");
                }
                string propertyName = (string)jsonReader.Value;

                // If it's "_updated", update the revision info.
                if (propertyName.Equals("_updated", StringComparison.Ordinal)) {
                    jsonReader.Read();
                    db.InsertOrReplace(new RegistryInfo() {
                        RegistryUrl = registryUrl,
                        Revision = (long)jsonReader.Value,
                        UpdatedOn = DateTime.Now
                    });
                    continue;
                }

                // Else the property should be an object literal representing the package
                jsonReader.Read();
                if (jsonReader.TokenType == JsonToken.StartObject) {
                    IPackage package = ReadPackage(jsonReader, builder);
                    if (package != null) {
                        InsertCatalogEntry(db, package);
                    }
                } else {
                    throw new JsonException("Unexpected JSON token reading a package from the NPM catalog data");
                }
            }
            throw new JsonException("Unexpected end of stream reading the NPM catalog data object");
        }

        private void ReadPackagesFromArray(SQLiteConnection db, JsonTextReader jsonReader) {
            // Inside the array, each object is an NPM package
            var builder = new NodeModuleBuilder();
            while (jsonReader.Read()) {
                switch (jsonReader.TokenType) {
                    case JsonToken.StartObject:
                        IPackage package = ReadPackage(jsonReader, builder);
                        if (package != null) {
                            InsertCatalogEntry(db, package);
                        }
                        break;
                    case JsonToken.EndArray:
                        // This is the spot the function should always exit on valid data
                        return;
                    default:
                        throw new JsonException("Unexpected JSON token in NPM catalog data array");
                }
            }
            throw new JsonException("Unexpected end of stream reading the NPM catalog data array");
        }

        private IPackage ReadPackage(JsonTextReader jsonReader, NodeModuleBuilder builder) {
            IPackage package = null;
            builder.Reset();

            try {
                // The JsonTextReader should be positioned at the start of the
                // object literal token for the package
                var module = JToken.ReadFrom(jsonReader);

                builder.Name = (string)module["name"];
                if (string.IsNullOrEmpty(builder.Name)) {
                    // I don't believe this should ever happen if the data returned is
                    // well formed. Could throw an exception, but just skip instead for
                    // resiliency on the NTVS side.
                    return null;
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
                        try {
                            builder.LatestVersion = SemverVersion.Parse(latestVersion);
                        } catch (SemverVersionFormatException) {
                            OnOutputLogged(String.Format(
                                Resources.InvalidPackageSemVersion,
                                latestVersion,
                                builder.Name));
                        }
                    }
                }

                var versions = module["versions"];
                if (versions != null) {
                    builder.AvailableVersions = GetVersions(versions);
                }

                AddKeywords(builder, module["keywords"]);
                AddAuthor(builder, module["author"]);
                AddHomepage(builder, module["homepage"]);

                package = builder.Build();
            } catch (InvalidOperationException) {
                // Occurs if a JValue appears where we expect JProperty
                return null;
            } catch (ArgumentException) {
                OnOutputLogged(string.Format(Resources.ParsingError, builder.Name));
                return null;
            }
            return package;
        }

        private static void InsertCatalogEntry(SQLiteConnection db, IPackage package) {
            db.Insert(new CatalogEntry() {
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
            IEnumerable<string> versionStrings = versionsToken.OfType<JProperty>().Select(v => (string)v.Name);
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

        internal const string RegistryCacheFilename = "registrycache.sqlite";

        internal const string DatabaseCacheFilename = "packagecache.sqlite";

        private string DatabaseCacheFilePath {
            get { return Path.Combine(CachePath, DatabaseCacheFilename); }
        }

        private async Task<string> DownloadPackageJsonCache(Uri registry, string cachePath, long refreshStartKey = 0) {
            string relativeUri, filename;

            if (refreshStartKey > 0) {
                relativeUri = String.Format("-/all/since?stale=update_after&startkey={0}", refreshStartKey);
                filename = Path.Combine(cachePath, "since_packages.json");
            } else {
                relativeUri = "-/all";
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

                var progress = string.Format(Resources.PackagesDownloadStarting, packageUri.AbsoluteUri);
                OnOutputLogged(progress);
                if (_progress != null) {
                    _progress.Report(progress);
                }

                var buffer = new byte[4096];

                using (var stream = response.GetResponseStream()) {
                    if (stream == null) return null;

                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                        totalDownloaded += bytesRead;

                        if (totalDownloaded > nextNotification * ONE_MB) {
                            if (totalLength > 0) {
                                progress = string.Format(
                                    Resources.PackagesDownloadedXOfYMB,
                                    nextNotification,
                                    totalLength / ONE_MB + 1
                                );
                            } else {
                                progress = string.Format(
                                    Resources.PackagesDownloadedXMB,
                                    nextNotification
                                );
                            }
                            OnOutputLogged(progress);
                            if (_progress != null) {
                                _progress.Report(progress);
                            }
                            nextNotification += 1;
                        }

                        await cache.WriteAsync(buffer, 0, bytesRead);
                    }

                    OnOutputLogged(Resources.PackagesDownloadComplete);

                    if (_progress != null) {
                        _progress.Report(Resources.PackagesDownloadComplete);
                    }
                }
            }

            return filename;
        }


        private async Task<Uri> GetRegistryUrl() {
            var output = await NpmHelpers.ExecuteNpmCommandAsync(
                null,
                GetPathToNpm(),
                FullPathToRootPackageDirectory,
                new[] { "config", "get", "registry" },
                null);

            if (output != null) {
                _registryUrl = output
                    .Select(s => {
                        Uri u;
                        return Uri.TryCreate(s, UriKind.Absolute, out u) ? u : null;
                    })
                    .FirstOrDefault(u => u != null);
            }
            _registryUrl = _registryUrl ?? new Uri("https://registry.npmjs.org/");
            return _registryUrl;
        }

        public override async Task<bool> ExecuteAsync() {
            var dbFilename = DatabaseCacheFilePath;
            bool catalogUpdated = false;
            string filename = null;
            string registryCacheDirectory = null;
            string registryCachePath = null;
            string registryCacheFilePath = null;

            // Use a semaphore instead of a mutex because await may return to a thread other than the calling thread.
            using (var semaphore = GetDatabaseSemaphore()) {
                // Wait until file is downloaded/parsed if another download is already in session.
                // Allows user to open multiple npm windows and show progress bar without file-in-use errors.
                bool success = await Task.Run(() => semaphore.WaitOne(TimeSpan.FromMinutes(5)));
                if (!success) {
                    // Return immediately so that the user can explicitly decide to refresh on failure.
                    return false;
                }

                Uri registryUrl = await GetRegistryUrl();
                OnOutputLogged(string.Format(Resources.InfoRegistryUrl, registryUrl));

                try {
                    DbVersion version = null;
                    RegistryInfo registryInfo = null;
                    RegistryFileMapping registryFileMapping = null;

                    Directory.CreateDirectory(Path.GetDirectoryName(dbFilename));

                    using (var db = new SQLiteConnection(dbFilename)) {
                        // prevent errors from occurring when table doesn't exist
                        db.CreateCatalogTableIfNotExists();
                        version = db.Table<DbVersion>().FirstOrDefault();
                        registryFileMapping = db.Table<RegistryFileMapping>().FirstOrDefault(info => info.RegistryUrl == registryUrl.ToString());
                    }

                    registryCacheDirectory = registryFileMapping != null ? registryFileMapping.DbFileLocation : Guid.NewGuid().ToString();
                    registryCachePath = Path.Combine(CachePath, registryCacheDirectory);
                    registryCacheFilePath = Path.Combine(registryCachePath, RegistryCacheFilename);

                    Directory.CreateDirectory(Path.GetDirectoryName(registryCacheFilePath));

                    if (File.Exists(registryCacheFilePath)) {
                        using (var registryDb = new SQLiteConnection(registryCacheFilePath)) {
                            // prevent errors from occurring when table doesn't exist
                            registryDb.CreateRegistryTableIfNotExists();
                            registryInfo = registryDb.Table<RegistryInfo>().FirstOrDefault();
                        }
                    }

                    bool correctDatabaseSchema = version != null && version.Id == _databaseSchemaVersion;
                    bool incrementalUpdate = correctDatabaseSchema && _forceDownload && registryInfo != null && registryInfo.Revision > 0;
                    bool fullUpdate = correctDatabaseSchema && (registryInfo == null || registryInfo.Revision <= 0);

                    if (!correctDatabaseSchema) {
                        OnOutputLogged(Resources.InfoCatalogUpgrade);
                        SafeDeleteFolder(CachePath);

                        CreateCatalogDatabaseAndInsertEntries(dbFilename, registryUrl, registryCacheDirectory);

                        filename = await UpdatePackageCache(registryUrl, CachePath);
                        catalogUpdated = true;
                    } else if (incrementalUpdate) {
                        filename = await UpdatePackageCache(registryUrl, registryCachePath, registryInfo.Revision);
                        catalogUpdated = true;
                    } else if (fullUpdate) {
                        CreateCatalogDatabaseAndInsertEntries(dbFilename, registryUrl, registryCacheDirectory);

                        filename = await UpdatePackageCache(registryUrl, registryCachePath);
                        catalogUpdated = true;
                    }

                    if (catalogUpdated) {
                        var fileInfo = new FileInfo(filename);
                        OnOutputLogged(String.Format(Resources.InfoReadingBytesFromPackageCache, fileInfo.Length, filename, fileInfo.LastWriteTime));

                        using (var reader = new StreamReader(filename)) {
                            await Task.Run(() => ParseResultsAndAddToDatabase(reader, registryCacheFilePath, registryUrl.ToString()));
                        }
                    }

                    using (var db = new SQLiteConnection(registryCacheFilePath)) {
                        db.CreateRegistryTableIfNotExists();
                        ResultsCount = db.Table<CatalogEntry>().Count();
                    }
                } catch (Exception ex) {
                    if (ex is StackOverflowException ||
                        ex is OutOfMemoryException ||
                        ex is ThreadAbortException ||
                        ex is AccessViolationException) {
                        throw;
                    }
                    // assume the results are corrupted
                    OnOutputLogged(ex.ToString());
                    throw;
                } finally {
                    if (ResultsCount == null) {
                        OnOutputLogged(string.Format(Resources.DownloadOrParsingFailed, CachePath));
                        SafeDeleteFolder(registryCacheDirectory);
                    } else if (ResultsCount <= 0) {
                        // Database file exists, but is corrupt. Delete database, so that we can download the file next time arround.
                        OnOutputLogged(string.Format(Resources.DatabaseCorrupt, dbFilename));
                        SafeDeleteFolder(registryCacheDirectory);
                    }

                    semaphore.Release();
                }
            }

            LastRefreshed = File.GetLastWriteTime(registryCacheFilePath);

            OnOutputLogged(String.Format(Resources.InfoCurrentTime, DateTime.Now));
            OnOutputLogged(String.Format(Resources.InfoLastRefreshed, LastRefreshed));
            if (ResultsCount != null) {
                OnOutputLogged(String.Format(Resources.InfoNumberOfResults, ResultsCount));
            }

            return true;
        }

        internal void CreateCatalogDatabaseAndInsertEntries(string dbFilename, Uri registryUrl, string registryCacheDirectory) {
            Directory.CreateDirectory(Path.GetDirectoryName(dbFilename));

            using (var db = new SQLiteConnection(dbFilename)) {
                // prevent errors from occurring when table doesn't exist
                db.RunInTransaction(() => {
                    db.CreateCatalogTableIfNotExists();
                    db.InsertOrReplace(new DbVersion() {
                        Id = _databaseSchemaVersion
                    });
                    db.InsertOrReplace(new RegistryFileMapping() {
                        RegistryUrl = registryUrl.ToString(),
                        DbFileLocation = registryCacheDirectory
                    });
                });
            }
        }

        private bool SafeDeleteFolder(string filename) {
            try {
                OnOutputLogged(string.Format(Resources.InfoDeletingFile, filename));
                Directory.Delete(filename, true);
            } catch (DirectoryNotFoundException) {
                // File has already been deleted. Do nothing.
            } catch (IOException exception) {
                // files are in use or path is too long
                OnOutputLogged(exception.Message);
                OnOutputLogged(string.Format(Resources.FailedToDeleteFile, filename));
                return false;
            } catch (Exception exception) {
                OnOutputLogged(exception.ToString());
                OnOutputLogged(string.Format(Resources.FailedToDeleteFile, filename));
                return false;
            }
            return true;
        }

        private Semaphore GetDatabaseSemaphore() {
            return new Semaphore(1, 1, DatabaseCacheFilePath.Replace('\\', '/'));
        }

        private async Task<string> UpdatePackageCache(Uri registry, string cachePath, long refreshStartKey = 0) {
            string pathToNpm = GetPathToNpm();
            OnOutputLogged(String.Format(Resources.InfoNpmPathLocation, pathToNpm));

            string filename = await DownloadPackageJsonCache(registry, cachePath, refreshStartKey);
            return filename;
        }

        public DateTime LastRefreshed { get; private set; }

        public IPackageCatalog Catalog { get { return this; } }

        public async Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText, Uri registryUrl = null) {
            IEnumerable<IPackage> packages = null;
            using (var semaphore = GetDatabaseSemaphore()) {
                // Wait until file is downloaded/parsed if another download is already in session.
                // Allows user to open multiple npm windows and show progress bar without file-in-use errors.
                bool success = semaphore.WaitOne(10);
                if (!success) {
                    OnOutputLogged(Resources.ErrorCatalogInUse);
                    // Return immediately so that the user can explicitly decide to refresh on failure.
                    return null;
                }

                try {
                    registryUrl = registryUrl ?? await GetRegistryUrl();
                    RegistryFileMapping registryFileMapping = null;
                    using (var db = new SQLiteConnection(DatabaseCacheFilePath)) {
                        registryFileMapping = db.Table<RegistryFileMapping>().FirstOrDefault(info => info.RegistryUrl == registryUrl.ToString());
                    }

                    if (registryFileMapping != null) {
                        string registryFileLocation = Path.Combine(CachePath, registryFileMapping.DbFileLocation, RegistryCacheFilename);

                        var packagesEnumerable = new DatabasePackageCatalogFilter(registryFileLocation).Filter(filterText);
                        packages = await Task.Run(() => packagesEnumerable.ToList());
                    }
                } catch (Exception e) {
                    OnOutputLogged(e.ToString());
                    throw;
                } finally {
                    semaphore.Release();
                }
            }
            return packages;
        }

        public long? ResultsCount { get; private set; }

        public IPackage this[string name] {
            get {
                return Task.Run(async () => await GetCatalogPackagesAsync(name)).Result.FirstOrDefault();
            }
        }
    }
}
