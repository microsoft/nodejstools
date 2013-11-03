using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using Microsoft.NodejsTools.Npm;
using Microsoft.VisualStudio;
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
                m_NpmController = NpmControllerFactory.Create( m_ProjectNode.BuildProject.DirectoryPath );
            }
        }

        private void ReloadHierarchy()
        {
            foreach ( var child in new List< HierarchyNode >( AllChildren ) )
            {
                RemoveChild(child);
            }

            lock ( m_Lock )
            {
                if ( null != m_NpmController )
                {
                    ReloadHierarchy(this, m_NpmController.RootPackage.Modules);
                }
            }
        }

        private void ReloadHierarchy( HierarchyNode parent, INodeModules modules )
        {
            foreach (IPackage package in modules)
            {
                var child = new DependencyNode(m_ProjectNode, parent as DependencyNode, package);
                parent.AddChild(child);
                ReloadHierarchy(child, package.Modules);
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
