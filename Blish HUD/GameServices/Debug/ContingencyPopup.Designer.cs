namespace Blish_HUD.Debug {
    partial class ContingencyPopup {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.PnlExtraInfo = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.LblTroubleshootingGuide = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.LblDiscordChannel = new System.Windows.Forms.LinkLabel();
            this.label5 = new System.Windows.Forms.Label();
            this.PnlAction = new System.Windows.Forms.FlowLayoutPanel();
            this.BttnOkay = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.LblDescription = new System.Windows.Forms.Label();
            this.PnlExtraInfo.SuspendLayout();
            this.PnlAction.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // PnlExtraInfo
            // 
            this.PnlExtraInfo.AutoSize = true;
            this.PnlExtraInfo.Controls.Add(this.label3);
            this.PnlExtraInfo.Controls.Add(this.LblTroubleshootingGuide);
            this.PnlExtraInfo.Controls.Add(this.label4);
            this.PnlExtraInfo.Controls.Add(this.LblDiscordChannel);
            this.PnlExtraInfo.Controls.Add(this.label5);
            this.PnlExtraInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PnlExtraInfo.Location = new System.Drawing.Point(0, 187);
            this.PnlExtraInfo.Name = "PnlExtraInfo";
            this.PnlExtraInfo.Padding = new System.Windows.Forms.Padding(12);
            this.PnlExtraInfo.Size = new System.Drawing.Size(564, 37);
            this.PnlExtraInfo.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 12);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Check our";
            // 
            // LblTroubleshootingGuide
            // 
            this.LblTroubleshootingGuide.AutoSize = true;
            this.LblTroubleshootingGuide.Location = new System.Drawing.Point(68, 12);
            this.LblTroubleshootingGuide.Margin = new System.Windows.Forms.Padding(0);
            this.LblTroubleshootingGuide.Name = "LblTroubleshootingGuide";
            this.LblTroubleshootingGuide.Size = new System.Drawing.Size(108, 13);
            this.LblTroubleshootingGuide.TabIndex = 0;
            this.LblTroubleshootingGuide.TabStop = true;
            this.LblTroubleshootingGuide.Text = "troubleshooting guide";
            this.LblTroubleshootingGuide.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LblTroubleshootingGuide_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(176, 12);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "for this issue or join our";
            // 
            // LblDiscordChannel
            // 
            this.LblDiscordChannel.AutoSize = true;
            this.LblDiscordChannel.Location = new System.Drawing.Point(290, 12);
            this.LblDiscordChannel.Margin = new System.Windows.Forms.Padding(0);
            this.LblDiscordChannel.Name = "LblDiscordChannel";
            this.LblDiscordChannel.Size = new System.Drawing.Size(107, 13);
            this.LblDiscordChannel.TabIndex = 3;
            this.LblDiscordChannel.TabStop = true;
            this.LblDiscordChannel.Text = "Discord help channel";
            this.LblDiscordChannel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblDiscordChannel_LinkClicked);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(397, 12);
            this.label5.Margin = new System.Windows.Forms.Padding(0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(10, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = ".";
            // 
            // PnlAction
            // 
            this.PnlAction.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PnlAction.Controls.Add(this.BttnOkay);
            this.PnlAction.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PnlAction.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.PnlAction.Location = new System.Drawing.Point(0, 152);
            this.PnlAction.Name = "PnlAction";
            this.PnlAction.Size = new System.Drawing.Size(564, 35);
            this.PnlAction.TabIndex = 1;
            // 
            // BttnOkay
            // 
            this.BttnOkay.Location = new System.Drawing.Point(482, 3);
            this.BttnOkay.Name = "BttnOkay";
            this.BttnOkay.Size = new System.Drawing.Size(75, 23);
            this.BttnOkay.TabIndex = 0;
            this.BttnOkay.Text = "OK";
            this.BttnOkay.UseVisualStyleBackColor = true;
            this.BttnOkay.Click += new System.EventHandler(this.BttnOkay_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.LblDescription);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(564, 152);
            this.panel2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Segoe UI Emoji", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(15, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 64);
            this.label2.TabIndex = 1;
            this.label2.Text = "⚠️";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LblDescription
            // 
            this.LblDescription.AutoSize = true;
            this.LblDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblDescription.ForeColor = System.Drawing.Color.MidnightBlue;
            this.LblDescription.Location = new System.Drawing.Point(82, 14);
            this.LblDescription.MaximumSize = new System.Drawing.Size(460, 0);
            this.LblDescription.Name = "LblDescription";
            this.LblDescription.Size = new System.Drawing.Size(448, 120);
            this.LblDescription.TabIndex = 0;
            this.LblDescription.Text = "Blish HUD was unable to access to Guild Wars 2 process.  It was likely started as" +
    " an administrator.\r\n\r\nRelaunch Blish HUD as an administrator or relaunch Guild W" +
    "ars 2 without admin.";
            this.LblDescription.Resize += new System.EventHandler(this.LblDescription_Resize);
            // 
            // ContingencyPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 224);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.PnlAction);
            this.Controls.Add(this.PnlExtraInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ContingencyPopup";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ContingencyPopup";
            this.TopMost = true;
            this.PnlExtraInfo.ResumeLayout(false);
            this.PnlExtraInfo.PerformLayout();
            this.PnlAction.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel PnlExtraInfo;
        private System.Windows.Forms.FlowLayoutPanel PnlAction;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button BttnOkay;
        private System.Windows.Forms.Label LblDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel LblTroubleshootingGuide;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel LblDiscordChannel;
        private System.Windows.Forms.Label label5;
    }
}