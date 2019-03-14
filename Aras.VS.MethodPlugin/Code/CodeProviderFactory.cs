//------------------------------------------------------------------------------
// <copyright file="CodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;

namespace Aras.VS.MethodPlugin.Code
{
	public class CodeProviderFactory : ICodeProviderFactory
	{
		private readonly IProjectManager projectManager;
		private readonly DefaultCodeProvider defaultCodeProvider;
		private readonly IIOWrapper iOWrapper;
		private readonly IDialogFactory dialogFactory;

		public CodeProviderFactory(IProjectManager projectManager, DefaultCodeProvider defaultCodeProvider, IIOWrapper iOWrapper, IDialogFactory dialogFactory)
		{
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (defaultCodeProvider == null) throw new ArgumentNullException(nameof(defaultCodeProvider));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));

			this.projectManager = projectManager;
			this.defaultCodeProvider = defaultCodeProvider;
			this.iOWrapper = iOWrapper;
			this.dialogFactory = dialogFactory;
		}

		public ICodeItemProvider GetCodeItemProvider(string projectLanguageCode)
		{
			ICodeItemProvider codeItemProvider = null;
			if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageCSharp)
			{
				codeItemProvider = new CSharpCodeItemProvider();
			}
			else
			{
				throw new NotSupportedException("Current project type is not supported");
			}

			return codeItemProvider;
		}

		public ICodeProvider GetCodeProvider(string projectLanguageCode, IProjectConfiguraiton projectConfiguration)
		{
			string projectLanguage = string.Empty;
			ICodeProvider codeProvider = null;
			if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageCSharp)
			{
				codeProvider = new CSharpCodeProvider(projectManager, projectConfiguration, defaultCodeProvider, new CSharpCodeItemProvider(), iOWrapper, this.dialogFactory);
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
