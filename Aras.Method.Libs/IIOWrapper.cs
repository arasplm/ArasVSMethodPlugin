//------------------------------------------------------------------------------
// <copyright file="IIOWrapper.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Aras.Method.Libs
{
	public interface IIOWrapper
	{
		string EnvironmentGetFolderPath(Environment.SpecialFolder folder);
		bool DirectoryExists(string path);
		DirectoryInfo DirectoryCreateDirectory(string path);
		string[] DirectoryGetFiles(string path);
		string[] DirectoryGetFiles(string path, string searchPattern);
		string[] DirectoryGetFiles(string path, string searchPattern, SearchOption searchOption);
		void DirectoryMove(string pathFrom, string pathTo);
		void DirectoryDelete(string path, bool recursive);
		bool FileExists(string path);
		string FileReadAllText(string path, Encoding encoding);
		void FileDelete(string path);
		string[] FileReadAllLine(string excludePath);
		void WriteAllTextIntoFile(string path, string text, Encoding encoding);
		string PathGetFileNameWithoutExtension(string fileName);
		string ReadAllText(string path, Encoding encoding);
		string PathCombine(string folder, string fileName);
		string PathCombine(string path1, string path2, string path3);
		string PathGetFullPath(string path);
		bool PathHasExtension(string path);
		char PathDirectorySeparatorChar();
		char PathAltDirectorySeparatorChar();
		string PathGetFileName(string path);
		string PathChangeExtension(string path, string extension);
		string GetFileNameWithoutExtension(string path);
		string PathGetDirectoryName(string path);
		bool PathIsPathRooted(string path);
		XmlDocument XmlDocumentLoad(string path);
		void XmlDocumentSave(XmlDocument xmlDocument, string fileName);
	}
}
