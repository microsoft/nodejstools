using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
    internal partial class InstalledPackageListControl : UserControl
    {
        public InstalledPackageListControl()
        {
            InitializeComponent();

            //  Hack to get a single auto-sized column
            var header = _listPackages.Columns.Add( "Package", "Package" );
            header.Width = -2;
            _listPackages.HeaderStyle = ColumnHeaderStyle.None;
            //  /hack

            //  Hack to force the row height
            var images = new ImageList();
            images.ImageSize = new Size( 40, 56 );
            _listPackages.SmallImageList = images;
            _listPackages.LargeImageList = images;
            //  /hack

            UpdateUIState();
        }

        private void UpdateUIState()
        {
            _btnUninstall.Enabled = _listPackages.Items.Count > 0 && _listPackages.SelectedItems.Count > 0;
        }

        public event EventHandler< PackageEventArgs > UninstallPackageRequested;

        private void OnUninstallPackageRequested( IPackage package )
        {
            var handlers = UninstallPackageRequested;
            if ( null != handlers )
            {
                handlers(this, new PackageEventArgs( package ));
            }
        }

        public IEnumerable< IPackage > Packages
        {
            set
            {
                var source = value ?? new List< IPackage >();
                _listPackages.Items.Clear();
                foreach ( var package in source )
                {
                    _listPackages.Items.Add( new ListViewItem() { Tag = package } );
                }
            }
        }

        private void _listPackages_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUIState();
        }

        private void _btnUninstall_Click(object sender, EventArgs e)
        {
            var selected = _listPackages.SelectedItems;
            if ( selected.Count > 0 )
            {
                foreach (ListViewItem item in selected)
                {
                    OnUninstallPackageRequested( item.Tag as IPackage );
                }
            }
        }

        private void _listPackages_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Color foreColor, backColor, lineColor;

            if ( ( e.State & ListViewItemStates.Selected ) == ListViewItemStates.Selected )
            {
                foreColor = SystemColors.HighlightText;
                backColor = SystemColors.Highlight;
            }
            else if ( ( e.State & ListViewItemStates.Hot ) == ListViewItemStates.Hot )
            {
                foreColor = SystemColors.WindowText;
                backColor = ColorUtils.MidPoint(SystemColors.Highlight, SystemColors.AppWorkspace);
            }
            else
            {
                foreColor = SystemColors.WindowText;
                backColor = SystemColors.Window;
            }

            lineColor = ColorUtils.MidPoint( foreColor, backColor );
            var bounds = e.Bounds;

            using ( var bg = new SolidBrush( backColor ) )
            {
                g.FillRectangle(bg, bounds);
            }

            using ( var line = new Pen( lineColor, 1F ) )
            {
                g.DrawLine( line, bounds.Left + 4, bounds.Bottom - 1, bounds.Right - 4, bounds.Bottom - 1 );
            }

            var pkg = e.Item.Tag as IPackage;
            var font = new Font( Font, FontStyle.Bold );
            TextRenderer.DrawText(
                g,
                pkg.Name,
                font,
                new Point(bounds.X + 2, bounds.Y + 2),
                foreColor,
                TextFormatFlags.Default);
            var size = TextRenderer.MeasureText( g, pkg.Name, font );
            TextRenderer.DrawText( 
                g,
                string.Format( "@{0}", pkg.Version ),
                font,
                new Point(( int ) ( bounds.X + 2 + size.Width ), bounds.Y + 2 ),
                ColorUtils.Mix( ForeColor, BackColor, 6, 4 ),
                TextFormatFlags.Default);


            TextRenderer.DrawText( 
                g,
                pkg.Description,
                Font,
                new Rectangle(
                    bounds.X + 2,
                    bounds.Y + 2 + Font.Height + 6,
                    bounds.Width - 4,
                    bounds.Height - ( 2 + Font.Height + 6 ) ),
                foreColor,
                TextFormatFlags.WordEllipsis);
        }
    }
}
