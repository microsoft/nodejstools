// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using VSLangProj;

namespace Microsoft.VisualStudioTools.Project.Automation
{
    [ComVisible(true)]
    public class OAAssemblyReference : OAReferenceBase
    {
        internal OAAssemblyReference(AssemblyReferenceNode assemblyReference) :
            base(assemblyReference)
        {
        }

        internal new AssemblyReferenceNode BaseReferenceNode => (AssemblyReferenceNode)base.BaseReferenceNode;

        #region Reference override
        public override int BuildNumber
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return this.BaseReferenceNode.ResolvedAssembly.Version.Build;
            }
        }
        public override string Culture
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.CultureInfo))
                {
                    return string.Empty;
                }
                return this.BaseReferenceNode.ResolvedAssembly.CultureInfo.Name;
            }
        }
        public override string Identity
        {
            get
            {
                // Note that in this function we use the assembly name instead of the resolved one
                // because the identity of this reference is the assembly name needed by the project,
                // not the specific instance found in this machine / environment.
                if (null == this.BaseReferenceNode.AssemblyName)
                {
                    return null;
                }
                // changed from MPFProj, http://mpfproj10.codeplex.com/workitem/11274
                return this.BaseReferenceNode.AssemblyName.Name;
            }
        }
        public override int MajorVersion
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return this.BaseReferenceNode.ResolvedAssembly.Version.Major;
            }
        }
        public override int MinorVersion
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return this.BaseReferenceNode.ResolvedAssembly.Version.Minor;
            }
        }

        public override string PublicKeyToken
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                (null == this.BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken()))
                {
                    return null;
                }
                var builder = new StringBuilder();
                var publicKeyToken = this.BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken();
                for (var i = 0; i < publicKeyToken.Length; i++)
                {
                    // changed from MPFProj:
                    // http://mpfproj10.codeplex.com/WorkItem/View.aspx?WorkItemId=8257
                    builder.AppendFormat("{0:x2}", publicKeyToken[i]);
                }
                return builder.ToString();
            }
        }

        public override string Name
        {
            get
            {
                if (null != this.BaseReferenceNode.ResolvedAssembly)
                {
                    return this.BaseReferenceNode.ResolvedAssembly.Name;
                }
                if (null != this.BaseReferenceNode.AssemblyName)
                {
                    return this.BaseReferenceNode.AssemblyName.Name;
                }
                return null;
            }
        }
        public override int RevisionNumber
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return this.BaseReferenceNode.ResolvedAssembly.Version.Revision;
            }
        }
        public override bool StrongName
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (0 == (this.BaseReferenceNode.ResolvedAssembly.Flags & AssemblyNameFlags.PublicKey)))
                {
                    return false;
                }
                return true;
            }
        }
        public override prjReferenceType Type => prjReferenceType.prjReferenceTypeAssembly;
        public override string Version
        {
            get
            {
                if ((null == this.BaseReferenceNode.ResolvedAssembly) ||
                    (null == this.BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return string.Empty;
                }
                return this.BaseReferenceNode.ResolvedAssembly.Version.ToString();
            }
        }
        #endregion
    }
}
