// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace TestAdapterTests
{
    internal class MockMessageLogger : IMessageLogger
    {
        public readonly List<Tuple<TestMessageLevel, string>> Messages = new List<Tuple<TestMessageLevel, string>>();

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            this.Messages.Add(new Tuple<TestMessageLevel, string>(testMessageLevel, message));
        }
    }
}

