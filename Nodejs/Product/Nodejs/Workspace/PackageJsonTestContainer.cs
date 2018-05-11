// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Extensibility.Model;

namespace Microsoft.NodejsTools.Workspace
{
    public sealed class PackageJsonTestContainer : ITestContainer
    {
        private int version;

        public PackageJsonTestContainer(ITestContainerDiscoverer discoverer, string source, string testRoot)
        {
            this.Discoverer = discoverer;
            this.Source = source;
            this.TestRoot = testRoot;
        }

        private PackageJsonTestContainer(PackageJsonTestContainer other) :
            this(other.Discoverer, other.Source, other.TestRoot)
        {
            this.version = other.version;
        }

        public ITestContainerDiscoverer Discoverer { get; }

        public string Source { get; }

        public string TestRoot { get; }

        public IEnumerable<Guid> DebugEngines { get { yield break; } }

        public FrameworkVersion TargetFramework { get; } = FrameworkVersion.None;

        public Architecture TargetPlatform { get; } = Architecture.X86;

        public bool IsAppContainerTestContainer { get; } = false;

        public int CompareTo(ITestContainer other)
        {
            if (other == null || !(other is PackageJsonTestContainer testContainer))
            {
                return -1;
            }

            if (this.version != testContainer.version)
            {
                return this.version - testContainer.version;
            }

            var sourceCompare = StringComparer.OrdinalIgnoreCase.Compare(this.Source, testContainer.Source);

            return sourceCompare == 0 ? sourceCompare : StringComparer.OrdinalIgnoreCase.Compare(this.TestRoot, testContainer.TestRoot);
        }

        public IDeploymentData DeployAppContainer() => null;

        public ITestContainer Snapshot() => new PackageJsonTestContainer(this);

        public bool IsContained(string javaScriptFilePath)
        {
            Debug.Assert(Path.IsPathRooted(javaScriptFilePath) && !string.IsNullOrEmpty(javaScriptFilePath), "Expected a rooted path.");

            return javaScriptFilePath.StartsWith(this.TestRoot, StringComparison.OrdinalIgnoreCase);
        }

        public void IncreaseVersion()
        {
            Interlocked.Increment(ref this.version);
        }
    }
}
