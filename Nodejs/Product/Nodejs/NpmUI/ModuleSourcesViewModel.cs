using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.NodejsTools.Npm;

namespace Microsoft.NodejsTools.NpmUI
{
	public class ModuleSourcesViewModel : INotifyPropertyChanged
	{
		private const string RefreshingProjectModules = "Refreshing project modules...";
		private const string InstallingModule = "Installing module '{0}'";
		private const string UninstallingModule = "Uninstalling module '{0}'";
		private const string InstallFailed = "Install of module '{0}' failed";
		private const string UninstallFailed = "Uninstall of module '{0}' failed";
		private const string SearchingForModule = "Searching for '{0}'";

        //private readonly INpmWrangler m_npm;
        //private readonly IPackageWrangler m_packageWrangler;
        //private readonly IModuleWrangler m_moduleWrangler;

		public ModuleSourcesViewModel(INpmController controller)
            //INpmWrangler npm, IPackageWrangler packageWrangler, IModuleWrangler moduleWrangler)
		{
            //m_npm = npm;
            //m_packageWrangler = packageWrangler;
            //m_moduleWrangler = moduleWrangler;
		    NpmController = controller;
            WeakEventManager<INpmController, EventArgs>.AddHandler( NpmController, "StartingRefresh", HandleNpmControllerStartingRefresh);
            WeakEventManager<INpmController, EventArgs>.AddHandler(NpmController, "FinishedRefresh", HandleNpmControllerFinishedRefresh);
			ShowProgressBar = Visibility.Collapsed;
			ShowProgressBarOK = Visibility.Collapsed;
			InstalledModulesExpanded = true;
		}

        private INpmController NpmController { get; set; }

	    private void HandleNpmControllerStartingRefresh( object source, EventArgs args )
	    {
	        
	    }

        private void HandleNpmControllerFinishedRefresh(object source, EventArgs args)
	    {
	        UpdateCurrentModulesCollection();
	    }
		
        //private INpmWrangler Npm { get { return m_npm; } }
        //private IModuleWrangler Modules { get { return m_moduleWrangler; } }


		#region Module collection

		private string m_selectedInstalledModuleCollection;
		private string m_selectedOnlineModuleCollection;

		readonly ObservableCollection<string> m_installedModuleSources = new ObservableCollection<string> { "All" };
		readonly ObservableCollection<string> m_onlineModuleSources = new ObservableCollection<string> { "All", "npm official module source" };
		private bool m_installedModulesExpanded;
		private bool m_onlineModulesExpanded;

		public ObservableCollection<string> InstalledModuleSources { get { return m_installedModuleSources;  } } 
		public ObservableCollection<string> OnlineModuleSources { get { return m_onlineModuleSources;  } } 
		public ObservableCollection<IPackage> CurrentModulesSelection { get { return m_currentModules; } }

		public bool InstalledModulesExpanded
		{
			get
			{
				return m_installedModulesExpanded;
			}
			set
			{
				if (Equals(value, m_installedModulesExpanded))
				{
					return;
				}
				m_installedModulesExpanded = value;
				OnPropertyChanged();
				OnlineModulesExpanded = !value;
				UpdateCurrentModulesCollection();
				OnPropertyChanged("CanSearch");
			}
		}

		public bool OnlineModulesExpanded
		{
			get
			{
				return m_onlineModulesExpanded;
			}
			set
			{
				if (Equals(value, m_onlineModulesExpanded))
				{
					return;
				}
				m_onlineModulesExpanded = value;
				OnPropertyChanged();
				InstalledModulesExpanded = !value;
				UpdateCurrentModulesCollection();
				OnPropertyChanged("CanSearch");
			}
		}


		public string SelectedInstalledModuleCollection
		{
			get
			{
				return m_selectedInstalledModuleCollection;
			}
			set
			{
				if (m_selectedInstalledModuleCollection == value)
					return;
				m_selectedInstalledModuleCollection = value;
				UpdateCurrentModulesCollection();
			}
		}

		public string SelectedOnlineModuleCollection
		{
			get
			{
				return m_selectedOnlineModuleCollection;
			}
			set
			{
				if (m_selectedOnlineModuleCollection == value)
					return;
				m_selectedOnlineModuleCollection = value;
				UpdateCurrentModulesCollection();
				OnPropertyChanged("SearchBoxEnabled");
			}
		}

		#endregion

		#region Progress bar

		private string m_progressBarText;

		private Visibility m_showProgressBarOk;

		private Visibility m_showProgressBar;

		public Visibility ShowProgressBar
		{
			get
			{
				return m_showProgressBar;
			}
			set
			{
				if (value == m_showProgressBar)
				{
					return;
				}
				m_showProgressBar = value;
				OnPropertyChanged();
				OnPropertyChanged("ShowNoModulesWarning");
				OnPropertyChanged("ShowModulesList");
				OnPropertyChanged("CanSearch");
				OnPropertyChanged("SearchBoxEnabled");
			}
		}

		public string ProgressBarText
		{
			get
			{
				return m_progressBarText;
			}
			set
			{
				if (value == m_progressBarText)
				{
					return;
				}
				m_progressBarText = value;
				OnPropertyChanged();
			}
		}

		public Visibility ShowProgressBarOK
		{
			get
			{
				return m_showProgressBarOk;
			}
			set
			{
				if (value.Equals(m_showProgressBarOk))
				{
					return;
				}
				m_showProgressBarOk = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Modules list

		readonly ObservableCollection<IPackage> m_currentModules = new ObservableCollection<IPackage>();
		private IPackage m_selectedModule;

		public Visibility ShowNoModulesWarning
		{
			get
			{
				return (ShowProgressBar != Visibility.Visible && !m_currentModules.Any()) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		public Visibility ShowModulesList
		{
			get
			{
				return (ShowProgressBar != Visibility.Visible && m_currentModules.Any() ? Visibility.Visible : Visibility.Hidden);
			}
		}


		public IPackage SelectedModule
		{
			get
			{
				return m_selectedModule;
			}
			set
			{
				if (Equals(value, m_selectedModule))
				{
					return;
				}
				m_selectedModule = value;
				OnPropertyChanged();
				OnPropertyChanged("DetailsPanelVisible");
			}
		}

		readonly object modulesListLock = new object();

		private /*async*/ void UpdateCurrentModulesCollection()
		{
			if (InstalledModulesExpanded)
			{
				ProgressBarText = RefreshingProjectModules;
				ShowProgressBar = Visibility.Visible;
				// There only is an "all" option...
			    var modules = NpmController.RootPackage.Modules;
				ShowProgressBar = Visibility.Collapsed;
				lock (modulesListLock)
				{
					m_currentModules.Clear();
					foreach (var m in modules)
					{
                        m_currentModules.Add( m );
						//m_currentModules.Add(new NpmPackage(m));
					}
				}
			}
			else
			{
				ProgressBarText = RefreshingProjectModules;
				ShowProgressBar = Visibility.Visible;
                var installedModules = NpmController.RootPackage.Modules;
				ShowProgressBar = Visibility.Collapsed;

				lock (modulesListLock)
				{
					m_currentModules.Clear();
					foreach (var m in m_searchResults)
					{
						var installedModule = installedModules.FirstOrDefault(module => module.Name == m.Name);

						if (installedModule != null)
						{
							var builder = new NodeModuleBuilder(m);
							builder.Flags |= PackageFlags.Installed;
							if (installedModule.Version != m.Version)
							{
                                builder.Flags |= PackageFlags.VersionMismatch;
							}
							m_currentModules.Add(builder.Build());
						}
						else
						{
							m_currentModules.Add(m);
						}
					}
				}
			}

			OnPropertyChanged("ShowNoModulesWarning");
			OnPropertyChanged("ShowModulesList");
		}

		#endregion

		#region Details panel

		public Visibility DetailsPanelVisible
		{
			get
			{
				return (SelectedModule != null) ? Visibility.Visible: Visibility.Hidden;
			}
		}

		#endregion

		#region Search

		private IEnumerable<IPackage> m_searchResults = new List<IPackage>();

		private string m_searchText;


		public bool CanSearch
		{
			get 
			{
				return OnlineModulesExpanded && 
				       (ShowProgressBar != Visibility.Visible) &&
				       !string.IsNullOrEmpty(SearchText) &&
					   SearchText.Length >= 3;
			}
		}

		public bool SearchBoxEnabled
		{
			get
			{
				return OnlineModulesExpanded &&
					   (ShowProgressBar != Visibility.Visible);
			}
		}

		public void DismissProgressBar()
		{
			ShowProgressBar = Visibility.Collapsed;
			ShowProgressBarOK = Visibility.Collapsed;
			UpdateCurrentModulesCollection();
		}


		public string SearchText
		{
			get
			{
				return m_searchText;
			}
			set
			{
				if (value == m_searchText)
				{
					return;
				}
				m_searchText = value;
				OnPropertyChanged();
				OnPropertyChanged("CanSearch");
			}
		}
		#endregion

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion

		#region Commands

		public async Task InstallationAction(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
		{
			bool install = (SelectedModule.Flags & PackageFlags.Installed) == 0;

			string version = install
                ? (SelectedModule.Flags & PackageFlags.Missing) != 0 ? SelectedModule.RequestedVersionRange : "*"
				: SelectedModule.Version.ToString();
			
			ProgressBarText = string.Format(install ? InstallingModule : UninstallingModule, SelectedModule.Name + "@" + version);
			
			ShowProgressBar = Visibility.Visible;

			bool result;
			if (install)
			{
				// Is this good behaviour? If we're installing a missing module, install the version requested
				// otherwise install the latest

				//result = await RedGate.NodeJs.VsProject.Modules.Install(SelectedModule.Name, version);
			    result = await NpmController.InstallPackageByVersionAsync( SelectedModule.Name, version, DependencyType.Standard );
			}
			else
			{
                //result = await RedGate.NodeJs.VsProject.Modules.Uninstall(SelectedModule);
			    result = await NpmController.UninstallPackageAsync( SelectedModule.Name );
			}

			if (!result)
			{
				ProgressBarText = string.Format(install ? InstallFailed : UninstallFailed, SelectedModule.Name + "@" + version);
				ShowProgressBarOK = Visibility.Visible;
			}
			else
			{
				ShowProgressBar = Visibility.Collapsed;

				UpdateCurrentModulesCollection();
			}

		}

		public async void Search()
		{
			if (!CanSearch)
				return;

			ProgressBarText = string.Format(SearchingForModule, SearchText);
			ShowProgressBar = Visibility.Visible;
			m_searchResults = await NpmController.SearchAsync(SearchText);
			ShowProgressBar = Visibility.Collapsed;
			UpdateCurrentModulesCollection();
		}

		public void UpdatePackagesJson()
		{
            //  Don't need to do this because packages.json will be updated by npm
            //if ((SelectedModule.Flags & PackageFlags.Missing) != 0)
            //{
            //    m_packageWrangler.Dependencies.Remove(SelectedModule.Name);
            //}
            //else if ((SelectedModule.Flags & PackageFlags.NotListedAsDependency) != 0)
            //{
            //    m_packageWrangler.Dependencies[SelectedModule.Name] = SelectedModule.Version;
            //}

			UpdateCurrentModulesCollection();
		}

		#endregion

	}
}
