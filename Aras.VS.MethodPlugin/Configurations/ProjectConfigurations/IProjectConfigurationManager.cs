//------------------------------------------------------------------------------
// <copyright file="IProjectConfigurationManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin.Configurations.ProjectConfigurations
{
	public interface IProjectConfigurationManager
	{
		IProjectConfiguraiton Load(string configFilePath);

		void Save(string configFilePath, IProjectConfiguraiton configuration);
	}
}
