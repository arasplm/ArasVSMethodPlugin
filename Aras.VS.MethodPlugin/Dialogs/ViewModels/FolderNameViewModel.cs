//------------------------------------------------------------------------------
// <copyright file="FolderNameViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Input;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class FolderNameViewModel
	{
		private readonly IDialogFactory dialogFactory;
		private readonly IMessageManager messageManager;

		private string folderName;

		private ICommand okCommand;
		private ICommand closeCommand;

		public FolderNameViewModel(IDialogFactory dialogFactory, IMessageManager messageManager)
		{
			this.dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));

			this.okCommand = new RelayCommand<object>(OnOkClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
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
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(messageManager.GetMessage("FolderNameIsEmpty"),
					messageManager.GetMessage("ArasVSMethodPlugin"),
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
