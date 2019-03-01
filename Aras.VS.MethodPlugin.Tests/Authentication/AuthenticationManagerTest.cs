using Aras.VS.MethodPlugin.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Authentication
{
	[TestFixture]
	public class AuthenticationManagerTest
	{
		private AuthenticationManager authenticationManager;

		[SetUp]
		public void Init()
		{
			authenticationManager = new AuthenticationManager();
			var wrapper = Substitute.For<IIOMWrapper>();
		}
	}
}