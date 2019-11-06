//------------------------------------------------------------------------------
// <copyright file="ProjectConfiguraiton.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

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
			UseCommonProjectStructure = default(bool);
			LastSelectedSearchTypeInOpenFromPackage = string.Empty;
			Connections.Clear();
			MethodInfos.Clear();
			LastSavedSearch.Clear();
			MethodsFolderPath = string.Empty;
			MethodConfigPath = string.Empty;
			IOMFilePath = string.Empty;
		}

		public string LastSelectedDir { get; set; }

		public string LastSelectedMfFile { get; set; }

		public bool UseVSFormatting { get; set; }

		public bool UseCommonProjectStructure { get; set; }

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
