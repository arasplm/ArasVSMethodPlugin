//------------------------------------------------------------------------------
// <copyright file="ICodeItemProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.Method.Libs.Code
{
	public enum CodeType
	{
		Partial,
		External
	}

	public enum CodeElementType
	{
		Interface,
		Class,
		Struct,
		Method,
		Enum,
		Custom
	}

	public interface ICodeItemProvider
	{
		List<CodeElementType> GetSupportedCodeElementTypes(CodeType type);
		string GetCodeElementTypeTemplate(CodeType codeType, CodeElementType codeElementType);
	}
}