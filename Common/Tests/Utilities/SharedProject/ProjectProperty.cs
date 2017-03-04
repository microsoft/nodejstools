// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    public class ProjectProperty : ProjectContentGenerator
    {
        public readonly string Name, Value;

        public ProjectProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            project.SetProperty(Name, Value);
        }
    }
}

