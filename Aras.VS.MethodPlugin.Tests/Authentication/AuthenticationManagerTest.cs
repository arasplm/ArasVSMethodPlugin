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

		private const string ProjectFullName = "testProjectFullName";
		private const string ServerUrl = "http://localhost/InnovatorServer";
		private const string Database = "InnovatorSolutions";
		private const string Login = "root";
		private IIOMWrapper iOMWrapper;

		[SetUp]
		public void Init()
		{
			messageManager = Substitute.For<MessageManager>();
			projectManager = Substitute.For<IProjectManager>();

			iOMWrapper = Substitute.For<IIOMWrapper>();
			iOMWrapper.ProjectFullName.Returns(ProjectFullName);

			authenticationManager = new AuthenticationManager(messageManager, projectManager, iOMWrapper);
		}

		[TestCase("innovator")]
		[TestCase("")]
		[TestCase(" p\u00e4ssword ")]
		[TestCase("0123456789abcdef0123456789abcdef")]
		[TestCase("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")]
		public void Login_PassesPlainTextPasswordToIom(string password)
		{
			((object)iOMWrapper.IomFactory_CreateWinAuthHttpServerConnection(ServerUrl, Database))
				.Returns<object>(new FakeServerConnection(new FakeLoginItem("Windows authentication failed")));
			SetupConnection(password, new FakeLoginItem());

			bool result = authenticationManager.Login("testProject", ProjectFullName, ServerUrl, Database, Login, password, null);

			Assert.IsTrue(result);
			Assert.AreEqual(Login, authenticationManager.InnovatorUser.userName);
			Assert.AreEqual(ServerUrl, authenticationManager.InnovatorUser.serverUrl);
			Assert.AreEqual(Database, authenticationManager.InnovatorUser.databaseName);
			Assert.AreEqual("testProject", authenticationManager.InnovatorUser.currentProjectName);
			iOMWrapper.Received(1).IomFactory_CreateWinAuthHttpServerConnection(ServerUrl, Database);
			iOMWrapper.Received(1).IomFactory_CreateHttpServerConnection(ServerUrl, Database, Login, password);
			iOMWrapper.ReceivedWithAnyArgs(1).IomFactory_CreateHttpServerConnection(default(string), default(string), default(string), default(string));
		}

		[Test]
		public void Login_WindowsAuthenticationSucceeds_DoesNotUsePasswordConnection()
		{
			((object)iOMWrapper.IomFactory_CreateWinAuthHttpServerConnection(ServerUrl, Database))
				.Returns<object>(new FakeServerConnection(new FakeLoginItem()));

			bool result = authenticationManager.Login("testProject", ProjectFullName, ServerUrl, Database, Login, "innovator", null);

			Assert.IsTrue(result);
			Assert.IsFalse(authenticationManager.InnovatorUser.IsEmpty());
			iOMWrapper.DidNotReceive().IomFactory_CreateHttpServerConnection(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
		}

		[TestCase(null, true)]
		[TestCase("", true)]
		[TestCase(Login, false)]
		public void InnovatorUser_IsEmpty_DependsOnUserName(string userName, bool expected)
		{
			var user = new InnovatorUser { userName = userName };

			Assert.AreEqual(expected, user.IsEmpty());
		}

		private void SetupConnection(string password, FakeLoginItem loginItem)
		{
			((object)iOMWrapper.IomFactory_CreateHttpServerConnection(ServerUrl, Database, Login, password))
				.Returns<object>(new FakeServerConnection(loginItem));
		}

		public class FakeServerConnection
		{
			private readonly FakeLoginItem loginItem;

			public FakeServerConnection(FakeLoginItem loginItem)
			{
				this.loginItem = loginItem;
			}

			public FakeLoginItem Login() => loginItem;

			public void Logout() { }
		}

		public class FakeLoginItem
		{
			private readonly string errorMessage;

			public FakeLoginItem(string errorMessage = null)
			{
				this.errorMessage = errorMessage;
			}

			public bool isError() => errorMessage != null;

			public string getErrorString() => errorMessage;
		}
	}
}