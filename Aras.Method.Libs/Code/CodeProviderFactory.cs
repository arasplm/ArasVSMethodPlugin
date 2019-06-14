//------------------------------------------------------------------------------
// <copyright file="CodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using EnvDTE;

namespace Aras.Method.Libs.Code
{
	public class CodeProviderFactory : ICodeProviderFactory
	{
		private readonly ICodeFormatter codeFormatter;
		private readonly MessageManager messageManager;
		private readonly IIOWrapper iOWrapper;

		public CodeProviderFactory(ICodeFormatter codeFormatter, MessageManager messageManager, IIOWrapper iOWrapper)
		{
			this.codeFormatter = codeFormatter ?? throw new ArgumentNullException(nameof(codeFormatter));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));
		}

		public ICodeItemProvider GetCodeItemProvider(string projectLanguageCode)
		{
			ICodeItemProvider codeItemProvider = null;
			if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageCSharp)
			{
				codeItemProvider = new CSharpCodeItemProvider(messageManager);
			}
			else
			{
				throw new NotSupportedException(this.messageManager.GetMessage("CurrentProjectTypeIsNotSupported"));
			}

			return codeItemProvider;
		}

		public ICodeProvider GetCodeProvider(string projectLanguageCode)
		{
			ICodeProvider codeProvider = null;
			if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageCSharp || projectLanguageCode == GlobalConsts.CSharp)
			{
				codeProvider = new CSharpCodeProvider(new CSharpCodeItemProvider(messageManager), codeFormatter, this.iOWrapper, this.messageManager);
			}
			else if (projectLanguageCode == CodeModelLanguageConstants.vsCMLanguageVB)
			{
				codeProvider = new VBCodeProvider();
			}
			else
			{
				throw new NotSupportedException(this.messageManager.GetMessage("CurrentProjectTypeIsNotSupported"));
			}

			return codeProvider;
		}
	}
}
