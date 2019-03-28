//------------------------------------------------------------------------------
// <copyright file="IMessageManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin
{
	public interface IMessageManager
	{
		string GetMessage(string key);

		string GetMessage(string key, params string[] args);
	}
}
