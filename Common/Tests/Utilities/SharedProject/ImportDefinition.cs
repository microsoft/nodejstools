// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.Build.Construction;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    public class ImportDefinition : ProjectContentGenerator
    {
        public readonly string Project;

        public ImportDefinition(string project)
        {
            Project = project;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            var target = project.Xml.AddImport(Project);
        }
    }
}

