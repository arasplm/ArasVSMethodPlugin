using System;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin
{
	public interface IVisualStudioServiceProvider
	{
		object GetService(Type serviceType);

		// Task<object> GetServiceAsync(Type serviceType);
	}
}
