// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using TestUtilities.SharedProject;

namespace Microsoft.Nodejs.Tests.UI
{
    public class NodejsProjectTest : SharedProjectTest
    {
        public static ProjectType NodejsProject = ProjectTypes.First(x => x.ProjectExtension == ".njsproj");

        public static ProjectDefinition Project(string name, params ProjectContentGenerator[] items)
        {
            return new ProjectDefinition(name, NodejsProject, items);
        }
    }
}

