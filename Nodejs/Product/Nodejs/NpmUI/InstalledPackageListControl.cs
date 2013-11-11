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

#region Static image members

        static InstalledPackageListControl()
        {
            Dependency =
                Image.FromStream(
                    typeof ( InstalledPackageListControl ).Assembly.GetManifestResourceStream(
                        "Microsoft.NodejsTools.Resources.Dependency_32.png" ) );

            DependencyDev =
                Image.FromStream(
                    typeof(InstalledPackageListControl).Assembly.GetManifestResourceStream(
                        "Microsoft.NodejsTools.Resources.DependencyDev_32.png"));

            DependencyOptional =
                Image.FromStream(
                    typeof(InstalledPackageListControl).Assembly.GetManifestResourceStream(
                        "Microsoft.NodejsTools.Resources.DependencyOptional_32.png"));

            Warning =
                Image.FromStream(
                    typeof(InstalledPackageListControl).Assembly.GetManifestResourceStream(
                        "Microsoft.NodejsTools.Resources.Warning_16.png"));
        }

        internal static Image Dependency
        {
            get; private set;
        }

        internal static Image DependencyDev
        {
            get; private set;
        }

        internal static Image DependencyOptional
        {
            get; private set;
        }

        internal static Image Warning
        {
            get; private set;
        }

#endregion

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
            images.ImageSize = new Size( 40, 40 );
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

            var img = Dependency;
            if ( pkg.IsDevDependency )
            {
                img = DependencyDev;
            }
            else if ( pkg.IsOptionalDependency )
            {
                img = DependencyOptional;
            }

            g.DrawImage(
                img,
                bounds.Left + 2,
                bounds.Top + (bounds.Height - img.Height) / 2,
                img.Width,
                img.Height);

            if ( pkg.IsMissing )
            {
                g.DrawImage( 
                    Warning,
                    bounds.Left + 2,
                    bounds.Top + (bounds.Height - img.Height) / 2 + img.Height - Warning.Height,
                    Warning.Width,
                    Warning.Height);
            }

            var font = new Font( Font, FontStyle.Bold );
            TextRenderer.DrawText(
                g,
                pkg.Name,
                font,
                new Point(bounds.X + 4 + img.Width, bounds.Y + 2),
                foreColor,
                TextFormatFlags.Default);
            var size = TextRenderer.MeasureText( g, pkg.Name, font );
            TextRenderer.DrawText( 
                g,
                string.Format( "@{0}", pkg.Version ),
                Font,
                new Point(( int ) ( bounds.X + 4 + size.Width + img.Width ), bounds.Y + 2 ),
                ColorUtils.Mix( foreColor, backColor, 5, 5 ),
                TextFormatFlags.Default);

            var author = "by (unknown author)";
            if ( pkg.HasPackageJson && null != pkg.Author && ! string.IsNullOrEmpty( pkg.Author.Name ) )
            {
                author = string.Format( "by {0}", pkg.Author.Name);
            }
            size = TextRenderer.MeasureText( g, author, Font );
            TextRenderer.DrawText( 
                g,
                author,
                Font,
                new Point((int) (bounds.Right - 2 - size.Width), bounds.Y + 2),
                ColorUtils.Mix( foreColor, BackColor, 5, 5 ),
                TextFormatFlags.Default);


            TextRenderer.DrawText( 
                g,
                pkg.Description,
                Font,
                new Rectangle(
                    bounds.X + 4 + img.Width,
                    bounds.Y + 2 + Font.Height + 6,
                    bounds.Width - 6 - img.Width,
                    bounds.Height - ( 2 + Font.Height + 6 ) ),
                foreColor,
                TextFormatFlags.WordEllipsis);
        }
    }
}
