// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Microsoft.NodejsTools.Telemetry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.NpmUI
{
    internal sealed class NpmWorker : IDisposable
    {
        // todo: get this from a user specified location?
        private static readonly Uri defaultRegistryUri = new Uri("https://registry.npmjs.org/");

        private readonly INpmController npmController;
        private readonly BlockingCollection<QueuedNpmCommandInfo> commandQueue = new BlockingCollection<QueuedNpmCommandInfo>();
        private readonly Thread worker;

        private QueuedNpmCommandInfo currentCommand;

        public NpmWorker(INpmController controller)
        {
            this.npmController = controller;

            this.worker = new Thread(this.Run)
            {
                Name = "NPM worker Execution",
                IsBackground = true
            };
            this.worker.Start();
        }

        private void QueueCommand(QueuedNpmCommandInfo info)
        {
            if (this.commandQueue.IsAddingCompleted
                || this.commandQueue.Contains(info)
                || info.Equals(this.currentCommand))
            {
                return;
            }

            this.commandQueue.Add(info);
        }

        public void QueueCommand(string arguments)
        {
            // this is safe since the we use a blocking collection to
            // store the commands
            this.QueueCommand(new QueuedNpmCommandInfo(arguments));
        }

        private void Execute(QueuedNpmCommandInfo info)
        {
            // Wait on the command to complete.
            // this way we're sure there's only one command being executed,
            // since the only thread starting this commands is the worker thread
            Debug.Assert(Thread.CurrentThread == this.worker, "The worked thread should be executing the NPM commands.");

            var cmdr = this.npmController.CreateNpmCommander();
            try
            {
                cmdr.ExecuteNpmCommandAsync(info.Arguments).Wait();
            }
            catch (AggregateException e) when (e.InnerException is TaskCanceledException)
            {
                // TaskCanceledException is not un-expected, 
                // and should not tear down this thread.
                // Other exceptions are handled higher up the stack.
            }
        }

        private void Run()
        {
            // We want the thread to continue running queued commands before
            // exiting so the user can close the install window without having to wait
            // for commands to complete.
            while (!this.commandQueue.IsCompleted)
            {
                // The Take method will block the worker thread when there are no items left in the queue
                // and the thread will be signalled when new items are items to the queue, or the queue is
                // marked completed.
                if (this.commandQueue.TryTake(out var command, Timeout.Infinite) && command != null)
                {
                    this.currentCommand = command;
                    Execute(this.currentCommand);
                }
            }
        }

        public async Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText)
        {
            if (string.IsNullOrWhiteSpace(filterText))
            {
                return Enumerable.Empty<IPackage>();
            }

            TelemetryHelper.LogSearchNpm();

            string relativeUri;
            if (filterText.Length == 1)
            {
                // Special case since the search API won't retur results for 
                // single chararcter queries.
                relativeUri = $"/{WebUtility.UrlEncode(filterText)}/latest";

                using (var response = await SearchNpmRegistry(relativeUri))
                {
                    /* We expect the following response
                      {
                        "name": "express",
                        "scope": "unscoped",
                        "version": "4.15.2",
                        "description": "Fast, unopinionated, minimalist web framework",
                        "keywords": [ "express", "framework", "sinatra", "web", "rest", "restful", "router", "app", "api" ],
                        "date": "2017-03-06T13:42:44.853Z",
                        "links": {
                          "npm": "https://www.npmjs.com/package/express",
                          "homepage": "http://expressjs.com/",
                          "repository": "https://github.com/expressjs/express",
                          "bugs": "https://github.com/expressjs/express/issues"
                        },
                        "author": {
                          "name": "TJ Holowaychuk",
                          "email": "tj@vision-media.ca"
                        },
                        "publisher": {
                          "username": "dougwilson",
                          "email": "doug@somethingdoug.com"
                        },
                        "maintainers": [
                          {
                            "username": "dougwilson",
                            "email": "doug@somethingdoug.com"
                          }
                        ]
                      }*/
                    var reader = new StreamReader(response.GetResponseStream());
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        while (jsonReader.Read())
                        {
                            switch (jsonReader.TokenType)
                            {
                                case JsonToken.StartObject:
                                    var token = JToken.ReadFrom(jsonReader);
                                    var package = ReadPackage(token, new NodeModuleBuilder());
                                    if (package != null)
                                    {
                                        return new[] { package };
                                    }
                                    break;
                                default:
                                    throw new InvalidOperationException("Unexpected json token.");
                            }
                        }
                    }
                }
            }
            else
            {
                relativeUri = $"/-/v1/search?text={WebUtility.UrlEncode(filterText)}";

                using (var response = await SearchNpmRegistry(relativeUri))
                {
                    /* We expect the following response:
                     {
                          "objects": [
                            {
                              "package": {
                                "name": "express",
                                "scope": "unscoped",
                                "version": "4.15.2",
                                "description": "Fast, unopinionated, minimalist web framework",
                                "keywords": [ "express", "framework", "sinatra", "web", "rest", "restful", "router", "app", "api" ],
                                "date": "2017-03-06T13:42:44.853Z",
                                "links": {
                                  "npm": "https://www.npmjs.com/package/express",
                                  "homepage": "http://expressjs.com/",
                                  "repository": "https://github.com/expressjs/express",
                                  "bugs": "https://github.com/expressjs/express/issues"
                                },
                                "author": {
                                  "name": "TJ Holowaychuk",
                                  "email": "tj@vision-media.ca"
                                },
                                "publisher": {
                                  "username": "dougwilson",
                                  "email": "doug@somethingdoug.com"
                                },
                                "maintainers": [
                                  {
                                    "username": "dougwilson",
                                    "email": "doug@somethingdoug.com"
                                  }
                                ]
                              },
                              "score": {
                                "final": 0.9549640105248649,
                                "detail": {
                                  "quality": 0.9427473299991661,
                                  "popularity": 0.9496544159654299,
                                  "maintenance": 0.9707450455348992
                                }
                              },
                              "searchScore": 100000.95
                            }
                          ],
                          "total": 10991,
                          "time": "Tue May 09 2017 22:41:07 GMT+0000 (UTC)"
                        }
                    */

                    var reader = new StreamReader(response.GetResponseStream());
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        while (jsonReader.Read())
                        {
                            switch (jsonReader.TokenType)
                            {
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
            }

            // should never get here
            throw new InvalidOperationException("Unexpected json token.");
        }

        private static Task<WebResponse> SearchNpmRegistry(string relativeUri)
        {
            var searchUri = new Uri(defaultRegistryUri, relativeUri);

            var request = WebRequest.Create(searchUri);
            return request.GetResponseAsync();
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
                        if (StringComparer.Ordinal.Equals(jsonReader.Value, "package"))
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

                var date = package["date"];
                if (date != null)
                {
                    builder.SetDate((string)date);
                }

                var version = package["version"];
                if (version != null)
                {
                    var semver = SemverVersion.Parse((string)version);
                    builder.AddVersion(semver);
                }

                AddKeywords(builder, package["keywords"]);
                AddAuthor(builder, package["author"]);
                AddHomepage(builder, package["links"]);

                return builder.Build();
            }
            catch (InvalidOperationException)
            {
                // Occurs if a JValue appears where we expect JProperty
                return null;
            }
            catch (ArgumentException)
            {
                return null;
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

        private static void AddHomepage(NodeModuleBuilder builder, JToken links)
        {
            var homepage = links?["homepage"];
            if (homepage != null)
            {
                builder.AddHomepage((string)homepage);
            }
        }

        private static void AddAuthor(NodeModuleBuilder builder, JToken author)
        {
            var name = author?["name"];
            if (author != null)
            {
                builder.AddAuthor((string)name);
            }
        }

        public void Dispose()
        {
            this.commandQueue.CompleteAdding();
        }

        private sealed class QueuedNpmCommandInfo
        {
            public QueuedNpmCommandInfo(string arguments)
            {
                this.Arguments = arguments;
            }

            public string Arguments { get; }

            public bool Equals(QueuedNpmCommandInfo other)
            {
                return StringComparer.CurrentCulture.Equals(this.ToString(), other?.ToString());
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as QueuedNpmCommandInfo);
            }

            public override int GetHashCode()
            {
                return this.ToString().GetHashCode();
            }

            public override string ToString()
            {
                var buff = new StringBuilder("npm ");
                buff.Append(this.Arguments);

                return buff.ToString();
            }
        }
    }
}
