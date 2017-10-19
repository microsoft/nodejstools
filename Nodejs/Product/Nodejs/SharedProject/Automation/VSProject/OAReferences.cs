// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents the automation object for the equivalent ReferenceContainerNode object
    /// </summary>
    [ComVisible(true)]
    public class OAReferences : ConnectionPointContainer,
                                IEventSource<_dispReferencesEvents>,
                                References,
                                ReferencesEvents
    {
        private readonly ProjectNode _project;
        private ReferenceContainerNode _container;

        /// <summary>
        /// Creates a new automation references object.  If the project type doesn't
        /// support references containerNode is null.
        /// </summary>
        /// <param name="containerNode"></param>
        /// <param name="project"></param>
        internal OAReferences(ReferenceContainerNode containerNode, ProjectNode project)
        {
            this._container = containerNode;
            this._project = project;

            AddEventSource<_dispReferencesEvents>(this as IEventSource<_dispReferencesEvents>);
            if (this._container != null)
            {
                this._container.OnChildAdded += new EventHandler<HierarchyNodeEventArgs>(this.OnReferenceAdded);
                this._container.OnChildRemoved += new EventHandler<HierarchyNodeEventArgs>(this.OnReferenceRemoved);
            }
        }

        #region Private Members
        private Reference AddFromSelectorData(VSCOMPONENTSELECTORDATA selector)
        {
            if (this._container == null)
            {
                return null;
            }
            ReferenceNode refNode = this._container.AddReferenceFromSelectorData(selector);
            if (null == refNode)
            {
                return null;
            }

            return refNode.Object as Reference;
        }

        private Reference FindByName(string stringIndex)
        {
            foreach (Reference refNode in this)
            {
                if (StringComparer.Ordinal.Equals(refNode.Name, stringIndex))
                {
                    return refNode;
                }
            }
            return null;
        }
        #endregion

        #region References Members

        public Reference Add(string bstrPath)
        {
            // ignore requests from the designer which are framework assemblies and start w/ a *.
            if (string.IsNullOrEmpty(bstrPath) || bstrPath.StartsWith("*"))
            {
                return null;
            }
            VSCOMPONENTSELECTORDATA selector = new VSCOMPONENTSELECTORDATA();
            selector.type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_File;
            selector.bstrFile = bstrPath;

            return AddFromSelectorData(selector);
        }

        public Reference AddActiveX(string bstrTypeLibGuid, int lMajorVer, int lMinorVer, int lLocaleId, string bstrWrapperTool)
        {
            VSCOMPONENTSELECTORDATA selector = new VSCOMPONENTSELECTORDATA();
            selector.type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_Com2;
            selector.guidTypeLibrary = new Guid(bstrTypeLibGuid);
            selector.lcidTypeLibrary = (uint)lLocaleId;
            selector.wTypeLibraryMajorVersion = (ushort)lMajorVer;
            selector.wTypeLibraryMinorVersion = (ushort)lMinorVer;

            return AddFromSelectorData(selector);
        }

        public Reference AddProject(EnvDTE.Project project)
        {
            if (null == project || this._container == null)
            {
                return null;
            }
            // Get the soulution.
            IVsSolution solution = this._container.ProjectMgr.Site.GetService(typeof(SVsSolution)) as IVsSolution;
            if (null == solution)
            {
                return null;
            }

            // Get the hierarchy for this project.
            ErrorHandler.ThrowOnFailure(solution.GetProjectOfUniqueName(project.UniqueName, out var projectHierarchy));

            // Create the selector data.
            VSCOMPONENTSELECTORDATA selector = new VSCOMPONENTSELECTORDATA();
            selector.type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_Project;

            // Get the project reference string.
            ErrorHandler.ThrowOnFailure(solution.GetProjrefOfProject(projectHierarchy, out selector.bstrProjRef));

            selector.bstrTitle = project.Name;
            selector.bstrFile = System.IO.Path.GetDirectoryName(project.FullName);

            return AddFromSelectorData(selector);
        }

        public EnvDTE.Project ContainingProject
        {
            get
            {
                return this._project.GetAutomationObject() as EnvDTE.Project;
            }
        }

        public int Count
        {
            get
            {
                if (this._container == null)
                {
                    return 0;
                }
                return this._container.EnumReferences().Count;
            }
        }

        public EnvDTE.DTE DTE
        {
            get
            {
                return this._project.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            }
        }

        public Reference Find(string bstrIdentity)
        {
            if (string.IsNullOrEmpty(bstrIdentity))
            {
                return null;
            }
            foreach (Reference refNode in this)
            {
                if (null != refNode)
                {
                    if (StringComparer.Ordinal.Equals(bstrIdentity, refNode.Identity))
                    {
                        return refNode;
                    }
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            if (this._container == null)
            {
                return new List<Reference>().GetEnumerator();
            }

            List<Reference> references = new List<Reference>();
            IEnumerator baseEnum = this._container.EnumReferences().GetEnumerator();
            if (null == baseEnum)
            {
                return references.GetEnumerator();
            }
            while (baseEnum.MoveNext())
            {
                ReferenceNode refNode = baseEnum.Current as ReferenceNode;
                if (null == refNode)
                {
                    continue;
                }
                Reference reference = refNode.Object as Reference;
                if (null != reference)
                {
                    references.Add(reference);
                }
            }
            return references.GetEnumerator();
        }

        public Reference Item(object index)
        {
            if (this._container == null)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            string stringIndex = index as string;
            if (null != stringIndex)
            {
                return FindByName(stringIndex);
            }
            // Note that this cast will throw if the index is not convertible to int.
            int intIndex = (int)index;
            IList<ReferenceNode> refs = this._container.EnumReferences();
            if (null == refs)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if ((intIndex <= 0) || (intIndex > refs.Count))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            // Let the implementation of IList<> throw in case of index not correct.
            return refs[intIndex - 1].Object as Reference;
        }

        public object Parent
        {
            get
            {
                if (this._container == null)
                {
                    return this._project.Object;
                }
                return this._container.Parent.Object;
            }
        }

        #endregion

        #region _dispReferencesEvents_Event Members
        public event _dispReferencesEvents_ReferenceAddedEventHandler ReferenceAdded;
        public event _dispReferencesEvents_ReferenceChangedEventHandler ReferenceChanged
        {
            add { }
            remove { }
        }
        public event _dispReferencesEvents_ReferenceRemovedEventHandler ReferenceRemoved;
        #endregion

        #region Callbacks for the HierarchyNode events
        private void OnReferenceAdded(object sender, HierarchyNodeEventArgs args)
        {
            // Validate the parameters.
            if ((this._container != sender as ReferenceContainerNode) ||
                (null == args) || (null == args.Child))
            {
                return;
            }

            // Check if there is any sink for this event.
            if (null == ReferenceAdded)
            {
                return;
            }

            // Check that the removed item implements the Reference interface.
            Reference reference = args.Child.Object as Reference;
            if (null != reference)
            {
                ReferenceAdded(reference);
            }
        }

        private void OnReferenceRemoved(object sender, HierarchyNodeEventArgs args)
        {
            // Validate the parameters.
            if ((this._container != sender as ReferenceContainerNode) ||
                (null == args) || (null == args.Child))
            {
                return;
            }

            // Check if there is any sink for this event.
            if (null == ReferenceRemoved)
            {
                return;
            }

            // Check that the removed item implements the Reference interface.
            Reference reference = args.Child.Object as Reference;
            if (null != reference)
            {
                ReferenceRemoved(reference);
            }
        }
        #endregion

        #region IEventSource<_dispReferencesEvents> Members
        void IEventSource<_dispReferencesEvents>.OnSinkAdded(_dispReferencesEvents sink)
        {
            ReferenceAdded += new _dispReferencesEvents_ReferenceAddedEventHandler(sink.ReferenceAdded);
            ReferenceChanged += new _dispReferencesEvents_ReferenceChangedEventHandler(sink.ReferenceChanged);
            ReferenceRemoved += new _dispReferencesEvents_ReferenceRemovedEventHandler(sink.ReferenceRemoved);
        }

        void IEventSource<_dispReferencesEvents>.OnSinkRemoved(_dispReferencesEvents sink)
        {
            ReferenceAdded -= new _dispReferencesEvents_ReferenceAddedEventHandler(sink.ReferenceAdded);
            ReferenceChanged -= new _dispReferencesEvents_ReferenceChangedEventHandler(sink.ReferenceChanged);
            ReferenceRemoved -= new _dispReferencesEvents_ReferenceRemovedEventHandler(sink.ReferenceRemoved);
        }
        #endregion
    }
}
