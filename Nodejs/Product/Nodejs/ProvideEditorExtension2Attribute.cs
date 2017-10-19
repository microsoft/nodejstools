// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools
{
    /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute"]' />
    /// <devdoc>
    ///     This attribute associates a file extension to a given editor factory.  
    ///     The editor factory may be specified as either a GUID or a type and 
    ///     is placed on a package.
    ///     
    /// This differs from the normal one in that more than one extension can be supplied and
    /// a linked editor GUID can be supplied.
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class ProvideEditorExtension2Attribute : RegistrationAttribute
    {
        private Guid _factory;
        private string _extension;
        private int _priority;
        private Guid _project;
        private string _templateDir;
        private int _resId;
        private int _editorNameResId;
        private bool _editorFactoryNotify;
        private string _editorName;
        private Guid _linkedEditorGuid;
        private readonly string[] _extensions;
        private __VSPHYSICALVIEWATTRIBUTES _commonViewAttrs;

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.ProvideEditorExtensionAttribute"]' />
        /// <devdoc>
        ///     Creates a new attribute.
        /// </devdoc>
        public ProvideEditorExtension2Attribute(object factoryType, string extension, int priority, params string[] extensions)
        {
            // figure out what type of object they passed in and get the GUID from it
            if (factoryType is string)
            {
                this._factory = new Guid((string)factoryType);
            }
            else if (factoryType is Type)
            {
                this._factory = ((Type)factoryType).GUID;
            }
            else if (factoryType is Guid)
            {
                this._factory = (Guid)factoryType;
            }
            else
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "invalid factory type: {0}", factoryType), nameof(factoryType));
            }

            this._extension = extension;
            this._priority = priority;
            this._project = Guid.Empty;
            this._templateDir = "./NullPath";
            this._resId = 0;
            this._editorFactoryNotify = false;
            this._extensions = extensions;
        }

        public ProvideEditorExtension2Attribute(object factoryType, string extension, int priority, __VSPHYSICALVIEWATTRIBUTES commonViewAttributes, params string[] extensions) :
            this(factoryType, extension, priority, extensions)
        {
            this._commonViewAttrs = commonViewAttributes;
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.Extension"]' />
        /// <devdoc>
        ///     The file extension of the file.
        /// </devdoc>
        public string Extension => this._extension;

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.Factory"]' />
        /// <devdoc>
        ///     The editor factory guid.
        /// </devdoc>
        public Guid Factory => this._factory;

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.Priority"]' />
        /// <devdoc>
        ///     The priority of this extension registration.
        /// </devdoc>
        public int Priority => this._priority;

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.ProjectGuid"]/*' />
        public string ProjectGuid
        {
            set { this._project = new System.Guid(value); }
            get { return this._project.ToString(); }
        }

        public string LinkedEditorGuid
        {
            get { return this._linkedEditorGuid.ToString(); }
            set { this._linkedEditorGuid = new System.Guid(value); }
        }

        public __VSPHYSICALVIEWATTRIBUTES CommonPhysicalViewAttributes => this._commonViewAttrs;

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.EditorFactoryNotify"]/*' />
        public bool EditorFactoryNotify
        {
            get { return this._editorFactoryNotify; }
            set { this._editorFactoryNotify = value; }
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.TemplateDir"]/*' />
        public string TemplateDir
        {
            get { return this._templateDir; }
            set { this._templateDir = value; }
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.NameResourceID"]/*' />
        public int NameResourceID
        {
            get { return this._resId; }
            set { this._resId = value; }
        }

        public int EditorNameResourceId
        {
            get { return this._editorNameResId; }
            set { this._editorNameResId = value; }
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="ProvideEditorExtensionAttribute.DefaultName"]/*' />
        public string DefaultName
        {
            get { return this._editorName; }
            set { this._editorName = value; }
        }

        /// <summary>
        ///        The reg key name of this extension.
        /// </summary>
        private string RegKeyName => string.Format(CultureInfo.InvariantCulture, "Editors\\{0}", this.Factory.ToString("B"));

        /// <summary>
        ///        The reg key name of the project.
        /// </summary>
        private string ProjectRegKeyName(RegistrationContext context)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "Projects\\{0}\\AddItemTemplates\\TemplateDirs\\{1}",
                                 this._project.ToString("B"),
                                 context.ComponentType.GUID.ToString("B"));
        }

        private string EditorFactoryNotifyKey => string.Format(CultureInfo.InvariantCulture, "Projects\\{0}\\FileExtensions\\{1}",
                                     this._project.ToString("B"),
                                     this.Extension);

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="Register"]' />
        /// <devdoc>
        ///     Called to register this attribute with the given context.  The context
        ///     contains the location where the registration inforomation should be placed.
        ///     it also contains such as the type being registered, and path information.
        ///
        ///     This method is called both for registration and unregistration.  The difference is
        ///     that unregistering just uses a hive that reverses the changes applied to it.
        /// </devdoc>
        public override void Register(RegistrationContext context)
        {
            using (var editorKey = context.CreateKey(this.RegKeyName))
            {
                if (!string.IsNullOrEmpty(this.DefaultName))
                {
                    editorKey.SetValue(null, this.DefaultName);
                }
                if (0 != this._editorNameResId)
                {
                    editorKey.SetValue("DisplayName", "#" + this._editorNameResId.ToString(CultureInfo.InvariantCulture));
                }
                else if (0 != this._resId)
                {
                    editorKey.SetValue("DisplayName", "#" + this._resId.ToString(CultureInfo.InvariantCulture));
                }

                if (this._linkedEditorGuid != Guid.Empty)
                {
                    editorKey.SetValue("LinkedEditorGuid", this._linkedEditorGuid.ToString("B"));
                }
                if (this._commonViewAttrs != 0)
                {
                    editorKey.SetValue("CommonPhysicalViewAttributes", (int)this._commonViewAttrs);
                }
                editorKey.SetValue("Package", context.ComponentType.GUID.ToString("B"));
            }

            using (var extensionKey = context.CreateKey(this.RegKeyName + "\\Extensions"))
            {
                extensionKey.SetValue(this.Extension.Substring(1), this.Priority);

                if (this._extensions != null && this._extensions.Length > 0)
                {
                    foreach (var extension in this._extensions)
                    {
                        var extensionAndPri = extension.Split(':');
                        if (extensionAndPri.Length != 2 || !int.TryParse(extensionAndPri[1], out var pri))
                        {
                            throw new InvalidOperationException("Expected extension:priority");
                        }

                        extensionKey.SetValue(extensionAndPri[0], pri);
                    }
                }
            }

            // Build the path of the registry key for the "Add file to project" entry
            if (this._project != Guid.Empty)
            {
                var prjRegKey = ProjectRegKeyName(context) + "\\/1";
                using (var projectKey = context.CreateKey(prjRegKey))
                {
                    if (0 != this._resId)
                    {
                        projectKey.SetValue("", "#" + this._resId.ToString(CultureInfo.InvariantCulture));
                    }

                    if (this._templateDir.Length != 0)
                    {
                        var url = new Uri(context.ComponentType.Assembly.CodeBase);
                        var templates = url.LocalPath;
                        templates = CommonUtils.GetAbsoluteDirectoryPath(Path.GetDirectoryName(templates), this._templateDir);
                        templates = context.EscapePath(templates);
                        projectKey.SetValue("TemplatesDir", templates);
                    }
                    projectKey.SetValue("SortPriority", this.Priority);
                }
            }

            // Register the EditorFactoryNotify
            if (this.EditorFactoryNotify)
            {
                // The IVsEditorFactoryNotify interface is called by the project system, so it doesn't make sense to
                // register it if there is no project associated to this editor.
                if (this._project == Guid.Empty)
                {
                    throw new InvalidOperationException("No project associated.");
                }

                // Create the registry key
                using (var edtFactoryNotifyKey = context.CreateKey(this.EditorFactoryNotifyKey))
                {
                    edtFactoryNotifyKey.SetValue("EditorFactoryNotify", this.Factory.ToString("B"));
                }
            }
        }

        /// <include file='doc\ProvideEditorExtensionAttribute.uex' path='docs/doc[@for="Unregister"]' />
        /// <devdoc>
        /// Unregister this editor.
        /// </devdoc>
        /// <param name="context"></param>
        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(this.RegKeyName);
            if (this._project != Guid.Empty)
            {
                context.RemoveKey(ProjectRegKeyName(context));
                if (this.EditorFactoryNotify)
                {
                    context.RemoveKey(this.EditorFactoryNotifyKey);
                }
            }
        }
    }
}
