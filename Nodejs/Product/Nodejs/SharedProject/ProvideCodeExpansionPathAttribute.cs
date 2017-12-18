// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    /// This attribute registers an additional path for code snippets to live
    /// in for a particular language.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    internal sealed class ProvideCodeExpansionPathAttribute : RegistrationAttribute
    {
        private readonly string _languageStringId;
        private readonly string _description;
        private readonly string _paths;

        /// <summary>
        /// Creates a new RegisterSnippetsAttribute.
        /// </summary>
        public ProvideCodeExpansionPathAttribute(string languageStringId, string description,
                                          string paths)
        {
            this._languageStringId = languageStringId;
            this._description = description;
            this._paths = paths;
        }

        /// <summary>
        /// Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId => this._languageStringId;
        /// <summary>
        /// Returns the paths to look for snippets.
        /// </summary>
        public string Paths => this._paths;
        /// <summary>
        /// The reg key name of the project.
        /// </summary>
        private string LanguageName()
        {
            return string.Format(CultureInfo.InvariantCulture, "Languages\\CodeExpansions\\{0}", this.LanguageStringId);
        }

        /// <summary>
        /// Called to register this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Register(RegistrationContext context)
        {
            using (var childKey = context.CreateKey(LanguageName()))
            {
                var snippetPaths = context.ComponentPath;
                snippetPaths = System.IO.Path.Combine(snippetPaths, this.Paths);
                snippetPaths = context.EscapePath(System.IO.Path.GetFullPath(snippetPaths));

                using (var pathsSubKey = childKey.CreateSubkey("Paths"))
                {
                    pathsSubKey.SetValue(this._description, snippetPaths);
                }
            }
        }

        /// <summary>
        /// Called to unregister this attribute with the given context.
        /// </summary>
        /// <param name="context">
        /// Contains the location where the registration information should be placed.
        /// It also contains other informations as the type being registered and path information.
        /// </param>
        public override void Unregister(RegistrationContext context)
        {
        }
    }
}
