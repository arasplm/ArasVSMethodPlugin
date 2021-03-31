//------------------------------------------------------------------------------
// <copyright file="ICodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.Method.Libs.Code
{
	public interface ICodeProviderFactory
	{
		ICodeItemProvider GetCodeItemProvider(string projectLanguageCode);

		ICodeProvider GetCodeProvider(string projectLanguageCode);
	}
}
