//------------------------------------------------------------------------------
// <copyright file="IIOWrapper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;

namespace Aras.VS.MethodPlugin
{
	public interface IIOWrapper
	{
		string EnvironmentGetFolderPath(Environment.SpecialFolder folder);
		bool DirectoryExists(string path);
		DirectoryInfo DirectoryCreateDirectory(string path);
		bool FileExists(string path);
		string PathCombine(string folder, string fileName);
		XmlDocument XmlDocumentLoad(string path);
		void XmlDocumentSave(XmlDocument xmlDocument, string fileName);
	}
}
