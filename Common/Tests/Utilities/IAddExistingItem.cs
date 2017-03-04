// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace TestUtilities
{
    public interface IAddExistingItem : IDisposable
    {
        void OK();
        void Add();
        void AddLink();
        string FileName
        {
            get;
            set;
        }
    }
}

