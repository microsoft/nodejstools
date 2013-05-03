using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.NodejsTools {
#if FALSE
    [ComVisible(true)]
    public class OAProject : EnvDTE.Project, EnvDTE.ISupportVSProperties {
        private readonly EnvDTE.Project _project;
        private readonly ProjectWrapper _wrapper;

        internal OAProject(EnvDTE.Project project, ProjectWrapper wrapper) {
            _project = project;
            _wrapper = wrapper;
        }

        #region Project Members

        public EnvDTE.CodeModel CodeModel {
            get { return _project.CodeModel; }
        }

        public EnvDTE.Projects Collection {
            get { return _project.Collection; }
        }

        public EnvDTE.ConfigurationManager ConfigurationManager {
            get { return _project.ConfigurationManager; }
        }

        public EnvDTE.DTE DTE {
            get { return _project.DTE; }
        }

        public void Delete() {
            _project.Delete();
        }

        public string ExtenderCATID {
            get { return _project.ExtenderCATID; }
        }

        public object ExtenderNames {
            get { return _project.ExtenderNames; }
        }

        public string FileName {
            get { return _project.FileName; }
        }

        public string FullName {
            get { return _project.FullName; }
        }

        public EnvDTE.Globals Globals {
            get { return _project.Globals; }
        }

        public bool IsDirty {
            get {
                return _project.IsDirty;
            }
            set {
                _project.IsDirty = value;
            }
        }

        public string Kind {
            get { return _project.Kind; }
        }

        public string Name {
            get {
                return _project.Name;
            }
            set {
                _project.Name = value;
            }
        }

        public new object Object {
            get { return _project.Object; }
        }

        public EnvDTE.ProjectItem ParentProjectItem {
            get { return _project.ParentProjectItem; }
        }

        public EnvDTE.ProjectItems ProjectItems {
            get { return _project.ProjectItems; }
        }

        public EnvDTE.Properties Properties {
            get { return new OAProperties(_project.Properties); }
        }

        public void Save(string FileName = "") {
            _project.Save(FileName);
        }

        public void SaveAs(string NewFileName) {
            _project.SaveAs(NewFileName);
        }

        public bool Saved {
            get {
                return _project.Saved;
            }
            set {
                _project.Saved = value;
            }
        }

        public string UniqueName {
            get {
                // Get Solution service
                IVsSolution solution = NodePackage.GetGlobalService(typeof(SVsSolution)) as IVsSolution;                

                // Ask solution for unique name of project
                string uniqueName = string.Empty;
                ErrorHandler.ThrowOnFailure(solution.GetUniqueNameOfProject(_wrapper, out uniqueName));
                return uniqueName;
            }
        }

        public object get_Extender(string ExtenderName) {
            return _project.get_Extender(ExtenderName);
        }

        #endregion

        #region ISupportVSProperties Members

        public void NotifyPropertiesDelete() {
            if (_project is ISupportVSProperties) {
                ((ISupportVSProperties)_project).NotifyPropertiesDelete();
            }
        }

        #endregion
    }

    class OAProperties : EnvDTE.Properties {
        private readonly Properties _properties;

        public OAProperties(Properties properties) {
            _properties = properties;
        }

        #region Properties Members

        public object Application {
            get { return _properties.Application;  }
        }

        public int Count {
            get { return _properties.Count; }
        }

        public EnvDTE.DTE DTE {
            get { return _properties.DTE; }
        }

        public System.Collections.IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        public EnvDTE.Property Item(object index) {
            if (index is string && (string)index == "TargetFramework") {
                return new OAProperty(this, "TargetFramework", (uint)0x40000);
            }

            return _properties.Item(index);
        }

        public object Parent {
            get { return _properties.Parent; }
        }

        #endregion
    }
#endif
}
