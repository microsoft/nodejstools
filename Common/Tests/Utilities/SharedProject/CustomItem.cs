// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using MSBuild = Microsoft.Build.Evaluation;

namespace TestUtilities.SharedProject
{
    /// <summary>
    /// Generates a custom msbuild item .
    /// 
    /// The item is added to the project if not excluded.
    /// </summary>
    public sealed class CustomItem : ProjectContentGenerator
    {
        public readonly string Name, Content, ItemType;
        public readonly bool IsExcluded;
        public readonly bool IsMissing;
        public readonly IEnumerable<KeyValuePair<string, string>> Metadata;

        /// <summary>
        /// Creates a new custom item with the specifed type, name, content, and metadata.
        /// </summary>
        public CustomItem(string itemType, string name, string content = null, bool isExcluded = false, bool isMissing = false, IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            ItemType = itemType;
            Name = name;
            IsExcluded = isExcluded;
            IsMissing = isMissing;
            Content = content;
            Metadata = metadata;
        }

        public override void Generate(ProjectType projectType, MSBuild.Project project)
        {
            var filename = Path.Combine(project.DirectoryPath, Name);
            if (!IsMissing)
            {
                File.WriteAllText(filename, Content);
            }

            if (!IsExcluded)
            {
                project.AddItem(
                    ItemType,
                    Name,
                    Metadata
                );
            }
        }
    }
}

