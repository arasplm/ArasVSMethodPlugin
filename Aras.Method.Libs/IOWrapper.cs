//------------------------------------------------------------------------------
// <copyright file="IOWrapper.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Aras.Method.Libs
{
	public class IOWrapper : IIOWrapper
	{
		public string EnvironmentGetFolderPath(Environment.SpecialFolder folder)
		{
			return Environment.GetFolderPath(folder);
		}

		public bool DirectoryExists(string path)
		{
			return Directory.Exists(path);
		}

		public DirectoryInfo DirectoryCreateDirectory(string path)
		{
			return Directory.CreateDirectory(path);
		}

		public string[] DirectoryGetFiles(string path)
		{
			return Directory.GetFiles(path);
		}

		public string[] DirectoryGetFiles(string path, string searchPattern)
		{
			return Directory.GetFiles(path, searchPattern);
		}

		public string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return Directory.GetFiles(path, searchPattern, searchOption);
		}

		public void DirectoryMove(string sourceDirName, string destDirName)
		{
			Directory.Move(sourceDirName, destDirName);
		}

		public void DirectoryDelete(string path, bool recursive)
		{
			Directory.Delete(path, recursive);
		}

		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public string FileReadAllText(string path, Encoding encoding)
		{
			return File.ReadAllText(path, encoding);
		}

		public void FileDelete(string path)
		{
			File.Delete(path);
		}

		public string[] FileReadAllLine(string excludePath)
		{
			return File.ReadAllLines(excludePath);
		}

		public void WriteAllTextIntoFile(string path, string text, Encoding encoding)
		{
			File.WriteAllText(path, text, encoding);
		}

		public string ReadAllText(string path, Encoding encoding)
		{
			return File.ReadAllText(path, encoding);
		}

		public string PathGetFileNameWithoutExtension(string fileName)
		{
			return Path.GetFileNameWithoutExtension(fileName);
		}

		public string PathCombine(string folder, string fileName)
		{
			return Path.Combine(folder, fileName);
		}

		public string PathCombine(string path1, string path2, string path3)
		{
			return Path.Combine(path1, path2, path3);
		}

		public bool PathHasExtension(string path)
		{
			return Path.HasExtension(path);
		}

		public char PathDirectorySeparatorChar()
		{
			return Path.DirectorySeparatorChar;
		}

		public char PathAltDirectorySeparatorChar()
		{
			return Path.AltDirectorySeparatorChar;
		}

		public string PathGetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		public string PathGetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		public string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		public string PathGetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		public bool PathIsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		public string PathChangeExtension(string path, string extension)
		{
			return Path.ChangeExtension(path, extension);
		}

		public XmlDocument XmlDocumentLoad(string path)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(path);
			return xmlDocument;
		}

		public void XmlDocumentSave(XmlDocument xmlDocument, string fileName)
		{
			xmlDocument.Save(fileName);
		}
	}
}
