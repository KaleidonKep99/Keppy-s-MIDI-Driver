﻿namespace OmniMIDIConfigurator{
    partial class ChangelogWindow
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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangelogWindow));
            this.ChangelogBrowser = new System.Windows.Forms.WebBrowser();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.OkBtn = new System.Windows.Forms.Button();
            this.UpdateBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ReleasesList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // ChangelogBrowser
            // 
            this.ChangelogBrowser.AllowNavigation = false;
            this.ChangelogBrowser.AllowWebBrowserDrop = false;
            this.ChangelogBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChangelogBrowser.IsWebBrowserContextMenuEnabled = false;
            this.ChangelogBrowser.Location = new System.Drawing.Point(0, 54);
            this.ChangelogBrowser.MinimumSize = new System.Drawing.Size(23, 23);
            this.ChangelogBrowser.Name = "ChangelogBrowser";
            this.ChangelogBrowser.ScriptErrorsSuppressed = true;
            this.ChangelogBrowser.Size = new System.Drawing.Size(826, 451);
            this.ChangelogBrowser.TabIndex = 0;
            this.ChangelogBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.ChangelogBrowser_DocumentCompleted);
            // 
            // VersionLabel
            // 
            this.VersionLabel.Location = new System.Drawing.Point(359, 20);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(232, 15);
            this.VersionLabel.TabIndex = 1;
            this.VersionLabel.Text = "Changelog for version 0.0.0.0";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OkBtn
            // 
            this.OkBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OkBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OkBtn.Location = new System.Drawing.Point(724, 14);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(87, 27);
            this.OkBtn.TabIndex = 2;
            this.OkBtn.Text = "OK";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // UpdateBtn
            // 
            this.UpdateBtn.Location = new System.Drawing.Point(594, 14);
            this.UpdateBtn.Name = "UpdateBtn";
            this.UpdateBtn.Size = new System.Drawing.Size(126, 27);
            this.UpdateBtn.TabIndex = 3;
            this.UpdateBtn.Text = "Check for updates";
            this.UpdateBtn.UseVisualStyleBackColor = true;
            this.UpdateBtn.Click += new System.EventHandler(this.UpdateBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(146, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Get changelog for version:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ReleasesList
            // 
            this.ReleasesList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ReleasesList.FormattingEnabled = true;
            this.ReleasesList.Location = new System.Drawing.Point(175, 16);
            this.ReleasesList.Name = "ReleasesList";
            this.ReleasesList.Size = new System.Drawing.Size(95, 23);
            this.ReleasesList.TabIndex = 5;
            this.ReleasesList.SelectedIndexChanged += new System.EventHandler(this.ReleasesList_SelectedIndexChanged);
            // 
            // ChangelogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.OkBtn;
            this.ClientSize = new System.Drawing.Size(826, 505);
            this.Controls.Add(this.ReleasesList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UpdateBtn);
            this.Controls.Add(this.OkBtn);
            this.Controls.Add(this.VersionLabel);
            this.Controls.Add(this.ChangelogBrowser);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChangelogWindow";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ChangelogWindow";
            this.Load += new System.EventHandler(this.ChangelogWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser ChangelogBrowser;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.Button UpdateBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ReleasesList;
    }
}