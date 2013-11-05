using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Microsoft.NodejsTools.NpmUI
{
	/// <summary>
	/// Interaction logic for UpdateDialog.xaml
	/// </summary>
	public partial class UpdateDialog 
	{
		public UpdateDialog(Task task)
		{
			InitializeComponent();

			task.ContinueWith(t => CloseMe());
		}

		private delegate void CloseMeDelegate();

		private void CloseMe()
		{
			if (Dispatcher.CheckAccess())
			{
				Close();
			}
			else
			{
				Dispatcher.Invoke(DispatcherPriority.Normal, new CloseMeDelegate(CloseMe));
			}
		}
	}
}
