using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.NodejsTools.NpmUI
{
	class UpdateDialogViewModel : INotifyPropertyChanged
	{
		private string m_whatIsUpdating = "ALL THE THINGS";

		private string m_log;

		public string WhatIsUpdating
		{
			get
			{
				return m_whatIsUpdating;
			}
			set
			{
				if (value == m_whatIsUpdating)
				{
					return;
				}
				m_whatIsUpdating = value;
				OnPropertyChanged();
			}
		}

		public string Log
		{
			get
			{
				return m_log;
			}
			set
			{
				if (value == m_log)
				{
					return;
				}
				m_log = value;
				OnPropertyChanged();
			}
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	}
}
