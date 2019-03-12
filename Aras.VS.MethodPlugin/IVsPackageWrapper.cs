using System;

namespace Aras.VS.MethodPlugin
{
	public interface IVsPackageWrapper
	{
		object GetGlobalService(Type type);
	}
}
