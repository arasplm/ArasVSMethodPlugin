using Aras.Method.Libs;
using Aras.VS.MethodPlugin.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Authentication
{
	[TestFixture]
	public class AuthenticationManagerTest
	{
		private MessageManager messageManager;
		private AuthenticationManager authenticationManager;

		[SetUp]
		public void Init()
		{
			messageManager = Substitute.For<MessageManager>();
			authenticationManager = new AuthenticationManager(messageManager);
			var wrapper = Substitute.For<IIOMWrapper>();
		}
	}
}