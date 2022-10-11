//------------------------------------------------------------------------------
// <copyright file="DirectoryStructure.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;

namespace Aras.VS.MethodPlugin.Dialogs.Directory
{
	public static class DirectoryStructure
	{
		public static List<DirectoryItem> GetLogicalDrives()
		{
			return System.IO.Directory.GetLogicalDrives().Select(drive => new DirectoryItem { FullPath = drive, Type = DirectoryItemType.Drive }).ToList();
		}

		public static List<DirectoryItem> GetDirectoryContents(string fullPath)
		{
			var items = new List<DirectoryItem>();

			try
			{
				var dirs = System.IO.Directory.GetDirectories(fullPath);
				if (dirs.Length > 0)
				{
					items.AddRange(dirs.Select(dir => new DirectoryItem { FullPath = dir, Type = DirectoryItemType.Folder }));
				}
			}
			catch { }

			try
			{
				var fs = System.IO.Directory.GetFiles(fullPath);
				if (fs.Length > 0)
				{
					items.AddRange(fs.Select(file => new DirectoryItem { FullPath = file, Type = DirectoryItemType.File }));
				}
			}
			catch { }

			return items;
		}

		#region Helpers

		public static string GetFileFolderName(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return string.Empty;
			}

			var normalizedPath = path.Replace('/', '\\');
			var lastIndex = normalizedPath.LastIndexOf('\\');

			if (lastIndex <= 0)
			{
				return path;
			}

			return path.Substring(lastIndex + 1);
		}

		#endregion
	}
}
