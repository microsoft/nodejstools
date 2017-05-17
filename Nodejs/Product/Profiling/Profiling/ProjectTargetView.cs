// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Profiling
{
    /// <summary>
    /// Provides a view model for the ProjectTarget class.
    /// </summary>
    public class ProjectTargetView
    {
        private readonly string _name;
        private readonly Guid _guid;

        /// <summary>
        /// Create a ProjectTargetView with values from an EnvDTE.Project.
        /// </summary>
        public ProjectTargetView(IVsHierarchy project)
        {
            object value;
            ErrorHandler.ThrowOnFailure(project.GetProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_Name,
                out value
            ));
            _name = value as string ?? "(Unknown name)";
            ErrorHandler.ThrowOnFailure(project.GetGuidProperty(
                (uint)VSConstants.VSITEMID.Root,
                (int)__VSHPROPID.VSHPROPID_ProjectIDGuid,
                out _guid
            ));
        }

        /// <summary>
        /// Create a ProjectTargetView with values from a ProjectTarget.
        /// </summary>
        public ProjectTargetView(ProjectTarget project)
        {
            _name = project.FriendlyName;
            _guid = project.TargetProject;
        }

        /// <summary>
        /// Returns a ProjectTarget created with the values from the view model.
        /// </summary>
        public ProjectTarget GetTarget()
        {
            return new ProjectTarget
            {
                FriendlyName = _name,
                TargetProject = _guid
            };
        }

        /// <summary>
        /// The display name of the project.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// The Guid identifying the project.
        /// </summary>
        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ProjectTargetView;
            if (other == null)
            {
                return false;
            }
            else
            {
                return Guid.Equals(other.Guid);
            }
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}



