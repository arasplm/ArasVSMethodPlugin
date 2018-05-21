//------------------------------------------------------------------------------
// <copyright file="ToolStripSystemRendererHook.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Forms;

namespace Aras.VS.MethodPlugin.Dialogs
{
	// https://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
	public class ToolStripSystemRendererHook : ToolStripSystemRenderer
	{
		public ToolStripSystemRendererHook() { }

		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
			//base.OnRenderToolStripBorder(e);
		}
	}
}
