// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Microsoft.NodejsTools.Profiling
{
    public sealed class CompareReportsView : INotifyPropertyChanged
    {
        private string _baselineFile;
        private string _comparisonFile;
        private bool _isValid;

        /// <summary>
        /// Create a CompareReportsView with default values.
        /// </summary>
        public CompareReportsView()
        {
            _baselineFile = null;
            _comparisonFile = null;
            _isValid = false;

            PropertyChanged += CompareReportsView_PropertyChanged;
        }

        /// <summary>
        /// Create a CompareReportsView with a specified baseline file.
        /// </summary>
        /// <param name="baselineFile"></param>
        public CompareReportsView(string baselineFile)
        : this()
        {
            BaselineFile = baselineFile;
        }

        /// <summary>
        /// Returns a vsp:// comparison URI if the settings are valid; otherwise, null.
        /// </summary>
        /// <returns></returns>
        public string GetComparisonUri()
        {
            if (IsValid)
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "vsp://diff/?baseline={0}&comparison={1}",
                    Uri.EscapeDataString(BaselineFile),
                    Uri.EscapeDataString(ComparisonFile)
                );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// The file filter for performance files.
        /// </summary>
        public string PerformanceFileFilter
        {
            get
            {
                return NodejsProfilingPackage.PerformanceFileFilter;
            }
        }

        /// <summary>
        /// The path to the baseline file.
        /// </summary>
        public string BaselineFile
        {
            get
            {
                return _baselineFile;
            }
            set
            {
                if (_baselineFile != value)
                {
                    _baselineFile = value;
                    OnPropertyChanged(nameof(BaselineFile));
                }
            }
        }

        /// <summary>
        /// The path to the file to compare against.
        /// </summary>
        public string ComparisonFile
        {
            get
            {
                return _comparisonFile;
            }
            set
            {
                if (_comparisonFile != value)
                {
                    _comparisonFile = value;
                    OnPropertyChanged(nameof(ComparisonFile));
                }
            }
        }

        /// <summary>
        /// Receives our own property change events to update IsValid.
        /// </summary>
        private void CompareReportsView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsValid")
            {
                IsValid = File.Exists(BaselineFile) && File.Exists(ComparisonFile);
            }
        }

        /// <summary>
        /// True if both paths are valid; otherwise, false.
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


