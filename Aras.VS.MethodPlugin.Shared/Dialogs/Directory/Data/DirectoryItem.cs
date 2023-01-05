//------------------------------------------------------------------------------
// <copyright file="DirectoryItem.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Dialogs.Directory.Data
{
	public class DirectoryItem
	{
		public DirectoryItemType Type { get; set; }

		public string FullPath { get; set; }

		public string Name { get { return this.Type == DirectoryItemType.Drive ? this.FullPath : DirectoryStructure.GetFileFolderName(this.FullPath); } }
	}
}
