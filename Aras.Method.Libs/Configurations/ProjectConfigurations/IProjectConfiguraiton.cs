//------------------------------------------------------------------------------
// <copyright file="IProjectConfiguraiton.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using EnvDTE;

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public interface IProjectConfiguraiton
	{
		string LastSelectedDir { get; set; }

		string LastSelectedMfFile { get; set; }

		bool UseVSFormatting { get; set; }

		string LastSelectedSearchTypeInOpenFromPackage { get; set; }

		List<ConnectionInfo> Connections { get; }

		List<MethodInfo> MethodInfos { get; set; }

		Dictionary<string, List<PropertyInfo>> LastSavedSearch { get; set; }

		string MethodsFolderPath { get; set; }

		string MethodConfigPath { get; set; }

		string IOMFilePath { get; set; }

		void RemoveFromMethodInfo(string methodName, ProjectItem projectItem);

		void UpdateMethodInfo(string methodName, ProjectItem projectItem, string oldName);

		void AddConnection(ConnectionInfo connection);

		void AddMethodInfo(MethodInfo methodInfo);

		void CleanUp();
	}
}
