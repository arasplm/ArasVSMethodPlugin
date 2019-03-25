//------------------------------------------------------------------------------
// <copyright file="SelectPathViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Dialogs.Directory;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SelectPathViewModel : BaseViewModel
	{
		private readonly IDialogFactory dialogFactory;
		private readonly IIOWrapper iOWrapper;
		private ObservableCollection<DirectoryItemViewModel> directoryItems;
		private DirectoryItemViewModel selectDirectoryItem;
		private DirectoryItemType searchToLevel;
		private string selectedPath;

		public event Action<string> SelectionChanged;

		private ICommand newFolderCommand;
		private ICommand renameFolderCommand;
		private ICommand deleteFolderCommand;
		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand pathChangeCommand;

		public SelectPathViewModel(IDialogFactory dialogFactory, DirectoryItemType searchToLevel, IIOWrapper iOWrapper, string rootPath = "", string startPath = "", string fileExtantion = "")
		{
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));

			this.dialogFactory = dialogFactory;
			this.iOWrapper = iOWrapper;
			this.searchToLevel = searchToLevel;
			this.selectedPath = startPath;
			this.newFolderCommand = new RelayCommand<object>(OnNewFolderClick);
			this.renameFolderCommand = new RelayCommand<object>(OnRenameFolderClick);
			this.deleteFolderCommand = new RelayCommand<object>(OnDeleteFolderClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.pathChangeCommand = new RelayCommand<object>(OnPathChange);

			this.SelectionChanged += OnFolderPathChange;
			this.DirectoryItems = new ObservableCollection<DirectoryItemViewModel>();

			List<DirectoryItem> children;
			if (System.IO.Directory.Exists(rootPath))
			{
				children = new List<DirectoryItem> { new DirectoryItem() { FullPath = rootPath, Type = DirectoryItemType.Folder } };
			}
			else
			{
				children = DirectoryStructure.GetLogicalDrives();
			}

			foreach (DirectoryItem drive in children)
			{
				var childViewModel = new DirectoryItemViewModel(drive.FullPath, drive.Type, searchToLevel, fileExtantion);
				childViewModel.SelectDirectoryItem += OnSelectDirectoryItem;
				this.DirectoryItems.Add(childViewModel);
			}

			if (!string.IsNullOrEmpty(startPath) && (System.IO.Directory.Exists(startPath) || File.Exists(startPath)))
			{
				var pathList = startPath.Split(Path.DirectorySeparatorChar).ToList();
				Navigate(this.DirectoryItems, pathList);
			}
			//TODO: navigate to path
		}

		public void Navigate(IList<DirectoryItemViewModel> currentItems, List<string> pathList)
		{
			var currentPath = pathList.FirstOrDefault();
			if (!string.IsNullOrEmpty(currentPath))
			{

				var currentDirectory = currentItems.FirstOrDefault(di => di.Name.Replace(Path.DirectorySeparatorChar.ToString(), string.Empty) == currentPath);
				if (currentDirectory != null)
				{
					currentDirectory.Expand();
					currentDirectory.IsExpanded = true;
					currentDirectory.IsSelected = true;
					pathList.Remove(currentPath);
					Navigate(currentDirectory.Children, pathList);
				}
			}
		}

		#region Properties

		public ObservableCollection<DirectoryItemViewModel> DirectoryItems
		{
			get { return directoryItems; }
			set { directoryItems = value; }
		}

		public string SelectedPath
		{
			get { return selectedPath; }
			set { selectedPath = value; RaisePropertyChanged(nameof(SelectedPath)); }
		}

		#endregion

		public ICommand NewFolderCommand { get { return newFolderCommand; } }

		public ICommand RenameFolderCommand { get { return renameFolderCommand; } }

		public ICommand DeleteFolderCommand { get { return deleteFolderCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand PathChangeCommand { get { return pathChangeCommand; } }

		private void OnCloseCliked(object window)
		{
			var wnd = window as Window;
			wnd.Close();
		}

		private void OkCommandClick(object window)
		{
			Window wnd = window as Window;
			if (System.IO.Directory.Exists(SelectedPath) || File.Exists(SelectedPath))
			{
				wnd.DialogResult = true;
				wnd.Close();
			}
			else
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog("Selection wasn't found",
					"Select path for saving",
					MessageButtons.OK,
					MessageIcon.None);
			}
		}

		private void OnSelectDirectoryItem(DirectoryItemViewModel selectDirectoryItem)
		{
			this.selectDirectoryItem = selectDirectoryItem;
			var selectionChanged = SelectionChanged;
			if (selectionChanged != null)
			{
				selectionChanged(selectDirectoryItem.FullPath);
			}
		}

		private void OnFolderPathChange(string fullPath)
		{
			this.SelectedPath = fullPath;
		}

		private void OnNewFolderClick(object window)
		{
			var folderNameDialogAdapter = this.dialogFactory.GetFolderNameDialog();
			FolderNameDialogResult result = folderNameDialogAdapter.ShowDialog();
			if (result.DialogOperationResult == true)
			{
				string folderName = result.FolderName;

				string newFolderPath = Path.Combine(selectDirectoryItem.FullPath, folderName);
				int index = 1;

				while (this.iOWrapper.DirectoryExists(newFolderPath))
				{
					newFolderPath = Path.Combine(selectDirectoryItem.FullPath, $"{folderName} {index}");
					index++;
				}

				this.iOWrapper.DirectoryCreateDirectory(newFolderPath);

				if (selectDirectoryItem.IsExpanded)
				{
					var childViewModel = new DirectoryItemViewModel(newFolderPath, DirectoryItemType.Folder, searchToLevel);
					childViewModel.Parent = selectDirectoryItem;
					childViewModel.SelectDirectoryItem += OnSelectDirectoryItem;

					selectDirectoryItem.Children.Add(childViewModel);
					selectDirectoryItem.Children = new ObservableCollection<DirectoryItemViewModel>(selectDirectoryItem.Children.OrderBy(x => x.Name));
					selectDirectoryItem.IsSelected = false;
					childViewModel.IsSelected = true;
				}
				else
				{
					selectDirectoryItem.IsExpanded = true;

					var childViewModel = selectDirectoryItem.Children.FirstOrDefault(x => x.FullPath == newFolderPath);
					if (childViewModel != null)
					{
						selectDirectoryItem.IsSelected = false;
						childViewModel.IsSelected = true;
					}
				}
			}
		}

		private void OnRenameFolderClick(object window)
		{
			var folderNameDialogAdapter = this.dialogFactory.GetFolderNameDialog();
			FolderNameDialogResult result = folderNameDialogAdapter.ShowDialog();

			if (result.DialogOperationResult == true)
			{
				string newFolderName = result.FolderName;
				string newFullPath = Path.Combine(Path.GetDirectoryName(selectDirectoryItem.FullPath), newFolderName);

				if (string.Equals(selectDirectoryItem.FullPath, newFullPath))
				{
					return;
				}

				try
				{
					this.iOWrapper.DirectoryMove(selectDirectoryItem.FullPath, newFullPath);
					selectDirectoryItem.FullPath = newFullPath;
				}
				catch (Exception ex)
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					messageWindow.ShowDialog(ex.Message,
						"Aras VS method plugin",
						MessageButtons.OK,
						MessageIcon.Error);
				}
			}
		}

		private void OnDeleteFolderClick(object window)
		{
			string message = $"Are you sure you want to delete the {selectDirectoryItem.Name} folder?";
			var messageWindow = this.dialogFactory.GetMessageBoxWindow();
			var dialogResult = messageWindow.ShowDialog(message,
				"Aras VS method plugin",
				MessageButtons.OKCancel,
				MessageIcon.Warning);

			if (dialogResult == MessageDialogResult.OK)
			{
				try
				{
					this.iOWrapper.DirectoryDelete(selectDirectoryItem.FullPath, true);
					if (selectDirectoryItem.Parent != null)
					{
						selectDirectoryItem.Parent.Update();
					}
				}
				catch (Exception ex)
				{
					var errorWindow = this.dialogFactory.GetMessageBoxWindow();
					errorWindow.ShowDialog(ex.Message,
						"Aras VS method plugin",
						MessageButtons.OK,
						MessageIcon.Error);
				}
			}
		}

		private void OnPathChange(object window)
		{
			if (this.iOWrapper.FileExists(selectedPath) || this.iOWrapper.DirectoryExists(selectedPath))
			{
				Navigate(DirectoryItems, selectedPath.Split(Path.DirectorySeparatorChar).ToList());
			}
			else
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog("File or Folder were not found",
					"Saving method to local package",
					MessageButtons.OK,
					MessageIcon.None);
			}
		}
	}
}
