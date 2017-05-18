// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace TestUtilities
{
    public interface IWarningLogger
    {
        void Log(string message, params object[] arguments);
    }
}

