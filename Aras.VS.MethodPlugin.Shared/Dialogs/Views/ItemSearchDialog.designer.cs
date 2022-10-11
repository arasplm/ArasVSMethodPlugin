//------------------------------------------------------------------------------
// <copyright file="ItemSearchDialog.designer.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	partial class ItemSearchDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		[ExcludeFromCodeCoverage]
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		[ExcludeFromCodeCoverage]
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ItemSearchDialog));
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
			this.contentPanel = new System.Windows.Forms.Panel();
			this.partsGrid = new Aras.VS.MethodPlugin.Dialogs.Views.KeyDownEnabledGrid();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.tools = new System.Windows.Forms.ToolStrip();
			this.runSearchBtn = new System.Windows.Forms.ToolStripButton();
			this.newSearchBtn = new System.Windows.Forms.ToolStripButton();
			this.savedSearchesSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.itemTypeLabel = new System.Windows.Forms.ToolStripLabel();
			this.itemTypeBox = new System.Windows.Forms.ToolStripComboBox();
			this.savedSearchesLabel = new System.Windows.Forms.ToolStripLabel();
			this.savedSearchesBox = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.tbPageSize = new System.Windows.Forms.ToolStripTextBox();
			this.getPreviousPage = new System.Windows.Forms.ToolStripButton();
			this.getNextPage = new System.Windows.Forms.ToolStripButton();
			this.titlePictureBox = new System.Windows.Forms.PictureBox();
			this.titleLabel = new System.Windows.Forms.Label();
			this.closeButton = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.contentPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.partsGrid)).BeginInit();
			this.tools.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.titlePictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
			// 
			// toolStripTextBox1
			// 
			this.toolStripTextBox1.Name = "toolStripTextBox1";
			this.toolStripTextBox1.Size = new System.Drawing.Size(40, 27);
			// 
			// contentPanel
			// 
			this.contentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.contentPanel.Controls.Add(this.partsGrid);
			this.contentPanel.Controls.Add(this.okButton);
			this.contentPanel.Controls.Add(this.cancelButton);
			this.contentPanel.Controls.Add(this.tools);
			this.contentPanel.Location = new System.Drawing.Point(4, 28);
			this.contentPanel.Name = "contentPanel";
			this.contentPanel.Size = new System.Drawing.Size(829, 423);
			this.contentPanel.TabIndex = 15;
			// 
			// partsGrid
			// 
			this.partsGrid.AllowUserToAddRows = false;
			this.partsGrid.AllowUserToDeleteRows = false;
			this.partsGrid.AllowUserToOrderColumns = true;
			this.partsGrid.AllowUserToResizeRows = false;
			this.partsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.partsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.partsGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
			this.partsGrid.Location = new System.Drawing.Point(0, 27);
			this.partsGrid.MultiSelect = false;
			this.partsGrid.Name = "partsGrid";
			this.partsGrid.RowHeadersVisible = false;
			this.partsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.partsGrid.Size = new System.Drawing.Size(829, 346);
			this.partsGrid.TabIndex = 18;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.okButton.Location = new System.Drawing.Point(664, 394);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 16;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelButton.Location = new System.Drawing.Point(745, 394);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 17;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// tools
			// 
			this.tools.AllowMerge = false;
			this.tools.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.tools.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runSearchBtn,
            this.newSearchBtn,
            this.savedSearchesSeparator,
            this.itemTypeLabel,
            this.itemTypeBox,
            this.savedSearchesLabel,
            this.savedSearchesBox,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.tbPageSize,
            this.getPreviousPage,
            this.getNextPage});
			this.tools.Location = new System.Drawing.Point(0, 0);
			this.tools.Name = "tools";
			this.tools.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.tools.Size = new System.Drawing.Size(829, 27);
			this.tools.TabIndex = 15;
			this.tools.Text = "toolStrip1";
			// 
			// runSearchBtn
			// 
			this.runSearchBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.runSearchBtn.Image = ((System.Drawing.Image)(resources.GetObject("runSearchBtn.Image")));
			this.runSearchBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.runSearchBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.runSearchBtn.Name = "runSearchBtn";
			this.runSearchBtn.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.runSearchBtn.Size = new System.Drawing.Size(24, 24);
			this.runSearchBtn.Text = "Run Search";
			this.runSearchBtn.ToolTipText = "Run Search";
			// 
			// newSearchBtn
			// 
			this.newSearchBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.newSearchBtn.Image = ((System.Drawing.Image)(resources.GetObject("newSearchBtn.Image")));
			this.newSearchBtn.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.newSearchBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newSearchBtn.Name = "newSearchBtn";
			this.newSearchBtn.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.newSearchBtn.Size = new System.Drawing.Size(24, 24);
			this.newSearchBtn.Text = "Clear Search Criteria";
			// 
			// savedSearchesSeparator
			// 
			this.savedSearchesSeparator.Name = "savedSearchesSeparator";
			this.savedSearchesSeparator.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.savedSearchesSeparator.Size = new System.Drawing.Size(6, 27);
			this.savedSearchesSeparator.Visible = false;
			// 
			// itemTypeLabel
			// 
			this.itemTypeLabel.Name = "itemTypeLabel";
			this.itemTypeLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.itemTypeLabel.Size = new System.Drawing.Size(60, 24);
			this.itemTypeLabel.Text = "Item type:";
			this.itemTypeLabel.Visible = false;
			// 
			// itemTypeBox
			// 
			this.itemTypeBox.BackColor = System.Drawing.SystemColors.Control;
			this.itemTypeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.itemTypeBox.Name = "itemTypeBox";
			this.itemTypeBox.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.itemTypeBox.Size = new System.Drawing.Size(121, 27);
			this.itemTypeBox.Visible = false;
			// 
			// savedSearchesLabel
			// 
			this.savedSearchesLabel.Name = "savedSearchesLabel";
			this.savedSearchesLabel.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.savedSearchesLabel.Size = new System.Drawing.Size(89, 24);
			this.savedSearchesLabel.Text = "Saved searches:";
			this.savedSearchesLabel.Visible = false;
			// 
			// savedSearchesBox
			// 
			this.savedSearchesBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.savedSearchesBox.Name = "savedSearchesBox";
			this.savedSearchesBox.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
			this.savedSearchesBox.Size = new System.Drawing.Size(121, 27);
			this.savedSearchesBox.Visible = false;
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(59, 24);
			this.toolStripLabel2.Text = "Page Size:";
			// 
			// tbPageSize
			// 
			this.tbPageSize.Name = "tbPageSize";
			this.tbPageSize.Size = new System.Drawing.Size(40, 27);
			// 
			// getPreviousPage
			// 
			this.getPreviousPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.getPreviousPage.Image = ((System.Drawing.Image)(resources.GetObject("getPreviousPage.Image")));
			this.getPreviousPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.getPreviousPage.Name = "getPreviousPage";
			this.getPreviousPage.Size = new System.Drawing.Size(24, 24);
			this.getPreviousPage.Text = "Page Up";
			// 
			// getNextPage
			// 
			this.getNextPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.getNextPage.Image = ((System.Drawing.Image)(resources.GetObject("getNextPage.Image")));
			this.getNextPage.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.getNextPage.Name = "getNextPage";
			this.getNextPage.Size = new System.Drawing.Size(24, 24);
			this.getNextPage.Text = "Page Down";
			// 
			// titlePictureBox
			// 
			this.titlePictureBox.Location = new System.Drawing.Point(6, 6);
			this.titlePictureBox.Name = "titlePictureBox";
			this.titlePictureBox.Size = new System.Drawing.Size(16, 16);
			this.titlePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.titlePictureBox.TabIndex = 16;
			this.titlePictureBox.TabStop = false;
			// 
			// titleLabel
			// 
			this.titleLabel.AutoSize = true;
			this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.titleLabel.Location = new System.Drawing.Point(26, 6);
			this.titleLabel.Name = "titleLabel";
			this.titleLabel.Size = new System.Drawing.Size(45, 16);
			this.titleLabel.TabIndex = 17;
			this.titleLabel.Text = "label1";
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeButton.Location = new System.Drawing.Point(810, 2);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(24, 24);
			this.closeButton.TabIndex = 18;
			this.closeButton.Text = "✖";
			this.closeButton.UseVisualStyleBackColor = true;
			// 
			// ItemSearchDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(836, 454);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.titleLabel);
			this.Controls.Add(this.titlePictureBox);
			this.Controls.Add(this.contentPanel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(500, 300);
			this.Name = "ItemSearchDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Search dialog";
			this.contentPanel.ResumeLayout(false);
			this.contentPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.partsGrid)).EndInit();
			this.tools.ResumeLayout(false);
			this.tools.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.titlePictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
		private System.Windows.Forms.Panel contentPanel;
		private KeyDownEnabledGrid partsGrid;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.ToolStrip tools;
		private System.Windows.Forms.ToolStripButton runSearchBtn;
		private System.Windows.Forms.ToolStripButton newSearchBtn;
		private System.Windows.Forms.ToolStripSeparator savedSearchesSeparator;
		private System.Windows.Forms.ToolStripLabel itemTypeLabel;
		private System.Windows.Forms.ToolStripComboBox itemTypeBox;
		private System.Windows.Forms.ToolStripLabel savedSearchesLabel;
		private System.Windows.Forms.ToolStripComboBox savedSearchesBox;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripTextBox tbPageSize;
		private System.Windows.Forms.ToolStripButton getPreviousPage;
		private System.Windows.Forms.ToolStripButton getNextPage;
		private System.Windows.Forms.PictureBox titlePictureBox;
		private System.Windows.Forms.Label titleLabel;
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.ToolTip toolTip1;
	}
}