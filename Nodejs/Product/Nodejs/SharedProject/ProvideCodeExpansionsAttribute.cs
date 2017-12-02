// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudioTools
{
    /// <summary>
    /// This attribute registers code snippets for a package.  The attributes on a 
    /// package do not control the behavior of the package, but they can be used by registration 
    /// tools to register the proper information with Visual Studio.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    // Disable the "IdentifiersShouldNotHaveIncorrectSuffix" warning.
    internal sealed class ProvideCodeExpansionsAttribute : RegistrationAttribute
    {
        private readonly Guid _languageGuid;
        private readonly bool _showRoots;
        private readonly short _displayName;
        private readonly string _languageStringId;
        private readonly string _indexPath;
        private readonly string _paths;

        /// <summary>
        /// Creates a new RegisterSnippetsAttribute.
        /// </summary>
        public ProvideCodeExpansionsAttribute(string languageGuid, bool showRoots, short displayName,
                                          string languageStringId, string indexPath,
                                          string paths)
        {
            this._languageGuid = new Guid(languageGuid);
            this._showRoots = showRoots;
            this._displayName = displayName;
            this._languageStringId = languageStringId;
            this._indexPath = indexPath;
            this._paths = paths;
        }

        /// <summary>
        /// Returns the language guid.
        /// </summary>
        public string LanguageGuid => this._languageGuid.ToString("B");
        /// <summary>
        /// Returns true if roots are shown.
        /// </summary>
        public bool ShowRoots => this._showRoots;
        /// <summary>
        /// Returns string ID corresponding to the language name.
        /// </summary>
        public short DisplayName => this._displayName;
        /// <summary>
        /// Returns the string to use for the language name.
        /// </summary>
        public string LanguageStringId => this._languageStringId;
        /// <summary>
        /// Returns the relative path to the snippet index file.
        /// </summary>
        public string IndexPath => this._indexPath;
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
            if (context == null)
            {
                return;
            }
            using (var childKey = context.CreateKey(LanguageName()))
            {
                childKey.SetValue("", this.LanguageGuid);

                var snippetIndexPath = context.ComponentPath;
                snippetIndexPath = System.IO.Path.Combine(snippetIndexPath, this.IndexPath);
                snippetIndexPath = context.EscapePath(System.IO.Path.GetFullPath(snippetIndexPath));

                childKey.SetValue("DisplayName", this.DisplayName.ToString(CultureInfo.InvariantCulture));
                childKey.SetValue("IndexPath", snippetIndexPath);
                childKey.SetValue("LangStringId", this.LanguageStringId.ToLowerInvariant());
                childKey.SetValue("Package", context.ComponentType.GUID.ToString("B"));
                childKey.SetValue("ShowRoots", this.ShowRoots ? 1 : 0);

                var snippetPaths = context.ComponentPath;
                snippetPaths = System.IO.Path.Combine(snippetPaths, this.Paths);
                snippetPaths = context.EscapePath(System.IO.Path.GetFullPath(snippetPaths));

                //The following enables VS to look into a user directory for more user-created snippets
                var myDocumentsPath = @";%MyDocs%\Code Snippets\" + this._languageStringId + @"\My Code Snippets\";
                using (var forceSubKey = childKey.CreateSubkey("ForceCreateDirs"))
                {
                    forceSubKey.SetValue(this.LanguageStringId, snippetPaths + myDocumentsPath);
                }

                using (var pathsSubKey = childKey.CreateSubkey("Paths"))
                {
                    pathsSubKey.SetValue(this.LanguageStringId, snippetPaths + myDocumentsPath);
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
            if (context != null)
            {
                context.RemoveKey(LanguageName());
            }
        }
    }
}
