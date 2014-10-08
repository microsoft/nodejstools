// codesettings.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.NodejsTools.Parsing
{
    /// <summary>
    /// Enum describing the type of input expected
    /// </summary>
    public enum JavaScriptSourceMode
    {
        /// <summary>Default input mode: a program, a block of top-level global statements</summary>
        Program = 0,

        /// <summary>Input is a single JavaScript Expression</summary>
        Expression,
    }

    /// <summary>
    /// Object used to store code settings for JavaScript parsing, minification, and output
    /// </summary>
    internal class CodeSettings
    {
        /// <summary>
        /// Instantiate a CodeSettings object with the default settings
        /// </summary>
        public CodeSettings()
        {
            // other fields we want initialized
            this.StrictMode = false;
            
            // no default globals
            this.m_knownGlobals = new HashSet<string>();
        }

        /// <summary>
        /// Instantiate a new CodeSettings object with the same settings as the current object.
        /// </summary>
        /// <returns>a copy CodeSettings object</returns>
        public CodeSettings Clone()
        {
            // create a new settings object and set all the properties using this settings object
            var newSettings = new CodeSettings()
            {
                ConstStatementsMozilla = this.ConstStatementsMozilla,
                KnownGlobalNamesList = this.KnownGlobalNamesList,
                SourceMode = this.SourceMode,
                StrictMode = this.StrictMode,
                AllowShebangLine = this.AllowShebangLine
            };

            return newSettings;
        }

        /// <summary>
        /// Allow #! on the first line
        /// </summary>
        public bool AllowShebangLine {
            get;
            set;
        }

        #region known globals

        private HashSet<string> m_knownGlobals;        

        /// <summary>
        /// sets the collection of known global names to the array of string passed to this method
        /// </summary>
        /// <param name="globalArray">array of known global names</param>
        [Obsolete("This property is deprecated; use SetKnownGlobalIdentifiers instead")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public int SetKnownGlobalNames(params string[] globalArray)
        {
            return SetKnownGlobalIdentifiers(globalArray);
        }

        /// <summary>
        /// Gets the known global name collection
        /// </summary>
        public IEnumerable<string> KnownGlobalCollection { get { return m_knownGlobals; } }

        /// <summary>
        /// sets the collection of known global names to the array of string passed to this method
        /// </summary>
        /// <param name="globalArray">collection of known global names</param>
        public int SetKnownGlobalIdentifiers(IEnumerable<string> globalArray)
        {
            m_knownGlobals.Clear();
            if (globalArray != null)
            {
                foreach (var name in globalArray)
                {
                    AddKnownGlobal(name);
                }
            }

            return m_knownGlobals.Count;
        }

        /// <summary>
        /// Add a known global identifier to the list
        /// </summary>
        /// <param name="identifier">global identifier</param>
        /// <returns>true if valid identifier; false if invalid identifier</returns>
        public bool AddKnownGlobal(string identifier)
        {
            if (JSScanner.IsValidIdentifier(identifier))
            {
                m_knownGlobals.Add(identifier);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the known global names list as a single comma-separated string
        /// </summary>
        public string KnownGlobalNamesList
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var knownGlobal in m_knownGlobals)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(',');
                    }
                    sb.Append(knownGlobal);
                }

                return sb.ToString();
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var knownGlobal in value.Split(',', ';'))
                    {
                        AddKnownGlobal(knownGlobal);
                    }
                }
                else
                {
                    m_knownGlobals.Clear();
                }
            }
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets or sets a boolean value indicating whether to use old-style const statements (just var-statements that
        /// define unchangeable fields) or new EcmaScript 6 lexical declarations.
        /// </summary>
        public bool ConstStatementsMozilla
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the source mode
        /// </summary>
        public JavaScriptSourceMode SourceMode
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets a boolean value indicating whether or not to force the input code into strict mode (true)
        /// or rely on the sources to turn on strict mode via the "use strict" prologue directive (false, default).
        /// </summary>
        public bool StrictMode
        {
            get;
            set;
        }

        #endregion
    }
}
