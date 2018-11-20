//------------------------------------------------------------------------------
// <copyright file="ICodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Code
{
	public interface ICodeProviderFactory
	{
		ICodeProvider GetCodeProvider(string projectLanguageCode, IProjectConfiguraiton projectConfiguration);
	}
}
