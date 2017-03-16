// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NodejsTools.Npm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.NodejsTools.NpmUI
{
    internal sealed class NpmWorker : IDisposable
    {
        private static readonly Uri defaultRegistryUri = new Uri("https://registry.npmjs.org/");

        private readonly INpmController npmController;
        private readonly Queue<QueuedNpmCommandInfo> commandQueue = new Queue<QueuedNpmCommandInfo>();
        private readonly object queuelock = new object();

        private bool isDisposed;
        private bool isExecutingCommand;
        private readonly Thread worker;
        private QueuedNpmCommandInfo currentCommand;
        private INpmCommander commander;

        public NpmWorker(INpmController controller)
        {
            this.npmController = controller;

            this.worker = new Thread(this.Run)
            {
                Name = "npm worker Execution",
                IsBackground = true
            };
            this.worker.Start();
        }

        private void Pulse()
        {
            lock (this.queuelock)
            {
                Monitor.PulseAll(this.queuelock);
            }
        }

        public bool IsExecutingCommand
        {
            get
            {
                lock (this.queuelock)
                {
                    return this.isExecutingCommand;
                }
            }
            set
            {
                lock (this.queuelock)
                {
                    this.isExecutingCommand = value;
                    Pulse();
                }
            }
        }

        private void QueueCommand(QueuedNpmCommandInfo info)
        {
            lock (this.queuelock)
            {
                if (this.commandQueue.Contains(info)
                    || info.Equals(this.currentCommand))
                {
                    return;
                }
                this.commandQueue.Enqueue(info);
                Monitor.PulseAll(this.queuelock);
            }
        }

        public void QueueCommand(string arguments)
        {
            QueueCommand(new QueuedNpmCommandInfo(arguments));
        }

        private async void Execute(QueuedNpmCommandInfo info)
        {
            this.IsExecutingCommand = true;
            INpmCommander cmdr = null;
            try
            {
                lock (this.queuelock)
                {
                    cmdr = this.npmController.CreateNpmCommander();

                    this.commander = cmdr;
                }

                await cmdr.ExecuteNpmCommandAsync(info.Arguments);
            }
            finally
            {
                lock (this.queuelock)
                {
                    this.commander = null;
                }
                this.IsExecutingCommand = false;
            }
        }

        private void Run()
        {
            var count = 0;
            // We want the thread to continue running queued commands before
            // exiting so the user can close the install window without having to wait
            // for commands to complete.
            while (!this.isDisposed || count > 0)
            {
                lock (this.queuelock)
                {
                    while ((this.commandQueue.Count == 0 && !this.isDisposed)
                        || this.npmController == null
                        || this.IsExecutingCommand)
                    {
                        Monitor.Wait(this.queuelock);
                    }

                    if (this.commandQueue.Count > 0)
                    {
                        this.currentCommand = this.commandQueue.Dequeue();
                    }
                    else
                    {
                        this.currentCommand = null;
                    }
                    count = this.commandQueue.Count;
                }

                if (null != this.currentCommand)
                {
                    Execute(this.currentCommand);
                }
            }
        }

        public async Task<IEnumerable<IPackage>> GetCatalogPackagesAsync(string filterText)
        {
            var relativeUri = string.Format("/-/v1/search?text={0}", filterText);
            var searchUri = new Uri(defaultRegistryUri, relativeUri);

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
            this.isDisposed = true;
            Pulse();
        }

        private sealed class QueuedNpmCommandInfo
        {
            public QueuedNpmCommandInfo(string arguments)
            {
                this.Name = arguments;
            }

            public string Arguments => this.Name;
            public string Name { get; }

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
