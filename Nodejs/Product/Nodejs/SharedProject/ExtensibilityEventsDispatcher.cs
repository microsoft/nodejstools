// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudioTools.Project
{
    /// <summary>
    /// This is a helper class which fires IVsExtensibility3 events if not in suspended state.
    /// </summary>
    internal sealed class ExtensibilityEventsDispatcher
    {
        private class SuspendLock : IDisposable
        {
            private readonly bool _previousState;
            private readonly ExtensibilityEventsDispatcher _owner;

            public SuspendLock(ExtensibilityEventsDispatcher owner)
            {
                this._owner = owner;
                this._previousState = this._owner._suspended;
                this._owner._suspended = true;
            }

            void IDisposable.Dispose()
            {
                this._owner._suspended = this._previousState;
            }
        }

        private readonly ProjectNode _project;
        private bool _suspended;

        public ExtensibilityEventsDispatcher(ProjectNode project)
        {
            Utilities.ArgumentNotNull("project", project);

            this._project = project;
        }

        /// <summary>
        /// Creates a lock which suspends firing of the events until it gets disposed.
        /// </summary>
        public IDisposable Suspend()
        {
            return new SuspendLock(this);
        }

        public void FireItemAdded(HierarchyNode node)
        {
            this.Fire(node, (IVsExtensibility3 vsExtensibility, ProjectItem item) =>
            {
                vsExtensibility.FireProjectItemsEvent_ItemAdded(item);
            });
        }

        public void FireItemRemoved(HierarchyNode node)
        {
            this.Fire(node, (IVsExtensibility3 vsExtensibility, ProjectItem item) =>
            {
                vsExtensibility.FireProjectItemsEvent_ItemRemoved(item);
            });
        }

        public void FireItemRenamed(HierarchyNode node, string oldName)
        {
            this.Fire(node, (IVsExtensibility3 vsExtensibility, ProjectItem item) =>
            {
                vsExtensibility.FireProjectItemsEvent_ItemRenamed(item, oldName);
            });
        }

        private void Fire(HierarchyNode node, Action<IVsExtensibility3, ProjectItem> fire)
        {
            // When we are in suspended mode. Do not fire anything
            if (this._suspended)
            {
                return;
            }

            // Project has to be opened
            if (!this._project.IsProjectOpened)
            {
                return;
            }

            // We don't want to fire events for references here. OAReferences should do the job
            if (node is ReferenceNode)
            {
                return;
            }

            var vsExtensibility = this._project.GetService(typeof(IVsExtensibility)) as IVsExtensibility3;
            if (vsExtensibility != null)
            {
                var obj = node.GetAutomationObject();
                var item = obj as ProjectItem;
                if (item != null)
                {
                    fire(vsExtensibility, item);
                }
            }
        }
    }
}
