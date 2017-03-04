// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.IO;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Generates a folder and if not excluded adds it to the generated project.
    /// </summary>
    public sealed class FolderItem : ProjectContentGenerator
    {
        public readonly string Name;
        public readonly bool IsExcluded, IsMissing;

        /// <summary>
        /// Creates a new folder with the specified name.  If the folder
        /// is excluded then it will be created on disk but not added to the
        /// project.
        /// </summary>
        public FolderItem(string name, bool isExcluded = false, bool isMissing = false)
        {
            Name = name;
            IsExcluded = isExcluded;
            IsMissing = isMissing;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            if (!IsMissing)
            {
                Directory.CreateDirectory(Path.Combine(project.DirectoryPath, Name));
            }

            if (!IsExcluded)
            {
                project.AddItem("Folder", Name);
            }
        }
    }
}

