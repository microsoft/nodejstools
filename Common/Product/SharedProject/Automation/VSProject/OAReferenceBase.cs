//*********************************************************//
//    Copyright (c) Microsoft. All rights reserved.
//    
//    Apache 2.0 License
//    
//    You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//    
//    Unless required by applicable law or agreed to in writing, software 
//    distributed under the License is distributed on an "AS IS" BASIS, 
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or 
//    implied. See the License for the specific language governing 
//    permissions and limitations under the License.
//
//*********************************************************//

using System;
using System.Runtime.InteropServices;
using VSLangProj;
using VSLangProj80;

namespace Microsoft.VisualStudioTools.Project.Automation {
    /// <summary>
    /// Represents the automation equivalent of ReferenceNode
    /// </summary>
    /// <typeparam name="RefType"></typeparam>
    [ComVisible(true)]
    public abstract class OAReferenceBase : Reference3 {
        #region fields
        private ReferenceNode referenceNode;
        #endregion

        #region ctors
        internal OAReferenceBase(ReferenceNode referenceNode) {
            this.referenceNode = referenceNode;
        }
        #endregion

        #region properties
        internal ReferenceNode BaseReferenceNode {
            get { return referenceNode; }
        }
        #endregion

        #region Reference Members
        public virtual int BuildNumber {
            get { return 0; }
        }

        public virtual References Collection {
            get {
                return BaseReferenceNode.Parent.Object as References;
            }
        }

        public virtual EnvDTE.Project ContainingProject {
            get {
                return BaseReferenceNode.ProjectMgr.GetAutomationObject() as EnvDTE.Project;
            }
        }

        public virtual bool CopyLocal {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual string Culture {
            get { throw new NotImplementedException(); }
        }

        public virtual EnvDTE.DTE DTE {
            get {
                return BaseReferenceNode.ProjectMgr.Site.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            }
        }

        public virtual string Description {
            get {
                return this.Name;
            }
        }

        public virtual string ExtenderCATID {
            get { throw new NotImplementedException(); }
        }

        public virtual object ExtenderNames {
            get { throw new NotImplementedException(); }
        }

        public virtual string Identity {
            get { throw new NotImplementedException(); }
        }

        public virtual int MajorVersion {
            get { return 0; }
        }

        public virtual int MinorVersion {
            get { return 0; }
        }

        public virtual string Name {
            get { throw new NotImplementedException(); }
        }

        public virtual string Path {
            get {
                return BaseReferenceNode.Url;
            }
        }

        public virtual string PublicKeyToken {
            get { throw new NotImplementedException(); }
        }

        public virtual void Remove() {
            BaseReferenceNode.Remove(false);
        }

        public virtual int RevisionNumber {
            get { return 0; }
        }

        public virtual EnvDTE.Project SourceProject {
            get { return null; }
        }

        public virtual bool StrongName {
            get { return false; }
        }

        public virtual prjReferenceType Type {
            get { throw new NotImplementedException(); }
        }

        public virtual string Version {
            get { return new Version().ToString(); }
        }

        public virtual object get_Extender(string ExtenderName) {
            throw new NotImplementedException();
        }
        #endregion

        public string Aliases {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public bool AutoReferenced {
            get { throw new NotImplementedException(); }
        }

        public virtual bool Isolated {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public uint RefType {
            get { throw new NotImplementedException(); }
        }

        public virtual bool Resolved {
            get { throw new NotImplementedException(); }
        }

        public string RuntimeVersion {
            get { throw new NotImplementedException(); }
        }

        public virtual bool SpecificVersion {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual string SubType {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }
    }
}
