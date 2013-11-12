using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;
using Timer = System.Windows.Forms.Timer;

namespace Microsoft.NodejsTools.NpmUI
{
    internal partial class PackageSearchPane : UserControl
    {
        private BusyControl _busy;
        private Timer _keypressFilterDelayTimer;
        private INpmController _npmController;
        private IEnumerable< IPackage > _allPackages;
        private IList< IPackage > _filteredPackages; 

        public PackageSearchPane()
        {
            InitializeComponent();
        }

        private async void LoadCatalogue()
        {
            _listResults.Hide();
            if ( null == _busy )
            {
                _busy = new BusyControl
                {
                    Message = "Loading published package list...",
                    Dock = DockStyle.Fill
                };
                this.Controls.Add( _busy );
            }
            else
            {
                _busy.Show();
            }

            _allPackages = await _npmController.GetRepositoryCatalogueAsync();
            _busy.Hide();
            _listResults.Show();
            StartFilter();
        }

        private void StartFilter()
        {
            ThreadPool.QueueUserWorkItem(o => Filter(_txtFind.Text));
        }

        private void Filter(string filterString)
        {
            filterString = filterString.ToLower();

            var target = new List< IPackage >();
            foreach ( var package in _allPackages )
            {
                if ( string.IsNullOrEmpty( filterString ) || package.Name.ToLower().Contains( filterString ) )
                {
                    target.Add( package );
                    continue;
                }

                string description = package.Description;
                if ( null != description && description.ToLower().Contains( filterString ) )
                {
                    target.Add( package );
                }

                //  TODO: match on keywords also!
            }

            target.Sort( new NpmSearchComparer( filterString ) );
            BeginInvoke( new Action( () => SetListData( target ) ) );
        }

        private void SetListData( IList< IPackage > filtered )
        {
            _filteredPackages = filtered;
            _listResults.VirtualListSize = _filteredPackages.Count;
            _listResults.Invalidate();
        }

        public INpmController NpmController
        {
            set
            {
                _npmController = value;
                BeginInvoke( new Action( LoadCatalogue ) );
            }
        }

        private void _listResults_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = new ListViewItem() { Tag = _filteredPackages[ e.ItemIndex ] };
        }

        private void _listResults_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            PackageListItemPainter.DrawItem( this, e );
        }

        private void _txtFind_KeyUp(object sender, KeyEventArgs e)
        {
            if ( null == _keypressFilterDelayTimer )
            {
                _keypressFilterDelayTimer = new Timer();
                _keypressFilterDelayTimer.Interval = 1000;
                _keypressFilterDelayTimer.Tick += _keypressFilterDelayTimer_Tick;
            }
            else
            {
                _keypressFilterDelayTimer.Stop();
            }
            _keypressFilterDelayTimer.Start();
        }

        void _keypressFilterDelayTimer_Tick(object sender, EventArgs e)
        {
            _keypressFilterDelayTimer.Stop();
            _keypressFilterDelayTimer.Tick -= _keypressFilterDelayTimer_Tick;
            _keypressFilterDelayTimer.Dispose();
            _keypressFilterDelayTimer = null;

            BeginInvoke( new Action( StartFilter ) );
        }
    }
}
