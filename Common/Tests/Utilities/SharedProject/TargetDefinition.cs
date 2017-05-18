// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.Build.Construction;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    public class TargetDefinition : ProjectContentGenerator
    {
        public readonly string Name;
        public readonly Action<ProjectTargetElement>[] Creators;

        public TargetDefinition(string name, params Action<ProjectTargetElement>[] creators)
        {
            Name = name;
            Creators = creators;
        }

        public string DependsOnTargets { get; set; }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            var target = project.Xml.AddTarget(Name);
            if (!string.IsNullOrEmpty(DependsOnTargets))
            {
                target.DependsOnTargets = DependsOnTargets;
            }
            foreach (var creator in Creators)
            {
                creator(target);
            }
        }
    }
}

