using System;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin
{
	class VsPackageWrapper : IVsPackageWrapper
	{
		public object GetGlobalService(Type type)
		{
			return Package.GetGlobalService(type);
		}
	}
}
