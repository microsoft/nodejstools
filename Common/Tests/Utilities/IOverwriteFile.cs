// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace TestUtilities
{
    public interface IOverwriteFile : IDisposable
    {
        string Text { get; }

        void No();

        bool AllItems { get; set; }

        void Yes();

        void Cancel();
    }
}

