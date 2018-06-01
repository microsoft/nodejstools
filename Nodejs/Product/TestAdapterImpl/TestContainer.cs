// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Extensibility.Model;

namespace Microsoft.NodejsTools.TestAdapter
{
    internal class TestContainer : ITestContainer, IComparable<ITestContainer>
    {
        private readonly DateTime timeStamp;

        public TestContainer(ITestContainerDiscoverer discoverer, string source, DateTime timeStamp)
        {
            this.Discoverer = discoverer;
            this.Source = source;
            this.timeStamp = timeStamp;
        }

        private TestContainer(TestContainer copy)
            : this(copy.Discoverer, copy.Source, copy.timeStamp)
        {
        }

        public int CompareTo(ITestContainer other)
        {
            var container = other as TestContainer;
            if (container == null)
            {
                return -1;
            }

            var result = StringComparer.OrdinalIgnoreCase.Compare(this.Source, container.Source);
            if (result != 0)
            {
                return result;
            }

            return this.timeStamp.CompareTo(container.timeStamp);
        }

        public IEnumerable<Guid> DebugEngines => Array.Empty<Guid>();

        public IDeploymentData DeployAppContainer() => null;

        public ITestContainerDiscoverer Discoverer { get; }

        public bool IsAppContainerTestContainer => false;

        public ITestContainer Snapshot() => new TestContainer(this);

        public string Source { get; }

        public FrameworkVersion TargetFramework => FrameworkVersion.None;

        public Architecture TargetPlatform => Architecture.Default;

        public override string ToString()
        {
            return this.Source + ":" + this.Discoverer.ExecutorUri.ToString();
        }
    }
}
