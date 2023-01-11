//------------------------------------------------------------------------------
// <copyright file="VisualStudioCodeFormatter.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.Method.Libs.Code;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Aras.VS.MethodPlugin.Code
{
	public class VisualStudioCodeFormatter : ICodeFormatter
	{
		private readonly IProjectManager projectManager;

		public VisualStudioCodeFormatter(IProjectManager projectManager)
		{
			this.projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
		}

		public string Format(string code)
		{
			SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
			SyntaxNode node = tree.GetRoot();
			node = Formatter.Format(node, this.projectManager.VisualStudioWorkspace);
			return node.ToString();
		}
	}
}
