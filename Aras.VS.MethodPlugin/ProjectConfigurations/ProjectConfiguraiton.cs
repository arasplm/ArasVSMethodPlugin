//------------------------------------------------------------------------------
// <copyright file="ProjectConfiguraiton.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;

namespace Aras.VS.MethodPlugin.ProjectConfigurations
{
	public class ProjectConfiguraiton : IProjectConfiguraiton
	{
		private const string serverMethodsFolderName = "ServerMethods";
		private string lastSelectedSearchTypeInOpenFromPackage;

		public ProjectConfiguraiton()
		{
			Connections = new List<ConnectionInfo>();
			MethodInfos = new List<MethodInfo>();
			LastSavedSearch = new Dictionary<string, List<PropertyInfo>>();
		}

		public void Update(IProjectManager projectManager)
		{
			ProjectItems serverMethods = projectManager.ServerMethodFolderItems;
			if (serverMethods.Count == 0)
			{
				this.MethodInfos.Clear();
				return;
			}

			var updatedMethodInfos = new List<MethodInfo>();

			foreach (MethodInfo methodInfo in MethodInfos)
			{
				if (!projectManager.IsMethodExist(methodInfo.MethodName))
				{
					continue;
				}

				MethodInfo updatedMethodInfo;
				if (methodInfo is PackageMethodInfo)
				{
					updatedMethodInfo = new PackageMethodInfo((PackageMethodInfo)methodInfo);
				}
				else
				{
					updatedMethodInfo = new MethodInfo(methodInfo);
				}

				updatedMethodInfo.PartialClasses.Clear();

				foreach (string partialClassPath in methodInfo.PartialClasses)
				{
					if (!projectManager.IsFileExist(partialClassPath))
					{
						continue;
					}

					updatedMethodInfo.PartialClasses.Add(partialClassPath);
				}

				updatedMethodInfos.Add(updatedMethodInfo);
			}

			this.MethodInfos = updatedMethodInfos;
		}

		public void UpdateMethodInfo(string methodName, ProjectItem projectItem, string oldName)
		{
			MethodInfo methodInformation = this.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInformation == null)
			{
				return;
			}

			string newFilePath = projectItem.FileNames[0];
			int index = newFilePath.IndexOf(serverMethodsFolderName);
			if (index == -1)
			{
				return;
			}

			string newPartialPath = newFilePath.Substring(index + serverMethodsFolderName.Length + 1);
			newPartialPath = Path.ChangeExtension(newPartialPath, null);

			string oldPartialPath = Path.Combine(Path.GetDirectoryName(newPartialPath), oldName);
			oldPartialPath = Path.ChangeExtension(oldPartialPath, null);

			for (int i = 0; i < methodInformation.PartialClasses.Count; i++)
			{
				if (string.Equals(methodInformation.PartialClasses[i], oldPartialPath, StringComparison.InvariantCultureIgnoreCase))
				{
					methodInformation.PartialClasses[i] = newPartialPath;

					string oldPartialAttribute = oldPartialPath.Substring(oldPartialPath.IndexOf("\\") + 1).Replace("\\", "/");
					string newPartialAttribute = newPartialPath.Substring(newPartialPath.IndexOf("\\") + 1).Replace("\\", "/");
					Encoding witoutBom = new UTF8Encoding(true);
					string code = File.ReadAllText(newFilePath, witoutBom);
					code = code.Replace($"[PartialPath(\"{oldPartialAttribute}\")]", $"[PartialPath(\"{newPartialAttribute}\")]");
					File.WriteAllText(newFilePath, code, witoutBom);

					break;
				}
			}
		}

		public void RemoveFromMethodInfo(string methodName, ProjectItem projectItem)
		{
			string filePath = projectItem.FileNames[0];
			int index = filePath.IndexOf(serverMethodsFolderName);
			if (index == 1)
			{
				return;
			}

			string partialPath = filePath.Substring(index + serverMethodsFolderName.Length + 1);
			partialPath = Path.ChangeExtension(partialPath, null);

			string fodlerPath = methodName + "\\";


			if (fodlerPath == partialPath)
			{
				MethodInfos.RemoveAll(x => x.MethodName == methodName);
				return;
			}
			MethodInfo methodInfos = this.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);

			List<string> classesForRemoving = new List<string>();

			foreach (var partialClass in methodInfos.PartialClasses)
			{
				if (partialClass.IndexOf(partialPath) == 0)
				{
					classesForRemoving.Add(partialClass);
				}
			}
			classesForRemoving.ForEach(cfr => methodInfos.PartialClasses.Remove(cfr));
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
