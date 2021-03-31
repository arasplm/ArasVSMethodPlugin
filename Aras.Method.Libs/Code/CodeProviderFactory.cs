//------------------------------------------------------------------------------
// <copyright file="CodeProviderFactory.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Aras.Method.Libs.Code
{
	public class CodeProviderFactory : ICodeProviderFactory
	{
		private readonly ICodeFormatter codeFormatter;
		private readonly MessageManager messageManager;
		private readonly IIOWrapper iOWrapper;

		private const string vsCMLanguageCSharp = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
		private const string vsCMLanguageVB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

		public CodeProviderFactory(ICodeFormatter codeFormatter, MessageManager messageManager, IIOWrapper iOWrapper)
		{
			this.codeFormatter = codeFormatter ?? throw new ArgumentNullException(nameof(codeFormatter));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));
		}

		public ICodeItemProvider GetCodeItemProvider(string projectLanguageCode)
		{
			ICodeItemProvider codeItemProvider = null;
			if (projectLanguageCode == vsCMLanguageCSharp)
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
			if (projectLanguageCode == vsCMLanguageCSharp || projectLanguageCode == GlobalConsts.CSharp)
			{
				codeProvider = new CSharpCodeProvider(new CSharpCodeItemProvider(messageManager), codeFormatter, this.iOWrapper, this.messageManager);
			}
			else if (projectLanguageCode == vsCMLanguageVB)
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
