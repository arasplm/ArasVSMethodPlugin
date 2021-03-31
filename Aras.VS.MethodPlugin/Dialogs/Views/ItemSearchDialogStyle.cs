//------------------------------------------------------------------------------
// <copyright file="ItemSearchDialogStyle.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.PlatformUI;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public partial class ItemSearchDialog
	{
		private const int BORDER_WIDTH = 2;
		private const int WM_NCLBUTTONDOWN = 0xA1;
		private const int HT_CAPTION = 0x2;
		private const int HTBOTTOMLEFT = 16;
		private const int HTBOTTOMRIGHT = 17;
		private const int HTLEFT = 10;
		private const int HTRIGHT = 11;
		private const int HTBOTTOM = 15;

		private readonly Cursor[] _resizeCursors = { Cursors.SizeNESW, Cursors.SizeWE, Cursors.SizeNWSE, Cursors.SizeWE, Cursors.SizeNS };
		private ResizeDirection _resizeDir;

		private enum ResizeDirection
		{
			BottomLeft,
			Left,
			Right,
			BottomRight,
			Bottom,
			None
		}

		private Color borderBrush;

		public void InitializeDialogStyle()
		{
			// title info
			this.titlePictureBox.Image = this.Icon.ToBitmap();
			this.titleLabel.Text = this.Text;

			// dialog move
			this.MouseDown += ItemSearchDialog_MouseDown;
			this.titlePictureBox.MouseDown += ItemSearchDialog_MouseDown;
			this.titleLabel.MouseDown += ItemSearchDialog_MouseDown;

			// dialog border
			this.Paint += ItemSearchDialog_Paint;
			this.Resize += ItemSearchDialog_Resize;

			// close button
			this.closeButton.FlatAppearance.BorderColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionBrushKey);
			this.closeButton.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionBrushKey);
			this.closeButton.ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ButtonTextBrushKey);

			this.closeButton.Click += (sender, e) => { this.Close(); };
			this.closeButton.MouseEnter += closeButton_MouseEnter;
			this.closeButton.MouseLeave += closeButton_MouseLeave;

			// buttons
			var buttons = new List<Button>
			{
				this.okButton,
				this.cancelButton,
			};

			foreach (Button button in buttons)
			{
				button.FlatAppearance.BorderColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBorderBrushKey);
				button.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
				button.ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxTextBrushKey);

				button.MouseEnter += button_MouseEnter;
				button.MouseLeave += button_MouseLeave;
			}

			ApplyVSTheme();

			this.contentPanel.MouseMove += (sender, args) => { OnMouseMove(args); };
		}

		private void ApplyVSTheme()
		{
			this.tools.Renderer = new ToolStripSystemRendererHook();

			this.borderBrush = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveBuildingBorderBrushKey);
			this.titleLabel.ForeColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionTextBrushKey);
			this.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionBrushKey);

			this.contentPanel.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
			this.tools.BackColor = Color.Transparent;

			Color controlForeground = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxTextBrushKey);
			Color controlBackground = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxBackgroundBrushKey);

			this.itemTypeLabel.ForeColor = controlForeground;
			this.savedSearchesLabel.ForeColor = controlForeground;
			this.toolStripLabel2.ForeColor = controlForeground;
		}

		private void closeButton_MouseEnter(object sender, EventArgs e)
		{
			this.closeButton.FlatAppearance.BorderColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowButtonHoverActiveBorderBrushKey);
			this.closeButton.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowButtonHoverActiveBrushKey);
		}

		private void closeButton_MouseLeave(object sender, EventArgs e)
		{
			this.closeButton.FlatAppearance.BorderColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionBrushKey);
			this.closeButton.BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.MainWindowActiveCaptionBrushKey);
		}

		private void button_MouseEnter(object sender, EventArgs e)
		{
			((Button)sender).BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ComboBoxMouseOverBorderBrushKey);
		}

		private void button_MouseLeave(object sender, EventArgs e)
		{
			((Button)sender).BackColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundBrushKey);
		}

		[System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();

		private void ItemSearchDialog_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private void ItemSearchDialog_Paint(object sender, PaintEventArgs e)
		{
			ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, borderBrush, ButtonBorderStyle.Solid);
		}

		private void ItemSearchDialog_Resize(object sender, EventArgs e)
		{
			Invalidate();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (DesignMode)
			{
				return;
			}

			if (e.Button == MouseButtons.Left)
			{
				ResizeForm(_resizeDir);
			}

			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (DesignMode)
			{
				return;
			}

			var isChildUnderMouse = GetChildAtPoint(e.Location) != null;

			if (e.Location.X < BORDER_WIDTH && e.Location.Y > Height - BORDER_WIDTH && !isChildUnderMouse)
			{
				_resizeDir = ResizeDirection.BottomLeft;
				Cursor = Cursors.SizeNESW;
			}
			else if (e.Location.X < BORDER_WIDTH && !isChildUnderMouse)
			{
				_resizeDir = ResizeDirection.Left;
				Cursor = Cursors.SizeWE;
			}
			else if (e.Location.X > Width - BORDER_WIDTH && e.Location.Y > Height - BORDER_WIDTH && !isChildUnderMouse)
			{
				_resizeDir = ResizeDirection.BottomRight;
				Cursor = Cursors.SizeNWSE;
			}
			else if (e.Location.X > Width - BORDER_WIDTH && !isChildUnderMouse)
			{
				_resizeDir = ResizeDirection.Right;
				Cursor = Cursors.SizeWE;
			}
			else if (e.Location.Y > Height - BORDER_WIDTH && !isChildUnderMouse)
			{
				_resizeDir = ResizeDirection.Bottom;
				Cursor = Cursors.SizeNS;
			}
			else
			{
				_resizeDir = ResizeDirection.None;
				if (_resizeCursors.Contains(Cursor))
				{
					Cursor = Cursors.Default;
				}
			}
		}

		private void ResizeForm(ResizeDirection direction)
		{
			if (DesignMode) return;
			var dir = -1;
			switch (direction)
			{
				case ResizeDirection.BottomLeft:
					dir = HTBOTTOMLEFT;
					break;
				case ResizeDirection.Left:
					dir = HTLEFT;
					break;
				case ResizeDirection.Right:
					dir = HTRIGHT;
					break;
				case ResizeDirection.BottomRight:
					dir = HTBOTTOMRIGHT;
					break;
				case ResizeDirection.Bottom:
					dir = HTBOTTOM;
					break;
			}

			ReleaseCapture();
			if (dir != -1)
			{
				SendMessage(Handle, WM_NCLBUTTONDOWN, dir, 0);
			}
		}
	}
}
