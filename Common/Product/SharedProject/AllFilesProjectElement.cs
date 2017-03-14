// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// Represents a project element which lives on disk and is visible when Show All Files
    /// is enabled.
    /// </summary>
    internal sealed class AllFilesProjectElement : VirtualProjectElement
    {
        private string _itemType;

        public AllFilesProjectElement(string path, string itemType, CommonProjectNode project)
            : base(project)
        {
            Rename(path);
        }

        public override bool IsExcluded => true;

        public new CommonProjectNode ItemProject => (CommonProjectNode)base.ItemProject;

        protected override string ItemType
        {
            get
            {
                return this._itemType;
            }
            set
            {
                this._itemType = value;
                OnItemTypeChanged();
            }
        }
    }
}

