// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.NodejsTools.Npm
{
    public interface INpmController : INpmLogSource, IDisposable
    {
        event EventHandler StartingRefresh;

        void Refresh();

        event EventHandler FinishedRefresh;

        IRootPackage RootPackage { get; }

        INpmCommander CreateNpmCommander();
    }
}
