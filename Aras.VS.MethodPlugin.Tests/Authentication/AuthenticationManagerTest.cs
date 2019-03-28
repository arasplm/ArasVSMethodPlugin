using Aras.VS.MethodPlugin.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Authentication
{
	[TestFixture]
	public class AuthenticationManagerTest
	{
		private IMessageManager messageManager;
		private AuthenticationManager authenticationManager;

		[SetUp]
		public void Init()
		{
			messageManager = Substitute.For<IMessageManager>();
			authenticationManager = new AuthenticationManager(messageManager);
			var wrapper = Substitute.For<IIOMWrapper>();
		}
	}
}