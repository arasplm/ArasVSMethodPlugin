//------------------------------------------------------------------------------
// <copyright file="MessageMenager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin
{
	internal class MessageManager
	{
		private static Dictionary<string, string> messages;

		static MessageManager()
		{
			messages = new Dictionary<string, string>
			{
				{ "AuthenticationFailedFor", "Authentication failed for {0}." }
			};
		}

		internal static string GetMessage(string key)
		{
			var message = string.Empty;
			messages.TryGetValue(key, out message);
			return message;
		}
	}
}
