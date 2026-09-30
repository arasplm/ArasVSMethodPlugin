//------------------------------------------------------------------------------
// <copyright file="IIOMWrapper.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin
{
	public interface IIOMWrapper
	{
		string ProjectFullName { get; }

		dynamic IomFactory_CreateHttpServerConnection(string serverUrl, string databaseName, string login, string password);

		dynamic Innovator_Ctor(dynamic serverConnection);

		dynamic IomFactory_CreateHttpServerConnection(string innovatorURL);

		dynamic IomFactory_CreateWinAuthHttpServerConnection(string innovatorURL, string databaseName);
	}
}
