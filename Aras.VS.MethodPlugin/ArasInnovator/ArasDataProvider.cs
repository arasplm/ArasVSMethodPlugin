//------------------------------------------------------------------------------
// <copyright file="ArasDataProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.VS.MethodPlugin.Authentication;

namespace Aras.VS.MethodPlugin.ArasInnovator
{
	public class ArasDataProvider : IArasDataProvider
	{
		private readonly IAuthenticationManager authenticationManager;

		public ArasDataProvider(IAuthenticationManager authenticationManager)
		{
			if (authenticationManager == null)
			{
				throw new ArgumentNullException(nameof(authenticationManager));
			}

			this.authenticationManager = authenticationManager;
		}

		public MethodItemTypeInfo GetMethodItemTypeInfo()
		{
			dynamic item = authenticationManager.InnovatorInstance.newItem("ItemType", "get");
			item.setProperty("keyed_name", "Method");
			item.setAttribute("levels", "1");
			item = item.apply();

			if (item.isError())
			{
				throw new Exception(item.getErrorString());
			}

			return new MethodItemTypeInfo(item);
		}
	}
}
