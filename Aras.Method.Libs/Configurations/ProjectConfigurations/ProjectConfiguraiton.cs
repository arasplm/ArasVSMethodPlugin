//------------------------------------------------------------------------------
// <copyright file="ProjectConfiguraiton.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EnvDTE;

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class ProjectConfiguraiton : IProjectConfiguraiton
	{
		private string lastSelectedSearchTypeInOpenFromPackage;

		public ProjectConfiguraiton()
		{
			Connections = new List<ConnectionInfo>();
			MethodInfos = new List<MethodInfo>();
			LastSavedSearch = new Dictionary<string, List<PropertyInfo>>();
		}

		public void CleanUp()
		{
			LastSelectedDir = string.Empty;
			LastSelectedMfFile = string.Empty;
			UseVSFormatting = default(bool);
			LastSelectedSearchTypeInOpenFromPackage = string.Empty;
			Connections.Clear();
			MethodInfos.Clear();
			LastSavedSearch.Clear();
			MethodsFolderPath = string.Empty;
			MethodConfigPath = string.Empty;
			IOMFilePath = string.Empty;
		}

		public void UpdateMethodInfo(string methodName, ProjectItem projectItem, string oldName)
		{
			MethodInfo methodInformation = this.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInformation == null)
			{
				return;
			}

			string newFilePath = projectItem.FileNames[0];
			int index = newFilePath.IndexOf(MethodsFolderPath);
			if (index == -1)
			{
				return;
			}

			string methodFolderPath = Path.Combine(MethodsFolderPath, methodInformation.Package.MethodFolderPath);

			string newAttributePath = newFilePath.Substring(index + methodFolderPath.Length);
			newAttributePath = Path.ChangeExtension(newAttributePath, null);
			string oldAttributePath = Path.Combine(Path.GetDirectoryName(newAttributePath), oldName);
			oldAttributePath = Path.ChangeExtension(oldAttributePath, null);

			string oldPartialAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newPartialAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string oldExternalAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newExternalAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");

			Encoding witoutBom = new UTF8Encoding(true);
			string code = File.ReadAllText(newFilePath, witoutBom);
			code = code.Replace($"[PartialPath(\"{oldPartialAttribute}\"", $"[PartialPath(\"{newPartialAttribute}\"");
			code = code.Replace($"[ExternalPath(\"{oldExternalAttribute}\"", $"[ExternalPath(\"{newExternalAttribute}\"");
			File.WriteAllText(newFilePath, code, witoutBom);
		}

		public void RemoveFromMethodInfo(string methodName, ProjectItem projectItem)
		{
			MethodInfo methodInfos = this.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInfos == null)
			{
				return;
			}

			string methodFolderPath = Path.Combine(MethodsFolderPath, methodInfos.Package.MethodFolderPath);
			string filePath = projectItem.FileNames[0];
			int index = filePath.IndexOf(MethodsFolderPath);
			if (index == 1)
			{
				return;
			}

			string attributePath = filePath.Substring(index + methodFolderPath.Length);
			attributePath = Path.ChangeExtension(attributePath, null);

			string fodlerPath = methodName + "\\";

			if (fodlerPath == attributePath)
			{
				MethodInfos.RemoveAll(x => x.MethodName == methodName);
				return;
			}
		}

		public string LastSelectedDir { get; set; }

		public string LastSelectedMfFile { get; set; }

		public bool UseVSFormatting { get; set; }

		public string LastSelectedSearchTypeInOpenFromPackage
		{
			get => lastSelectedSearchTypeInOpenFromPackage;
			set
			{
				if (value == null)
				{
					lastSelectedSearchTypeInOpenFromPackage = string.Empty;
				}
				else
				{
					lastSelectedSearchTypeInOpenFromPackage = value;
				}
			}
		}

		public List<ConnectionInfo> Connections { get; private set; }

		public List<MethodInfo> MethodInfos { get; set; }

		public Dictionary<string, List<PropertyInfo>> LastSavedSearch { get; set; }

		public string MethodsFolderPath { get; set; }

		public string MethodConfigPath { get; set; }

		public string IOMFilePath { get; set; }

		public void AddConnection(ConnectionInfo connection)
		{
			int count = Connections.Count;
			for (int i = 0; i < count; i++)
			{
				if (Connections[i].ServerUrl == connection.ServerUrl)
				{
					Connections.RemoveAt(i);
					count--;
					i--;
					continue;
				}

				Connections[i].LastConnection = false;
			}

			this.Connections.Add(connection);
		}

		public void AddMethodInfo(MethodInfo methodInfo)
		{
			MethodInfos.RemoveAll(x => x.MethodName == methodInfo.MethodName);
			MethodInfos.Add(methodInfo);
		}
	}
}
