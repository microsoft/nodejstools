// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools.Profiling
{
    /// <summary>
    /// Provides a view model for the ProfilingTarget class.
    /// </summary>
    public sealed class ProfilingTargetView : INotifyPropertyChanged
    {
        private ReadOnlyCollection<ProjectTargetView> _availableProjects;

        private ProjectTargetView _project;
        private bool _isProjectSelected, _isStandaloneSelected;
        private StandaloneTargetView _standalone;
        private readonly string _startText;

        private bool _isValid;

        /// <summary>
        /// Create a ProfilingTargetView with default values.
        /// </summary>
        public ProfilingTargetView()
        {
            var solution = NodejsProfilingPackage.Instance.Solution;

            var availableProjects = new List<ProjectTargetView>();
            foreach (var project in solution.EnumerateLoadedProjects(onlyNodeProjects: true))
            {
                availableProjects.Add(new ProjectTargetView((IVsHierarchy)project));
            }
            _availableProjects = new ReadOnlyCollection<ProjectTargetView>(availableProjects);

            _project = null;
            _standalone = new StandaloneTargetView();
            _isProjectSelected = true;

            _isValid = false;

            PropertyChanged += new PropertyChangedEventHandler(ProfilingTargetView_PropertyChanged);
            _standalone.PropertyChanged += new PropertyChangedEventHandler(Standalone_PropertyChanged);

            var startupProject = NodejsProfilingPackage.Instance.GetStartupProjectGuid();
            Project = AvailableProjects.FirstOrDefault(p => p.Guid == startupProject) ??
                AvailableProjects.FirstOrDefault();
            if (Project != null)
            {
                IsStandaloneSelected = false;
                IsProjectSelected = true;
            }
            else
            {
                IsProjectSelected = false;
                IsStandaloneSelected = true;
            }
            _startText = Resources.ProfilingStart;
        }

        /// <summary>
        /// Create a ProfilingTargetView with values taken from a template.
        /// </summary>
        /// <param name="template"></param>
        public ProfilingTargetView(ProfilingTarget template)
            : this()
        {
            if (template.ProjectTarget != null)
            {
                Project = new ProjectTargetView(template.ProjectTarget);
                IsStandaloneSelected = false;
                IsProjectSelected = true;
            }
            else if (template.StandaloneTarget != null)
            {
                Standalone = new StandaloneTargetView(template.StandaloneTarget);
                IsProjectSelected = false;
                IsStandaloneSelected = true;
            }
            _startText = Resources.ProfilingOk;
        }

        /// <summary>
        /// Returns a ProfilingTarget with the values set from the view model.
        /// </summary>
        public ProfilingTarget GetTarget()
        {
            if (IsValid)
            {
                return new ProfilingTarget
                {
                    ProjectTarget = IsProjectSelected ? Project.GetTarget() : null,
                    StandaloneTarget = IsStandaloneSelected ? Standalone.GetTarget() : null
                };
            }
            else
            {
                return null;
            }
        }

        public ReadOnlyCollection<ProjectTargetView> AvailableProjects
        {
            get
            {
                return _availableProjects;
            }
        }

        /// <summary>
        /// True if AvailableProjects has at least one item.
        /// </summary>
        public bool IsAnyAvailableProjects
        {
            get
            {
                return _availableProjects.Count > 0;
            }
        }

        /// <summary>
        /// A view of the details of the current project.
        /// </summary>
        public ProjectTargetView Project
        {
            get
            {
                return _project;
            }
            set
            {
                if (_project != value)
                {
                    _project = value;
                    OnPropertyChanged(nameof(Project));
                }
            }
        }

        /// <summary>
        /// True if a project is the currently selected target; otherwise, false.
        /// </summary>
        public bool IsProjectSelected
        {
            get
            {
                return _isProjectSelected;
            }
            set
            {
                if (_isProjectSelected != value)
                {
                    _isProjectSelected = value;
                    OnPropertyChanged(nameof(IsProjectSelected));
                }
            }
        }

        /// <summary>
        /// A view of the details of the current standalone script.
        /// </summary>
        public StandaloneTargetView Standalone
        {
            get
            {
                return _standalone;
            }
            set
            {
                if (_standalone != value)
                {
                    if (_standalone != null)
                    {
                        _standalone.PropertyChanged -= Standalone_PropertyChanged;
                    }
                    _standalone = value;
                    if (_standalone != null)
                    {
                        _standalone.PropertyChanged += Standalone_PropertyChanged;
                    }

                    OnPropertyChanged(nameof(Standalone));
                }
            }
        }

        /// <summary>
        /// True if a standalone script is the currently selected target; otherwise, false.
        /// </summary>
        public bool IsStandaloneSelected
        {
            get
            {
                return _isStandaloneSelected;
            }
            set
            {
                if (_isStandaloneSelected != value)
                {
                    _isStandaloneSelected = value;
                    OnPropertyChanged(nameof(IsStandaloneSelected));
                }
            }
        }

        /// <summary>
        /// Receives our own property change events to update IsValid.
        /// </summary>
        private void ProfilingTargetView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(sender == this);

            if (e.PropertyName != "IsValid")
            {
                IsValid = (IsProjectSelected != IsStandaloneSelected) &&
                    (IsProjectSelected ?
                        Project != null :
                        (Standalone != null && Standalone.IsValid));
            }
        }

        /// <summary>
        /// Propagate property change events from Standalone.
        /// </summary>
        private void Standalone_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(Standalone == sender);
            OnPropertyChanged(nameof(Standalone));
        }

        /// <summary>
        /// True if all settings are valid; otherwise, false.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
            private set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        public string StartText
        {
            get
            {
                return _startText;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            var evt = PropertyChanged;
            if (evt != null)
            {
                evt(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raised when the value of a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}


