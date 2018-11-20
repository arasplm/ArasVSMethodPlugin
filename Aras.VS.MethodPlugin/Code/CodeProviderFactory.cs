//------------------------------------------------------------------------------
// <copyright file="CodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;

namespace Aras.VS.MethodPlugin.Code
{
	public class CodeProviderFactory : ICodeProviderFactory
	{
		private readonly IProjectManager projectManager;
		private readonly DefaultCodeProvider defaultCodeProvider;

		public CodeProviderFactory(IProjectManager projectManager, DefaultCodeProvider defaultCodeProvider)
		{
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (defaultCodeProvider == null) throw new ArgumentNullException(nameof(defaultCodeProvider));

			this.projectManager = projectManager;
			this.defaultCodeProvider = defaultCodeProvider;
		}

		public ICodeProvider GetCodeProvider(string projectLanguageCode, IProjectConfiguraiton projectConfiguration)
		{
			string projectLanguage = string.Empty;
			ICodeProvider codeProvider = null;
			if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageCSharp)
			{
				codeProvider = new CSharpCodeProvider(projectManager, projectConfiguration, defaultCodeProvider);
			}
			else if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageVB)
			{
				codeProvider = new VBCodeProvider(projectManager.SelectedProject, projectConfiguration);
			}
			else
			{
				throw new NotSupportedException("Current project type is not supported");
			}

			return codeProvider;
		}
	}
}
