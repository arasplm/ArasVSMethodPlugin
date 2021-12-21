//------------------------------------------------------------------------------
// <copyright file="VsPackageWrapper.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin
{
	public class VsPackageWrapper : IVsPackageWrapper
	{
		public object GetGlobalService(Type type)
		{
			return Package.GetGlobalService(type);
		}
	}
}
