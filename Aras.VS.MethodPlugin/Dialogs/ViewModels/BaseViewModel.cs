//------------------------------------------------------------------------------
// <copyright file="BaseViewModel.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			var propChanged = PropertyChanged;
			if (propChanged != null)
			{
				propChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
