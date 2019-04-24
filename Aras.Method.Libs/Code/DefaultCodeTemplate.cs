//------------------------------------------------------------------------------
// <copyright file="DefaultCodeTemplate.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Code
{
	public class DefaultCodeTemplate
	{
		public string TempalteName { get; set; }

		public EventSpecificData EventDataType { get; set; }

		public string WrapperSourceCode { get; set; }

		public string SimpleSourceCode { get; set; }

		public string SimpleUnitTestsCode { get; set; }

		public string AdvancedSourceCode { get; set; }

		public string AdvancedUnitTestsCode { get; set; }
	}
}
