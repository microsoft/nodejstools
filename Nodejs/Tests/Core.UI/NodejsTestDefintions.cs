// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using TestUtilities.SharedProject;

namespace Microsoft.Nodejs.Tests.UI
{
    public sealed class NodejsTestDefintions
    {
        [Export]
        [ProjectExtension(".njsproj")]
        [ProjectTypeGuid("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")]
        [CodeExtension(".js")]
        [SampleCode("console.log('hi');")]
        internal static ProjectTypeDefinition ProjectTypeDefinition = new ProjectTypeDefinition();
    }
}

