using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin
{
	internal class VisualStudioServiceProvider : IVisualStudioServiceProvider
	{
		private readonly AsyncPackage package;

		public VisualStudioServiceProvider(AsyncPackage package)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
		}

		public object GetService(Type serviceType)
		{
			return package.GetServiceAsync(serviceType).GetAwaiter().GetResult();
		}

		public async Task<object> GetServiceAsync(Type serviceType)
		{
			return await package.GetServiceAsync(serviceType);
		}
	}
}
