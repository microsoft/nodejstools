// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Groups a set of ProjectContentGenerator together.
    /// 
    /// This class exists solely to allow a hierarchy to be written in
    /// source code when describing the test projects.
    /// 
    /// It takes a list of ProjectContentGenerator, and when asked to
    /// generate will generate the list in order.
    /// </summary>
    public class ProjectContentGroup : ProjectContentGenerator
    {
        private readonly ProjectContentGenerator[] _content;

        public ProjectContentGroup(ProjectContentGenerator[] content)
        {
            _content = content;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            foreach (var content in _content)
            {
                content.Generate(projectType, project);
            }
        }
    }
}

