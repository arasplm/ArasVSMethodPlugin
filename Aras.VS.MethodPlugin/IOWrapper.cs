//------------------------------------------------------------------------------
// <copyright file="IOWrapper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Xml;

namespace Aras.VS.MethodPlugin
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

		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public string PathCombine(string folder, string fileName)
		{
			return Path.Combine(folder, fileName);
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
