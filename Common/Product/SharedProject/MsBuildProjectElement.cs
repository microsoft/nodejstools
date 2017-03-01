// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio;
using MSBuild = Microsoft.Build.Evaluation;

namespace Microsoft.VisualStudioTools.Project
{
    internal class MsBuildProjectElement : ProjectElement
    {
        private MSBuild.ProjectItem _item;
        private string _url; // cached Url

        /// <summary>
        /// Constructor to create a new MSBuild.ProjectItem and add it to the project
        /// Only have internal constructors as the only one who should be creating
        /// such object is the project itself (see Project.CreateFileNode()).
        /// </summary>
        internal MsBuildProjectElement(ProjectNode project, string itemPath, string itemType)
            : base(project)
        {
            Utilities.ArgumentNotNullOrEmpty("itemPath", itemPath);
            Utilities.ArgumentNotNullOrEmpty("itemType", itemType);

            // create and add the item to the project

            this._item = project.BuildProject.AddItem(itemType, Microsoft.Build.Evaluation.ProjectCollection.Escape(itemPath))[0];
            this._url = base.Url;
        }

        /// <summary>
        /// Constructor to Wrap an existing MSBuild.ProjectItem
        /// Only have internal constructors as the only one who should be creating
        /// such object is the project itself (see Project.CreateFileNode()).
        /// </summary>
        /// <param name="project">Project that owns this item</param>
        /// <param name="existingItem">an MSBuild.ProjectItem; can be null if virtualFolder is true</param>
        /// <param name="virtualFolder">Is this item virtual (such as reference folder)</param>
        internal MsBuildProjectElement(ProjectNode project, MSBuild.ProjectItem existingItem)
            : base(project)
        {
            Utilities.ArgumentNotNull("existingItem", existingItem);

            // Keep a reference to project and item
            this._item = existingItem;
            this._url = base.Url;
        }

        protected override string ItemType
        {
            get
            {
                return this._item.ItemType;
            }
            set
            {
                this._item.ItemType = value;
                OnItemTypeChanged();
            }
        }

        /// <summary>
        /// Set an attribute on the project element
        /// </summary>
        /// <param name="attributeName">Name of the attribute to set</param>
        /// <param name="attributeValue">Value to give to the attribute</param>
        public override void SetMetadata(string attributeName, string attributeValue)
        {
            Debug.Assert(StringComparer.OrdinalIgnoreCase.Equals(attributeName, ProjectFileConstants.Include), "Use rename as this won't work");

            // Build Action is the type, not a property, so intercept
            if (StringComparer.OrdinalIgnoreCase.Equals(attributeName, ProjectFileConstants.BuildAction))
            {
                this._item.ItemType = attributeValue;
                return;
            }

            // Check out the project file.
            if (!this.ItemProject.QueryEditProjectFile(false))
            {
                throw Marshal.GetExceptionForHR(VSConstants.OLE_E_PROMPTSAVECANCELLED);
            }

            if (attributeValue == null)
            {
                this._item.RemoveMetadata(attributeName);
            }
            else
            {
                this._item.SetMetadataValue(attributeName, attributeValue);
            }
        }

        /// <summary>
        /// Get the value of an attribute on a project element
        /// </summary>
        /// <param name="attributeName">Name of the attribute to get the value for</param>
        /// <returns>Value of the attribute</returns>
        public override string GetMetadata(string attributeName)
        {
            // cannot ask MSBuild for Include, so intercept it and return the corresponding property
            if (StringComparer.OrdinalIgnoreCase.Equals(attributeName, ProjectFileConstants.Include))
            {
                return this._item.EvaluatedInclude;
            }

            // Build Action is the type, not a property, so intercept this one as well
            if (StringComparer.OrdinalIgnoreCase.Equals(attributeName, ProjectFileConstants.BuildAction))
            {
                return this._item.ItemType;
            }

            return this._item.GetMetadataValue(attributeName);
        }

        public override void Rename(string newPath)
        {
            var escapedPath = Microsoft.Build.Evaluation.ProjectCollection.Escape(newPath);

            this._item.Rename(escapedPath);
            this.RefreshProperties();
        }

        public override void RefreshProperties()
        {
            this.ItemProject.BuildProject.ReevaluateIfNecessary();

            this._url = base.Url;

            IEnumerable<ProjectItem> items = this.ItemProject.BuildProject.GetItems(this._item.ItemType);
            foreach (var projectItem in items)
            {
                if (projectItem != null && projectItem.UnevaluatedInclude.Equals(this._item.UnevaluatedInclude))
                {
                    this._item = projectItem;
                    return;
                }
            }
        }

        /// <summary>
        /// Calling this method remove this item from the project file.
        /// Once the item is delete, you should not longer be using it.
        /// Note that the item should be removed from the hierarchy prior to this call.
        /// </summary>
        public override void RemoveFromProjectFile()
        {
            if (!this.Deleted)
            {
                this.ItemProject.BuildProject.RemoveItem(this._item);
            }

            base.RemoveFromProjectFile();
        }

        internal MSBuild.ProjectItem Item => this._item;

        public override string Url => this._url;

        public override bool Equals(object obj)
        {
            // Do they reference the same element?
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var msBuildProjElem = obj as MsBuildProjectElement;
            if (Object.ReferenceEquals(msBuildProjElem, null))
            {
                return false;
            }

            // Do they reference the same project?
            if (!this.ItemProject.Equals(msBuildProjElem.ItemProject))
                return false;

            // Do they have the same include?
            var include1 = GetMetadata(ProjectFileConstants.Include);
            var include2 = msBuildProjElem.GetMetadata(ProjectFileConstants.Include);

            return StringComparer.OrdinalIgnoreCase.Equals(include1, include2);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(GetMetadata(ProjectFileConstants.Include));
        }
    }
}

