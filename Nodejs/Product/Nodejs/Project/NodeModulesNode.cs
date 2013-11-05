using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using EnvDTE;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools.Project;

namespace Microsoft.NodejsTools.Project
{
    class NodeModulesNode : HierarchyNode
    {

        #region Constants

        /// <summary>
        /// The caption to display for this node
        /// </summary>
        private const string c_Caption = "Node Modules";

        /// <summary>
        /// The GUID for this node
        /// </summary>
        public const string NodeModulesVirtualName = "NodeModules";

        #endregion

        #region Member variables

        private readonly NodejsProjectNode m_ProjectNode;
        private FileSystemWatcher m_Watcher;
        private Timer m_FileSystemWatcherTimer;
        private INpmController m_NpmController; //  TODO: This is totally not the right place for this!!
        private readonly object m_Lock = new object();

        private bool m_IsDisposed;

        #endregion

        #region Initialisation

        public NodeModulesNode(NodejsProjectNode root) : base( root )
        {
            m_ProjectNode = root;
            ExcludeNodeFromScc = true;

            m_Watcher = new FileSystemWatcher(m_ProjectNode.BuildProject.DirectoryPath) { NotifyFilter = NotifyFilters.LastWrite };
            m_Watcher.Changed += m_Watcher_Changed;
            m_Watcher.EnableRaisingEvents = true;
        }

        protected override void Dispose( bool disposing )
        {
            if ( ! m_IsDisposed )
            {
                lock ( m_Lock )
                {
                    m_Watcher.Changed -= m_Watcher_Changed;
                    m_Watcher.Dispose();

                    if ( null != m_FileSystemWatcherTimer )
                    {
                        m_FileSystemWatcherTimer.Dispose();
                        m_FileSystemWatcherTimer = null;
                    }

                    if ( null != m_NpmController )
                    {
                        m_NpmController.OutputLogged -= m_NpmController_OutputLogged;
                        m_NpmController.ErrorLogged -= m_NpmController_ErrorLogged;
                    }
                }
                m_IsDisposed = true;
            }

            base.Dispose( disposing );
        }

        #endregion

        #region Properties

        public INpmController NpmController { get { return m_NpmController; } }

        #endregion

        #region Updating module hierarchy

        void m_Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            if (!path.EndsWith("package.json") && !path.Contains("\\node_modules"))
            {
                return;
            }

            lock (m_Lock)
            {
                if ( null != m_FileSystemWatcherTimer )
                {
                    m_FileSystemWatcherTimer.Dispose();
                }

                m_FileSystemWatcherTimer = new Timer(o => UpdateModulesFromTimer(), null, 1000, Timeout.Infinite);
            }
        }

        private void UpdateModulesFromTimer()
        {
            lock ( m_Lock )
            {
                if ( null != m_FileSystemWatcherTimer )
                {
                    m_FileSystemWatcherTimer.Dispose();
                    m_FileSystemWatcherTimer = null;
                }

                ReloadModules();
            }

            if ( UIThread.Instance.IsUIThread )
            {
                ReloadHierarchy();
            }
            else
            {
                UIThread.Instance.Run(ReloadHierarchy);
            }
        }

        private void ReloadModules()
        {
            lock ( m_Lock )
            {
                if ( null == m_NpmController )
                {
                    m_NpmController = NpmControllerFactory.Create( m_ProjectNode.BuildProject.DirectoryPath );
                    m_NpmController.OutputLogged += m_NpmController_OutputLogged;
                    m_NpmController.ErrorLogged += m_NpmController_ErrorLogged;
                }
                else
                {
                    m_NpmController.Refresh();
                }
            }
        }

        private static readonly Guid NpmOutputPaneGuid = new Guid( "25764421-33B8-4163-BD02-A94E299D52D8" );

        private IVsOutputWindowPane GetNpmOutputPane()
        {
            var outputWindow = ( IVsOutputWindow ) m_ProjectNode.GetService( typeof ( SVsOutputWindow ) );
            IVsOutputWindowPane pane;
            if ( outputWindow.GetPane(NpmOutputPaneGuid, out pane) != VSConstants.S_OK )
            {
                outputWindow.CreatePane( NpmOutputPaneGuid, "Npm", 1, 1 );
                outputWindow.GetPane( NpmOutputPaneGuid, out pane );
            }

            return pane;
        }

        private void WriteNpmLogToOutputWindow( NpmLogEventArgs args )
        {
            var pane = GetNpmOutputPane();
            if ( null != pane )
            {
                pane.OutputStringThreadSafe( args.LogText );
            }
        }

        void m_NpmController_ErrorLogged(object sender, NpmLogEventArgs e)
        {
            WriteNpmLogToOutputWindow( e );
        }

        void m_NpmController_OutputLogged(object sender, NpmLogEventArgs e)
        {
            WriteNpmLogToOutputWindow( e );
        }

        private void ReloadHierarchy()
        {
            INpmController controller;

            lock ( m_Lock )
            {
                controller = m_NpmController;
            }

            if (null != controller)
            {
                ReloadHierarchy(this, controller.RootPackage.Modules);
            }
        }

        private void ReloadHierarchy( HierarchyNode parent, INodeModules modules )
        {
            //  We're going to reuse nodes for which matching modules exist in the new set.
            //  The reason for this is that we want to preserve the expansion state of the
            //  hierarchy. If we just bin everything off and recreate it all from scratch
            //  it'll all be in the collapsed state, which will be annoying for users who
            //  have drilled down into the hierarchy
            var recycle = new Dictionary<string, DependencyNode>();
            var remove = new List<HierarchyNode>();
            for (var current = parent.FirstChild; null != current; current = current.NextSibling)
            {
                var dep = current as DependencyNode;
                if (null == dep)
                {
                    remove.Add(current);
                    continue;
                }

                if (modules.Any(module =>
                    module.Name == dep.Package.Name
                    && module.Version == dep.Package.Version
                    && module.IsBundledDependency == dep.Package.IsBundledDependency
                    && module.IsDevDependency == dep.Package.IsDevDependency
                    && module.IsListedInParentPackageJson == dep.Package.IsListedInParentPackageJson
                    && module.IsMissing == dep.Package.IsMissing
                    && module.IsOptionalDependency == dep.Package.IsOptionalDependency))
                {
                    recycle[dep.Package.Name] = dep;
                }
                else
                {
                    remove.Add(current);
                }
            }

            foreach (var obsolete in remove)
            {
                parent.RemoveChild(obsolete);
                ProjectMgr.OnItemDeleted(obsolete);
            }

            foreach (var package in modules)
            {
                DependencyNode child;

                if (recycle.ContainsKey(package.Name))
                {
                    child = recycle[package.Name];
                    child.Package = package;
                }
                else
                {
                    child = new DependencyNode(m_ProjectNode, parent as DependencyNode, package);
                    parent.AddChild(child);
                }

                ReloadHierarchy(child, package.Modules);
                if (!recycle.ContainsKey(package.Name))
                {
                    child.ExpandItem(EXPANDFLAGS.EXPF_CollapseFolder);
                }
            }
        }

        #endregion

        #region HierarchyNode implementation

        public override string GetEditLabel()
        {
            return null;
        }

        public override int SortPriority
        {
            get { return DefaultSortOrderNode.ReferenceContainerNode + 1; }
        }

        public override object GetIconHandle( bool open )
        {
            return ProjectMgr.ImageHandler.GetIconHandle(open ? (int)ProjectNode.ImageName.OpenReferenceFolder : (int)ProjectNode.ImageName.ReferenceFolder);
        }

        public override string Url
        {
            get { return NodeModulesVirtualName; }
        }

        public override string Caption
        {
            get { return c_Caption; }
        }

        public override Guid ItemTypeGuid
        {
            get { return VSConstants.GUID_ItemType_VirtualFolder; }
        }

        public override int MenuCommandId
        {
            get { return (int)PkgCmdId.menuIdNPM; }
        }

        #endregion
    }
}
