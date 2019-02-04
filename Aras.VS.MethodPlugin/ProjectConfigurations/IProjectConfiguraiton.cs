using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin.ProjectConfigurations
{
	public interface IProjectConfiguraiton
	{

		string LastSelectedDir { get; set; }

		string LastSelectedMfFile { get; set; }

		bool UseVSFormatting { get; set; }

		string LastSelectedSearchTypeInOpenFromPackage{ get; set; }

		List<ConnectionInfo> Connections { get; }

		List<MethodInfo> MethodInfos { get; set; }

		Dictionary<string, List<PropertyInfo>> LastSavedSearch { get; set; }

		void RemoveFromMethodInfo(string methodName, ProjectItem projectItem);

		void UpdateMethodInfo(string methodName, ProjectItem projectItem, string oldName);

		void Update(IProjectManager projectManager);

		void AddConnection(ConnectionInfo connection);

		void AddMethodInfo(MethodInfo methodInfo);
	}
}
