//------------------------------------------------------------------------------
// <copyright file="MoveToViewModel.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs.Code;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class MoveToViewModel : BaseViewModel
	{
		private readonly IDialogFactory dialogFactory;
		private readonly string methodFolderPath;

		private string codeItemPath = string.Empty;
		private string fileName = string.Empty;

		private ObservableCollection<CodeType> codeTypes;
		private CodeType selectedCodeType; 

		private ICommand fileFolderBrowserCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public MoveToViewModel(IDialogFactory dialogFactory, string methodFolderPath, SyntaxNode node)
		{
			this.dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
			this.methodFolderPath = methodFolderPath;

			this.fileFolderBrowserCommand = new RelayCommand<object>(OnFileFolderBrowserCommandClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			InitializeCodeTypes(node);
		}

		#region Properties

		public string CodeItemPath
		{
			get { return codeItemPath; }
			set
			{
				codeItemPath = value;
				RaisePropertyChanged(nameof(CodeItemPath));
			}
		}

		public string FileName
		{
			get { return fileName; }
			set
			{
				fileName = value;
				RaisePropertyChanged(nameof(FileName));
			}
		}

		public string SelectedFullPath
		{
			get
			{
				string selectedFullPath = Path.Combine(codeItemPath, fileName);
				if (!Path.HasExtension(selectedFullPath))
				{
					selectedFullPath += ".cs";
				}

				return selectedFullPath;
			}
		}

		public ObservableCollection<CodeType> CodeTypes
		{
			get { return codeTypes; }
			set
			{
				codeTypes = value;
				RaisePropertyChanged(nameof(CodeTypes));
			}
		}

		public CodeType SelectedCodeType
		{
			get { return this.selectedCodeType; }
			set
			{
				this.selectedCodeType = value;
				RaisePropertyChanged(nameof(SelectedCodeType));
			}
		}

		private bool IsEnabledOkButton(object obj)
		{
			return !string.IsNullOrEmpty(this.codeItemPath) && !string.IsNullOrEmpty(this.fileName);
		}

		#endregion

		#region Commands

		public ICommand FileFolderBrowserCommand
		{
			get { return fileFolderBrowserCommand; }
		}

		public ICommand OkCommand
		{
			get { return okCommand; }
		}

		public ICommand CloseCommand
		{
			get { return closeCommand; }
		}

		#endregion

		private void InitializeCodeTypes(SyntaxNode node)
		{
			if (node is MethodDeclarationSyntax)
			{
				CodeTypes = new ObservableCollection<CodeType>() { CodeType.Partial };
			}
			else
			{
				CodeTypes = new ObservableCollection<CodeType>() { CodeType.Partial, CodeType.External };
			}
		}

		private void OnFileFolderBrowserCommandClick(object window)
		{
			var selectPathDialog = dialogFactory.GetSelectPathDialog(DirectoryItemType.File, methodFolderPath);
			var selectPathDialogResult = selectPathDialog.ShowDialog();
			if (selectPathDialogResult.DialogOperationResult == true)
			{
				FileAttributes attributes = File.GetAttributes(selectPathDialogResult.SelectedFullPath);
				if (attributes.HasFlag(FileAttributes.Directory))
				{
					this.CodeItemPath = selectPathDialogResult.SelectedFullPath;
				}
				else
				{
					this.CodeItemPath = Path.GetDirectoryName(selectPathDialogResult.SelectedFullPath);
					this.FileName = Path.GetFileNameWithoutExtension(selectPathDialogResult.SelectedFullPath);
				}
			}
		}

		private void OkCommandClick(object view)
		{
			var window = view as Window;
			window.DialogResult = true;
			window.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}
	}
}
