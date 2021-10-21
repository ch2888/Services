/*
SAMPLE CODE NOTICE

THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED, 
OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.  
THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.  
NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
*/


namespace Microsoft.Dynamics.Retail.Pos.Services
{
	partial class DualDisplayForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				imageRotatorTimer.Elapsed -= new System.Timers.ElapsedEventHandler(imageRotatorTimer_Elapsed);

				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DualDisplayForm));
            this.picImageRotator = new DevExpress.XtraEditors.PictureEdit();
            this.receipt = new LSRetailPosis.POSProcesses.WinControls.Receipt();
            this.panelReceipt = new DevExpress.XtraEditors.PanelControl();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.totalAmounts = new LSRetailPosis.POSProcesses.WinControls.TotalAmounts();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.lblInitialText = new System.Windows.Forms.Label();
            this.panelInitialState = new DevExpress.XtraEditors.PanelControl();
            this.panelAdvertisements = new DevExpress.XtraEditors.PanelControl();
            this.picImageLogo = new DevExpress.XtraEditors.PictureEdit();
            this.lblAdSettingsMissing = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.styleController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picImageRotator.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelReceipt)).BeginInit();
            this.panelReceipt.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelInitialState)).BeginInit();
            this.panelInitialState.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelAdvertisements)).BeginInit();
            this.panelAdvertisements.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImageLogo.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // styleController
            // 
            this.styleController.LookAndFeel.SkinName = "Money Twins";
            // 
            // picImageRotator
            // 
            this.picImageRotator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picImageRotator.Location = new System.Drawing.Point(2, 2);
            this.picImageRotator.Name = "picImageRotator";
            this.picImageRotator.Properties.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.picImageRotator.Properties.Appearance.Options.UseBackColor = true;
            this.picImageRotator.Properties.SizeMode = DevExpress.XtraEditors.Controls.PictureSizeMode.Squeeze;
            this.picImageRotator.Size = new System.Drawing.Size(444, 389);
            this.picImageRotator.TabIndex = 0;
            // 
            // receipt
            // 
            this.receipt.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.receipt.Appearance.Options.UseBackColor = true;
            this.receipt.Appearance.Options.UseFont = true;
            this.receipt.AutoSize = true;
            this.receipt.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.receipt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.receipt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.receipt.Location = new System.Drawing.Point(3, 3);
            this.receipt.LookAndFeel.SkinName = "Money Twins";
            this.receipt.Name = "receipt";
            this.receipt.ReturnItems = false;
            this.receipt.Size = new System.Drawing.Size(389, 522);
            this.receipt.TabIndex = 21;
            this.receipt.TabStop = false;
            // 
            // panelReceipt
            // 
            this.panelReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.panelReceipt.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.panelReceipt.Controls.Add(this.tableLayoutPanel);
            this.panelReceipt.Location = new System.Drawing.Point(1, 2);
            this.panelReceipt.Name = "panelReceipt";
            this.panelReceipt.Padding = new System.Windows.Forms.Padding(5);
            this.panelReceipt.Size = new System.Drawing.Size(409, 719);
            this.panelReceipt.TabIndex = 23;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Controls.Add(this.totalAmounts, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.receipt, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(7, 7);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 75F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(395, 705);
            this.tableLayoutPanel.TabIndex = 22;
            // 
            // totalAmounts
            // 
            this.totalAmounts.Appearance.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.totalAmounts.Appearance.Options.UseFont = true;
            this.totalAmounts.AutoSize = true;
            this.totalAmounts.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.totalAmounts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.totalAmounts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.totalAmounts.Location = new System.Drawing.Point(3, 531);
            this.totalAmounts.Name = "totalAmounts";
            this.totalAmounts.Size = new System.Drawing.Size(389, 171);
            this.totalAmounts.TabIndex = 22;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(2, 2);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScrollBarsEnabled = false;
            this.webBrowser1.Size = new System.Drawing.Size(444, 389);
            this.webBrowser1.TabIndex = 1;
            this.webBrowser1.Url = new System.Uri("", System.UriKind.Relative);
            // 
            // lblInitialText
            // 
            this.lblInitialText.AutoSize = true;
            this.lblInitialText.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.lblInitialText.Location = new System.Drawing.Point(42, 190);
            this.lblInitialText.Name = "lblInitialText";
            this.lblInitialText.Size = new System.Drawing.Size(265, 32);
            this.lblInitialText.TabIndex = 26;
            this.lblInitialText.Text = "Retail POS Dual Display";
            // 
            // panelInitialState
            // 
            this.panelInitialState.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.panelInitialState.Controls.Add(this.lblInitialText);
            this.panelInitialState.Location = new System.Drawing.Point(561, 277);
            this.panelInitialState.Name = "panelInitialState";
            this.panelInitialState.Size = new System.Drawing.Size(448, 393);
            this.panelInitialState.TabIndex = 27;
            // 
            // panelAdvertisements
            // 
            this.panelAdvertisements.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.panelAdvertisements.Controls.Add(this.picImageLogo);
            this.panelAdvertisements.Controls.Add(this.lblAdSettingsMissing);
            this.panelAdvertisements.Controls.Add(this.webBrowser1);
            this.panelAdvertisements.Controls.Add(this.picImageRotator);
            this.panelAdvertisements.Location = new System.Drawing.Point(746, 30);
            this.panelAdvertisements.Name = "panelAdvertisements";
            this.panelAdvertisements.Size = new System.Drawing.Size(448, 393);
            this.panelAdvertisements.TabIndex = 28;
            // 
            // picImageLogo
            // 
            this.picImageLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picImageLogo.Location = new System.Drawing.Point(2, 2);
            this.picImageLogo.Image = ((System.Drawing.Image)(resources.GetObject("Logo")));
            this.picImageLogo.Name = "picImageLogo";
            this.picImageLogo.Size = new System.Drawing.Size(444, 389);
            this.picImageLogo.TabIndex = 29;
            // 
            // lblAdSettingsMissing
            // 
            this.lblAdSettingsMissing.AutoSize = true;
            this.lblAdSettingsMissing.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.lblAdSettingsMissing.Location = new System.Drawing.Point(30, 176);
            this.lblAdSettingsMissing.Name = "lblAdSettingsMissing";
            this.lblAdSettingsMissing.Size = new System.Drawing.Size(348, 32);
            this.lblAdSettingsMissing.TabIndex = 27;
            this.lblAdSettingsMissing.Text = "Advertisement settings missing";
            // 
            // DualDisplayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.panelAdvertisements);
            this.Controls.Add(this.panelInitialState);
            this.Controls.Add(this.panelReceipt);
            this.LookAndFeel.SkinName = "Money Twins";
            this.Name = "DualDisplayForm";
            this.Text = "POS - Dual Display";
            ((System.ComponentModel.ISupportInitialize)(this.styleController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picImageRotator.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.panelReceipt)).EndInit();
            this.panelReceipt.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelInitialState)).EndInit();
            this.panelInitialState.ResumeLayout(false);
            this.panelInitialState.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.panelAdvertisements)).EndInit();
            this.panelAdvertisements.ResumeLayout(false);
            this.panelAdvertisements.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picImageLogo.Properties)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

		private LSRetailPosis.POSProcesses.WinControls.Receipt receipt;
		private DevExpress.XtraEditors.PictureEdit picImageRotator;
		private DevExpress.XtraEditors.PanelControl panelReceipt;
		private System.Windows.Forms.WebBrowser webBrowser1;
		private System.Windows.Forms.Label lblInitialText;
		private DevExpress.XtraEditors.PanelControl panelInitialState;
		private DevExpress.XtraEditors.PanelControl panelAdvertisements;
		private System.Windows.Forms.Label lblAdSettingsMissing;
        private DevExpress.XtraEditors.PictureEdit picImageLogo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private LSRetailPosis.POSProcesses.WinControls.TotalAmounts totalAmounts;

	}
}

