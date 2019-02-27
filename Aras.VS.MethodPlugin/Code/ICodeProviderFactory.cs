//------------------------------------------------------------------------------
// <copyright file="ICodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Code
{
	public interface ICodeProviderFactory
	{
		ICodeItemProvider GetCodeItemProvider(string projectLanguageCode);

		ICodeProvider GetCodeProvider(string projectLanguageCode, IProjectConfiguraiton projectConfiguration);
	}
}
