//------------------------------------------------------------------------------
// <copyright file="IIOWrapper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Aras.VS.MethodPlugin
{
	public interface IIOWrapper
	{
		string EnvironmentGetFolderPath(Environment.SpecialFolder folder);
		bool DirectoryExists(string path);
		DirectoryInfo DirectoryCreateDirectory(string path);
		bool FileExists(string path);
		string FileReadAllText(string path, UTF8Encoding encoding);
		string PathCombine(string folder, string fileName);
		bool PathHasExtension(string path);
		char PathDirectorySeparatorChar();
		char PathAltDirectorySeparatorChar();
		XmlDocument XmlDocumentLoad(string path);
		void XmlDocumentSave(XmlDocument xmlDocument, string fileName);
	}
}
