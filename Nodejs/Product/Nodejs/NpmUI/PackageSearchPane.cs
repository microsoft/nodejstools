/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.NodejsTools.Npm;
using Timer = System.Windows.Forms.Timer;

namespace Microsoft.NodejsTools.NpmUI{
    internal partial class PackageSearchPane : UserControl{
        private BusyControl _busy;
        private Timer _keypressFilterDelayTimer;
        private INpmController _npmController;
        private IEnumerable<IPackage> _allPackages;
        private IList<IPackage> _filteredPackages;

        public PackageSearchPane(){
            InitializeComponent();

            //  Hack to get a single auto-sized column
            var header = _listResults.Columns.Add("Package", "Package");
            header.Width = -2;
            _listResults.HeaderStyle = ColumnHeaderStyle.None;
            //  /hack

            //  Hack to force the row height
            var images = new ImageList();
            images.ImageSize = new Size(40, 40);
            _listResults.SmallImageList = images;
            _listResults.LargeImageList = images;
            //  /hack
        }

        private async void LoadCatalogue(){
            _listResults.Hide();
            if (null == _busy){
                _busy = new BusyControl{
                    Message = "Loading published package list...",
                    Dock = DockStyle.Fill
                };
                this.Controls.Add(_busy);
            } else{
                _busy.Show();
            }

            try{
                _allPackages = await _npmController.GetRepositoryCatalogueAsync();
                _busy.Hide();
                _listResults.Show();
                StartFilter();
            } catch (NpmNotFoundException){
                _busy.Finished = true;
                _busy.Message = "Catalog retrieval aborted - npm.cmd not found";
            }
        }

        private void StartFilter(){
            ThreadPool.QueueUserWorkItem(o => Filter(_txtFind.Text));
        }

        private void Filter(string filterString){
            if (null == _allPackages){
                return;
            }

            filterString = filterString.ToLower();

            var target = new List<IPackage>();
            foreach (var package in _allPackages){
                if (string.IsNullOrEmpty(filterString) || package.Name.ToLower().Contains(filterString)){
                    target.Add(package);
                    continue;
                }

                string description = package.Description;
                if (null != description && description.ToLower().Contains(filterString)){
                    target.Add(package);
                }

                //  TODO: match on keywords also!
            }

            target.Sort(new NpmSearchComparer(filterString));
            if (!IsDisposed){
                BeginInvoke(new Action(() => SetListData(target)));
            }
        }

        private void SetListData(IList<IPackage> filtered){
            _filteredPackages = filtered;
            _listResults.VirtualListSize = _filteredPackages.Count;
            _listResults.Invalidate();
        }

        public INpmController NpmController{
            set{
                _npmController = value;
                BeginInvoke(new Action(LoadCatalogue));
            }
        }

        private void _listResults_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e){
            e.Item = new ListViewItem(){Tag = _filteredPackages[e.ItemIndex]};
        }

        private void _listResults_DrawItem(object sender, DrawListViewItemEventArgs e){
            PackageListItemPainter.DrawItem(this, e);
        }

        private void _txtFind_KeyUp(object sender, KeyEventArgs e){
            if (null == _keypressFilterDelayTimer){
                _keypressFilterDelayTimer = new Timer();
                _keypressFilterDelayTimer.Interval = 1000;
                _keypressFilterDelayTimer.Tick += _keypressFilterDelayTimer_Tick;
            } else{
                _keypressFilterDelayTimer.Stop();
            }
            _keypressFilterDelayTimer.Start();
        }

        private void _keypressFilterDelayTimer_Tick(object sender, EventArgs e){
            _keypressFilterDelayTimer.Stop();
            _keypressFilterDelayTimer.Tick -= _keypressFilterDelayTimer_Tick;
            _keypressFilterDelayTimer.Dispose();
            _keypressFilterDelayTimer = null;

            BeginInvoke(new Action(StartFilter));
        }

        public event EventHandler SelectedPackageChanged;

        private void OnSelectedPackageChanged(){
            var handlers = SelectedPackageChanged;
            if (null != handlers){
                handlers(this, EventArgs.Empty);
            }
        }

        public IPackage SelectedPackage{
            get{
                var indices = _listResults.SelectedIndices;
                if (null == indices
                    || indices.Count == 0
                    || indices[0] < 0
                    || null == _filteredPackages
                    || indices[0] >= _filteredPackages.Count){
                    return null;
                }

                return _filteredPackages[indices[0]];
            }
        }

        private void _listResults_SelectedIndexChanged(object sender, EventArgs e){
            OnSelectedPackageChanged();
        }
    }
}