//------------------------------------------------------------------------------
// <copyright file="GlobalConfiguration.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using Aras.Method.Libs;

namespace Aras.VS.MethodPlugin.Configurations
{
	public class GlobalConfiguration : IGlobalConfiguration
	{
		private const string projectFolderName = "Aras Innovator Method Plugin";
		private const string configFileName = "config.xml";
		private const string configuration = "configuration";
		private const string userCodeTemplates = "userCodeTemplates";
		private const string userCodeTemplate = "userCodeTemplate";

		private readonly IIOWrapper iIOWrapper;

		protected XmlDocument configXmlDocument = null;
		private XmlElement configurationXmlElement = null;
		private XmlElement userCodeTemplatesXmlElement = null;

		public GlobalConfiguration(IIOWrapper iIOWrapper)
		{
			this.iIOWrapper = iIOWrapper ?? throw new ArgumentNullException(nameof(iIOWrapper));

			Load();
		}

		private string ConfigFilePath
		{
			get
			{
				string folderPath = this.iIOWrapper.PathCombine(this.iIOWrapper.EnvironmentGetFolderPath(Environment.SpecialFolder.MyDocuments), projectFolderName);
				if (!this.iIOWrapper.DirectoryExists(folderPath))
				{
					this.iIOWrapper.DirectoryCreateDirectory(folderPath);
				}

				return this.iIOWrapper.PathCombine(folderPath, configFileName);
			}
		}

		public void Load()
		{
			string filePath = this.ConfigFilePath;
			if (!this.iIOWrapper.FileExists(filePath))
			{
				this.configXmlDocument = new XmlDocument();
				this.configurationXmlElement = this.configXmlDocument.CreateElement(configuration);
				this.userCodeTemplatesXmlElement = this.configXmlDocument.CreateElement(userCodeTemplates);

				this.configurationXmlElement.AppendChild(this.userCodeTemplatesXmlElement);
				this.configXmlDocument.AppendChild(this.configurationXmlElement);
			}
			else
			{
				this.configXmlDocument = this.iIOWrapper.XmlDocumentLoad(filePath);
				this.configurationXmlElement = (XmlElement)this.configXmlDocument.SelectSingleNode(configuration);
				this.userCodeTemplatesXmlElement = (XmlElement)this.configurationXmlElement.SelectSingleNode(userCodeTemplates);
			}
		}

		public void Save()
		{
			this.iIOWrapper.XmlDocumentSave(this.configXmlDocument, this.ConfigFilePath);
		}

		public List<string> GetUserCodeTemplatesPaths()
		{
			List<string> resultList = new List<string>();
			foreach (XmlNode node in this.userCodeTemplatesXmlElement.ChildNodes)
			{
				resultList.Add(node.InnerText);
			}

			return resultList;
		}

		public void AddUserCodeTemplatePath(string path)
		{
			foreach (XmlNode node in this.userCodeTemplatesXmlElement.ChildNodes)
			{
				if (string.Equals(node.InnerText, path, StringComparison.InvariantCultureIgnoreCase))
				{
					return;
				}
			}

			XmlElement userCodeTemplateXmlElement = this.configXmlDocument.CreateElement(userCodeTemplate);
			userCodeTemplateXmlElement.InnerText = path;
			this.userCodeTemplatesXmlElement.AppendChild(userCodeTemplateXmlElement);
		}

		public void RemoveUserCodeTemplatePath(string path)
		{
			foreach (XmlNode node in this.userCodeTemplatesXmlElement.ChildNodes)
			{
				if (string.Equals(node.InnerText, path, StringComparison.InvariantCultureIgnoreCase))
				{
					this.userCodeTemplatesXmlElement.RemoveChild(node);
				}
			}
		}
	}
}
