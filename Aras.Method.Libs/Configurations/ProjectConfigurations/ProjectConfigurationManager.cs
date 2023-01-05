//------------------------------------------------------------------------------
// <copyright file="ProjectConfigurationManager.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class ProjectConfigurationManager : IProjectConfigurationManager
	{
		private const string DefaultMethodsFolderPath = "ServerMethods";
		private const string DefaultMethodConfigFilePath = "method-config.xml";
		private const string DefaultIOMDllFilePath = @"ArasLibs\IOM.dll";

		private readonly MessageManager messageManager;

		public ProjectConfigurationManager(MessageManager messageManager)
		{
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
		}

		public IProjectConfiguraiton CurrentProjectConfiguraiton { get; } = new ProjectConfiguraiton();

		public void Load(string configFilePath)
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(configFilePath);

				this.CurrentProjectConfiguraiton.CleanUp();

				MapXmlDocToProjectConfig(this.CurrentProjectConfiguraiton, doc);
			}
			catch (Exception ex)
			{
				throw new Exception(this.messageManager.GetMessage("errorWhileLoadingProjectConfigFile"), ex);
			}
		}

		public void Save(string configFilePath)
		{
			try
			{
				XmlDocument xmlDoc = MapProjectConfigToXmlDoc(this.CurrentProjectConfiguraiton);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Encoding = new UTF8Encoding(true);
				settings.Indent = true;
				settings.IndentChars = "\t";
				settings.NewLineOnAttributes = false;
				settings.OmitXmlDeclaration = true;
				using (XmlWriter xmlWriter = XmlWriter.Create(configFilePath, settings))
				{
					xmlDoc.Save(xmlWriter);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(this.messageManager.GetMessage("errorWhileSavingProjectConfigFile"), ex);
			}
		}

		private XmlDocument MapProjectConfigToXmlDoc(IProjectConfiguraiton configuration)
		{
			//TODO: Refactoring: move to constant. All hardcoded sting should be constants if using more then 2 times.
			string configTempalte = "<?xml version = '1.0\' encoding = 'utf-8' ?><projectinfo><lastSelectedDir></lastSelectedDir><lastSelectedMfFile></lastSelectedMfFile><connections></connections><MethodsFolderPath></MethodsFolderPath><MethodConfigPath></MethodConfigPath><IOMFilePath></IOMFilePath><methods></methods><lastSavedSearch></lastSavedSearch><useVSFormatting></useVSFormatting><UseCommonProjectStructure></UseCommonProjectStructure><OpenFromPackageLastSearchType></OpenFromPackageLastSearchType></projectinfo>";

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(configTempalte);
			var lastSelectedDir = xmlDoc.SelectSingleNode("projectinfo/lastSelectedDir");
			lastSelectedDir.InnerText = configuration.LastSelectedDir;

			var lastSelectedMfFile = xmlDoc.SelectSingleNode("projectinfo/lastSelectedMfFile");
			lastSelectedMfFile.InnerText = configuration.LastSelectedMfFile;

			var usedVSFormat = xmlDoc.SelectSingleNode("projectinfo/useVSFormatting");
			usedVSFormat.InnerText = configuration.UseVSFormatting.ToString();

			var useCommonProjectStructureXmlNode = xmlDoc.SelectSingleNode("projectinfo/UseCommonProjectStructure");
			useCommonProjectStructureXmlNode.InnerText = configuration.UseCommonProjectStructure.ToString();

			var openFromPackageLastSearchType = xmlDoc.SelectSingleNode("projectinfo/OpenFromPackageLastSearchType");
			openFromPackageLastSearchType.InnerText = configuration.LastSelectedSearchTypeInOpenFromPackage;

			XmlNode methodsFolderPathNode = xmlDoc.SelectSingleNode("projectinfo/MethodsFolderPath");
			methodsFolderPathNode.InnerText = configuration.MethodsFolderPath;

			XmlNode methodConfigPathNode = xmlDoc.SelectSingleNode("projectinfo/MethodConfigPath");
			methodConfigPathNode.InnerText = configuration.MethodConfigPath;

			XmlNode IOMFilePathNode = xmlDoc.SelectSingleNode("projectinfo/IOMFilePath");
			IOMFilePathNode.InnerText = configuration.IOMFilePath;

			var connectionInfoXmlNode = xmlDoc.SelectSingleNode("projectinfo/connections");
			foreach (var connectionInfo in configuration.Connections)
			{
				XmlElement connectionInfoNode = xmlDoc.CreateElement("connectionInfo");

				XmlElement serverUrl = xmlDoc.CreateElement("serverUrl");
				serverUrl.InnerText = connectionInfo.ServerUrl;
				connectionInfoNode.AppendChild(serverUrl);

				XmlElement databaseName = xmlDoc.CreateElement("databaseName");
				databaseName.InnerText = connectionInfo.Database;
				connectionInfoNode.AppendChild(databaseName);

				XmlElement login = xmlDoc.CreateElement("login");
				login.InnerText = connectionInfo.Login;
				connectionInfoNode.AppendChild(login);

				XmlElement lastConnection = xmlDoc.CreateElement("lastConnection");
				lastConnection.InnerText = connectionInfo.LastConnection.ToString();
				connectionInfoNode.AppendChild(lastConnection);

				connectionInfoXmlNode.AppendChild(connectionInfoNode);
			}

			var methodInfoXmlNode = xmlDoc.SelectSingleNode("projectinfo/methods");
			foreach (var methodInfo in configuration.MethodInfos)
			{
				XmlElement metohdInfoNode = xmlDoc.CreateElement("methodInfo");

				XmlElement innovatorMethodConfigId = xmlDoc.CreateElement("configId");
				innovatorMethodConfigId.InnerText = methodInfo.InnovatorMethodConfigId;
				metohdInfoNode.AppendChild(innovatorMethodConfigId);

				XmlElement innovatorMethodId = xmlDoc.CreateElement("id");
				innovatorMethodId.InnerText = methodInfo.InnovatorMethodId;
				metohdInfoNode.AppendChild(innovatorMethodId);

				XmlElement methodName = xmlDoc.CreateElement("methodName");
				methodName.InnerText = methodInfo.MethodName;
				metohdInfoNode.AppendChild(methodName);

				XmlElement methodType = xmlDoc.CreateElement("methodType");
				methodType.InnerText = methodInfo.MethodType;
				metohdInfoNode.AppendChild(methodType);

				XmlElement comments = xmlDoc.CreateElement("comments");
				comments.InnerText = methodInfo.MethodComment;
				metohdInfoNode.AppendChild(comments);

				XmlElement language = xmlDoc.CreateElement("language");
				language.InnerText = methodInfo.MethodLanguage;
				metohdInfoNode.AppendChild(language);

				XmlElement templateName = xmlDoc.CreateElement("templateName");
				templateName.InnerText = methodInfo.TemplateName;
				metohdInfoNode.AppendChild(templateName);

				XmlElement packageName = xmlDoc.CreateElement("packageName");
				packageName.InnerText = methodInfo.Package.Name;
				metohdInfoNode.AppendChild(packageName);

				XmlElement eventData = xmlDoc.CreateElement("eventData");
				eventData.InnerText = methodInfo.EventData.ToString();
				metohdInfoNode.AppendChild(eventData);

				XmlElement executionAllowedTo = xmlDoc.CreateElement("executionAllowedTo");
				executionAllowedTo.InnerText = methodInfo.ExecutionAllowedToId;

				XmlAttribute executionAllowedToKeyedName = xmlDoc.CreateAttribute("keyedName");
				executionAllowedToKeyedName.InnerText = methodInfo.ExecutionAllowedToKeyedName;
				executionAllowedTo.Attributes.Append(executionAllowedToKeyedName);
				metohdInfoNode.AppendChild(executionAllowedTo);
				if (methodInfo is PackageMethodInfo)
				{
					var packageMethodInfo = (PackageMethodInfo)methodInfo;
					XmlElement manifestFileName = xmlDoc.CreateElement("manifestFileName");
					manifestFileName.InnerText = packageMethodInfo.ManifestFileName;
					metohdInfoNode.AppendChild(manifestFileName);
				}

				methodInfoXmlNode.AppendChild(metohdInfoNode);
			}

			var lastSavedSearchXmlNode = xmlDoc.SelectSingleNode("projectinfo/lastSavedSearch");
			foreach (var lastSavedSearch in configuration.LastSavedSearch)
			{
				string itemTypeName = lastSavedSearch.Key;
				List<PropertyInfo> properties = lastSavedSearch.Value;

				XmlElement searchItemNode = xmlDoc.CreateElement("searchItem");

				XmlAttribute itemTypeNameXmlAttribute = xmlDoc.CreateAttribute("itemTypeName");
				itemTypeNameXmlAttribute.InnerText = itemTypeName;
				searchItemNode.Attributes.Append(itemTypeNameXmlAttribute);

				foreach (PropertyInfo property in properties)
				{
					if (property.IsReadonly || string.IsNullOrEmpty(property.PropertyValue))
					{
						continue;
					}

					XmlElement propertyXmlElement = xmlDoc.CreateElement("property");

					XmlElement propertyName = xmlDoc.CreateElement("propertyName");
					propertyName.InnerText = property.PropertyName;
					propertyXmlElement.AppendChild(propertyName);

					XmlElement propertyValue = xmlDoc.CreateElement("propertyValue");
					propertyValue.InnerText = property.PropertyValue;
					propertyXmlElement.AppendChild(propertyValue);

					searchItemNode.AppendChild(propertyXmlElement);
					lastSavedSearchXmlNode.AppendChild(searchItemNode);
				}
			}

			return xmlDoc;
		}

		private IProjectConfiguraiton MapXmlDocToProjectConfig(IProjectConfiguraiton projectConfiguration, XmlDocument xmlDoc)
		{
			projectConfiguration.LastSelectedDir = xmlDoc.SelectSingleNode("projectinfo/lastSelectedDir")?.InnerText;
			projectConfiguration.LastSelectedMfFile = xmlDoc.SelectSingleNode("projectinfo/lastSelectedMfFile")?.InnerText;
			bool.TryParse(xmlDoc.SelectSingleNode("projectinfo/useVSFormatting")?.InnerText, out bool isUsedVSFormatting);
			projectConfiguration.UseVSFormatting = isUsedVSFormatting;
			bool.TryParse(xmlDoc.SelectSingleNode("projectinfo/UseCommonProjectStructure")?.InnerText, out bool isUsedCommonProjectStructure);
			projectConfiguration.UseCommonProjectStructure = isUsedCommonProjectStructure;

			projectConfiguration.LastSelectedSearchTypeInOpenFromPackage = xmlDoc.SelectSingleNode("projectinfo/OpenFromPackageLastSearchType")?.InnerText;
			projectConfiguration.MethodsFolderPath = xmlDoc.SelectSingleNode("projectinfo/MethodsFolderPath")?.InnerText ?? DefaultMethodsFolderPath;
			projectConfiguration.MethodConfigPath = xmlDoc.SelectSingleNode("projectinfo/MethodConfigPath")?.InnerText ?? DefaultMethodConfigFilePath;
			projectConfiguration.IOMFilePath = xmlDoc.SelectSingleNode("projectinfo/IOMFilePath")?.InnerText ?? DefaultIOMDllFilePath;

			var connectionInfoXmlNodes = xmlDoc.SelectNodes("projectinfo/connections/connectionInfo");
			foreach (XmlNode connectionInfoNode in connectionInfoXmlNodes)
			{
				var connectionInfo = new ConnectionInfo();
				connectionInfo.ServerUrl = connectionInfoNode.SelectSingleNode("serverUrl").InnerText;
				connectionInfo.Database = connectionInfoNode.SelectSingleNode("databaseName").InnerText;
				connectionInfo.Login = connectionInfoNode.SelectSingleNode("login").InnerText;

				bool value;
				if (bool.TryParse(connectionInfoNode.SelectSingleNode("lastConnection")?.InnerText, out value))
				{
					connectionInfo.LastConnection = value;
				}

				projectConfiguration.AddConnection(connectionInfo);
			}
			var methodInfoXmlNodes = xmlDoc.SelectNodes("projectinfo/methods/methodInfo");
			foreach (XmlNode methodInfoNode in methodInfoXmlNodes)
			{
				XmlNode manifestFileNameNode = methodInfoNode.SelectSingleNode("manifestFileName");
				MethodInfo methodInfo;
				if (manifestFileNameNode != null)
				{
					methodInfo = new PackageMethodInfo()
					{
						ManifestFileName = manifestFileNameNode.InnerText
					};
				}
				else
				{
					methodInfo = new MethodInfo();
				}

				methodInfo.InnovatorMethodConfigId = methodInfoNode.SelectSingleNode("configId").InnerText;
				methodInfo.InnovatorMethodId = methodInfoNode.SelectSingleNode("id").InnerText;
				methodInfo.MethodName = methodInfoNode.SelectSingleNode("methodName").InnerText;
				methodInfo.MethodType = methodInfoNode.SelectSingleNode("methodType").InnerText;
				methodInfo.MethodComment = methodInfoNode.SelectSingleNode("comments")?.InnerText;
				methodInfo.MethodLanguage = methodInfoNode.SelectSingleNode("language").InnerText;
				methodInfo.TemplateName = methodInfoNode.SelectSingleNode("templateName").InnerText;
				methodInfo.Package = new PackageInfo(methodInfoNode.SelectSingleNode("packageName").InnerText);

				EventSpecificData eventData;
				if (Enum.TryParse(methodInfoNode.SelectSingleNode("eventData")?.InnerText, out eventData))
				{
					methodInfo.EventData = eventData;
				}
				else
				{
					methodInfo.EventData = EventSpecificData.None;
				}

				XmlNode executionAllowedTo = methodInfoNode.SelectSingleNode("executionAllowedTo");
				XmlAttribute executionAllowedKeyedName = executionAllowedTo.Attributes["keyedName"];
				methodInfo.ExecutionAllowedToId = executionAllowedTo.InnerText;
				methodInfo.ExecutionAllowedToKeyedName = executionAllowedKeyedName.InnerText;

				projectConfiguration.MethodInfos.Add(methodInfo);
			}

			var searchItemNodes = xmlDoc.SelectNodes("projectinfo/lastSavedSearch/searchItem");
			foreach (XmlNode searchItemNode in searchItemNodes)
			{
				string itemTypeName = searchItemNode.Attributes["itemTypeName"]?.InnerText;
				if (string.IsNullOrEmpty(itemTypeName))
					continue;
				var properties = new List<PropertyInfo>();
				var propertyNodes = searchItemNode.SelectNodes("property");

				foreach (XmlNode propertyNode in propertyNodes)
				{
					var savedSearch = new PropertyInfo();
					savedSearch.PropertyName = propertyNode.SelectSingleNode("propertyName").InnerText;
					savedSearch.PropertyValue = propertyNode.SelectSingleNode("propertyValue").InnerText;

					properties.Add(savedSearch);
				}


				projectConfiguration.LastSavedSearch.Add(itemTypeName, properties);
			}

			return projectConfiguration;
		}
	}
}
