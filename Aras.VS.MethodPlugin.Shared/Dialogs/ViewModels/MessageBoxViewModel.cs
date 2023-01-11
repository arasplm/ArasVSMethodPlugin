//------------------------------------------------------------------------------
// <copyright file="MessageBoxViewModel.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class MessageBoxViewModel
	{
		private ICommand closeCommand;

		public MessageBoxViewModel()
		{
			closeCommand = new RelayCommand<object>(OnCloseCliked);
		}

		public ICommand CloseCommand { get { return closeCommand; } }

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}
	}
}
