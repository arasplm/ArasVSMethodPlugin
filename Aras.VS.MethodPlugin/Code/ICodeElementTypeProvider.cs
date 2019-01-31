//------------------------------------------------------------------------------
// <copyright file="ICodeElementTypeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Code
{
	public enum CodeElementType
	{
		Interface,
		Class,
		Struct,
		Method,
		Enum,
		Custom
	}

	public interface ICodeElementTypeProvider
	{
		string GetCodeElementTypeTemplate(CodeElementType type);
	}
}