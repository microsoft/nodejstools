// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using Microsoft.VisualStudio.TestWindow.Extensibility.Model;

namespace Microsoft.VisualStudioTools.TestAdapter
{
    internal class TestContainer : ITestContainer, IComparable<ITestContainer>
    {
        private readonly DateTime timeStamp;

        public TestContainer(ITestContainerDiscoverer discoverer, string source, DateTime timeStamp, Architecture architecture)
        {
            this.Discoverer = discoverer;
            this.Source = source;
            this.TargetPlatform = architecture;
            this.timeStamp = timeStamp;
        }

        private TestContainer(TestContainer copy)
            : this(copy.Discoverer, copy.Source, copy.timeStamp, copy.TargetPlatform)
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

        public IEnumerable<Guid> DebugEngines
        {
            get
            {
                // TODO: Create a debug engine that can be used to attach to the (managed) test executor
                // Mixed mode debugging is not strictly necessary, provided that
                // the first engine returned from this method can attach to a
                // managed executable. This may change in future versions of the
                // test framework, in which case we may be able to start
                // returning our own debugger and having it launch properly.
                yield break;
            }
        }

        public IDeploymentData DeployAppContainer()
        {
            return null;
        }

        public ITestContainerDiscoverer Discoverer { get; }

        public bool IsAppContainerTestContainer => false;

        public ITestContainer Snapshot()
        {
            return new TestContainer(this);
        }

        public string Source { get; }

        public FrameworkVersion TargetFramework=> FrameworkVersion.None;

        public Architecture TargetPlatform { get; }

        public override string ToString()
        {
            return this.Source + ":" + this.Discoverer.ExecutorUri.ToString();
        }
    }
}
