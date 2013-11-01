/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject {
    /// <summary>
    /// Class used to define a project.  A project consists of a type, a name, 
    /// the items in the project (which will be generated at test time) as well as
    /// MSBuild project properties.
    /// </summary>
    public sealed class ProjectDefinition {
        public readonly ProjectType ProjectType;
        public readonly string Name;
        public readonly ProjectContentGenerator[] Items;
        public readonly bool IsUserProject;

        /// <summary>
        /// Creates a new project definition which can be included in a solution or generated.
        /// </summary>
        /// <param name="name">The name of the project</param>
        /// <param name="projectType">The project type which controls the language being tested</param>
        /// <param name="items">The items included in the project</param>
        public ProjectDefinition(string name, ProjectType projectType, params ProjectContentGenerator[] items) {
            ProjectType = projectType;
            Name = name;
            Items = items;
        }

        public ProjectDefinition(string name, ProjectType projectType, bool isUserProject, params ProjectContentGenerator[] items)
            : this(name, projectType, items) {
            IsUserProject = isUserProject;
        }

        /// <summary>
        /// Helper function which generates the project and solution with just this 
        /// project in the solution.
        /// </summary>
        public SolutionFile Generate() {
            return SolutionFile.Generate(Name, this);
        }

        public MSBuild.Project Save(MSBuild.ProjectCollection collection, string location) {
            location = Path.Combine(location, Name);
            Directory.CreateDirectory(location);

            var project = new MSBuild.Project(collection);
            string projectFile = Path.Combine(location, Name) + ProjectType.ProjectExtension;
            if (IsUserProject) {
                projectFile += ".user";
            }
            project.Save(projectFile);

            var projGuid = Guid.NewGuid();
            project.SetProperty("ProjectTypeGuid", ProjectType.ProjectTypeGuid.ToString());
            project.SetProperty("Name", Name);
            project.SetProperty("ProjectGuid", projGuid.ToString("B"));
            project.SetProperty("SchemaVersion", "2.0");

            foreach (var processor in ProjectType.Processors) {
                processor.PreProcess(project);
            }

            foreach (var item in Items) {
                item.Generate(ProjectType, project);
            }

            foreach (var processor in ProjectType.Processors) {
                processor.PostProcess(project);
            }

            project.Save();

            return project;
        }
    }
}
