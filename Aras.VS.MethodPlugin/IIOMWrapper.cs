//------------------------------------------------------------------------------
// <copyright file="IIOMWrapper.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin
{
	public interface IIOMWrapper
	{
		string ProjectFullName { get; }

		dynamic Innovator_ScalcMD5(string password);

		dynamic IomFactory_CreateHttpServerConnection(string serverUrl, string databaseName, string login, string passwordHash);

		dynamic Innovator_Ctor(dynamic serverConnection);

		dynamic IomFactory_CreateHttpServerConnection(string innovatorURL);

		dynamic IomFactory_CreateWinAuthHttpServerConnection(string innovatorURL, string databaseName);
	}
}
