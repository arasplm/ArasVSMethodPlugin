using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.SolutionManagement;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Authentication
{
	[TestFixture]
	public class AuthenticationManagerTest
	{
		private MessageManager messageManager;
		private IProjectManager projectManager;
		private AuthenticationManager authenticationManager;

		[SetUp]
		public void Init()
		{
			messageManager = Substitute.For<MessageManager>();
			projectManager = Substitute.For<IProjectManager>();

			authenticationManager = new AuthenticationManager(messageManager, projectManager);
		}
	}
}