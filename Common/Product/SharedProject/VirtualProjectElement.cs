// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudioTools.Project
{
    internal class VirtualProjectElement : ProjectElement
    {
        private readonly Dictionary<string, string> _virtualProperties;

        /// <summary>
        /// Constructor to Wrap an existing MSBuild.ProjectItem
        /// Only have internal constructors as the only one who should be creating
        /// such object is the project itself (see Project.CreateFileNode()).
        /// </summary>
        /// <param name="project">Project that owns this item</param>
        /// <param name="existingItem">an MSBuild.ProjectItem; can be null if virtualFolder is true</param>
        /// <param name="virtualFolder">Is this item virtual (such as reference folder)</param>
        internal VirtualProjectElement(ProjectNode project)
            : base(project)
        {
            this._virtualProperties = new Dictionary<string, string>();
        }

        protected override string ItemType
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        /// <summary>
        /// Set an attribute on the project element
        /// </summary>
        /// <param name="attributeName">Name of the attribute to set</param>
        /// <param name="attributeValue">Value to give to the attribute</param>
        public override void SetMetadata(string attributeName, string attributeValue)
        {
            Debug.Assert(!StringComparer.OrdinalIgnoreCase.Equals(attributeName, ProjectFileConstants.Include), "Use rename as this won't work");

            // For virtual node, use our virtual property collection
            this._virtualProperties[attributeName] = attributeValue;
        }

        /// <summary>
        /// Get the value of an attribute on a project element
        /// </summary>
        /// <param name="attributeName">Name of the attribute to get the value for</param>
        /// <returns>Value of the attribute</returns>
        public override string GetMetadata(string attributeName)
        {
            // For virtual items, use our virtual property collection
            if (!this._virtualProperties.ContainsKey(attributeName))
            {
                return string.Empty;
            }

            return this._virtualProperties[attributeName];
        }

        public override void Rename(string newPath)
        {
            this._virtualProperties[ProjectFileConstants.Include] = newPath;
        }

        public override bool Equals(object obj)
        {
            return Object.ReferenceEquals(this, obj);
        }

        public override int GetHashCode()
        {
            return RuntimeHelpers.GetHashCode(this);
        }
    }
}
