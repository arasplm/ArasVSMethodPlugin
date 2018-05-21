//------------------------------------------------------------------------------
// <copyright file="FolderNameViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class FolderNameViewModel
	{
		private string folderName;

		private ICommand okCommand;
		private ICommand closeCommand;

		public FolderNameViewModel()
		{
			okCommand = new RelayCommand<object>(OnOkClick);
			closeCommand = new RelayCommand<object>(OnCloseCliked);
		}

		public string FolderName
		{
			get { return folderName; }
			set { folderName = value; }
		}

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void OnOkClick(object window)
		{
			var wnd = window as Window;

			if (string.IsNullOrEmpty(folderName))
			{
				var messageWindow = new MessageBoxWindow();
				messageWindow.ShowDialog(wnd,
					"Folder name is empty.",
					"Aras VS method plugin",
					MessageButtons.OK,
					MessageIcon.None);
			}
			else
			{
				wnd.DialogResult = true;
				wnd.Close();
			}
		}
	}
}
