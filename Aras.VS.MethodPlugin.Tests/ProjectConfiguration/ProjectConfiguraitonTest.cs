using System.Collections.Generic;
using System.Linq;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.ProjectConfiguration
{
	[TestFixture]
	public class ProjectConfiguraitonTest
	{
		private IProjectConfiguraiton projectConfiguraiton;

		[SetUp]
		public void Init()
		{
			projectConfiguraiton = new ProjectConfiguraiton();
		}

		[Test]
		public void AddConnection_ShouldAddConnection()
		{
			//Arrange 
			var connect = new ConnectionInfo()
			{
				Database = string.Empty,
				LastConnection = false,
				Login = string.Empty,
				ServerUrl = string.Empty
			};
			var currentConnectionCount = projectConfiguraiton.Connections.Count;

			//Act
			projectConfiguraiton.AddConnection(connect);

			//Assert
			Assert.AreEqual(currentConnectionCount + 1, projectConfiguraiton.Connections.Count);
		}


		[Test]
		public void AddMethodInfo_ShouldAddMethodInfo()
		{
			//Arrange 
			var methodInfo = new MethodInfo
			{
				EventData = Aras.Method.Libs.Code.EventSpecificData.None,
				ExecutionAllowedToId = "A73B655731924CD0B027E4F4D5FCC0A9",
				ExecutionAllowedToKeyedName = "World",
				InnovatorMethodConfigId = "6D5D2A114135409D82561DC1C422C87F",
				InnovatorMethodId = "6B6E21E655CA4A1093FB9E970463F061",
				MethodComment = "",
				MethodLanguage = "C#",
				MethodName = "Method1",
				MethodType = "server",
				Package = new PackageInfo("MSO_Standard"),
				TemplateName = "CSharp"
			};
			var currentMethodInfoCount = projectConfiguraiton.MethodInfos.Count;

			//Act
			projectConfiguraiton.AddMethodInfo(methodInfo);

			//Assert
			Assert.AreEqual(currentMethodInfoCount + 1, projectConfiguraiton.MethodInfos.Count);
		}
	}
}
