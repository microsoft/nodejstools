// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using VSLangProj;
using VSLangProj80;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    /// <summary>
    /// Represents the automation equivalent of ReferenceNode
    /// </summary>
    /// <typeparam name="RefType"></typeparam>
    [ComVisible(true)]
    public abstract class OAReferenceBase : Reference3
    {
        #region fields
        private ReferenceNode referenceNode;
        #endregion

        #region ctors
        internal OAReferenceBase(ReferenceNode referenceNode)
        {
            this.referenceNode = referenceNode;
        }
        #endregion

        #region properties
        internal ReferenceNode BaseReferenceNode => this.referenceNode;
        #endregion

        #region Reference Members
        public virtual int BuildNumber => 0;
        public virtual References Collection => this.BaseReferenceNode.Parent.Object as References;

        public virtual EnvDTE.Project ContainingProject => this.BaseReferenceNode.ProjectMgr.GetAutomationObject() as EnvDTE.Project;

        public virtual bool CopyLocal
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual string Culture => throw new NotImplementedException();
        public virtual EnvDTE.DTE DTE => this.BaseReferenceNode.ProjectMgr.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

        public virtual string Description => this.Name;

        public virtual string ExtenderCATID => throw new NotImplementedException();
        public virtual object ExtenderNames => throw new NotImplementedException();
        public virtual string Identity => throw new NotImplementedException();
        public virtual int MajorVersion => 0;
        public virtual int MinorVersion => 0;
        public virtual string Name => throw new NotImplementedException();
        public virtual string Path => this.BaseReferenceNode.Url;

        public virtual string PublicKeyToken => throw new NotImplementedException();
        public virtual void Remove()
        {
            this.BaseReferenceNode.Remove(false);
        }

        public virtual int RevisionNumber => 0;
        public virtual EnvDTE.Project SourceProject => null;
        public virtual bool StrongName => false;
        public virtual prjReferenceType Type => throw new NotImplementedException();
        public virtual string Version => new Version().ToString();
        public virtual object get_Extender(string ExtenderName)
        {
            throw new NotImplementedException();
        }
        #endregion

        public string Aliases
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool AutoReferenced => throw new NotImplementedException();
        public virtual bool Isolated
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual uint RefType =>
                // Default to native reference to help prevent callers from
                // making incorrect assumptions
                (uint)__PROJECTREFERENCETYPE.PROJREFTYPE_NATIVE;

        public virtual bool Resolved => throw new NotImplementedException();
        public string RuntimeVersion => throw new NotImplementedException();
        public virtual bool SpecificVersion
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual string SubType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
