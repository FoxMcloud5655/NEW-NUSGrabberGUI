using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace NUSGrabberGUI
{
    partial class SettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.RegionBox = new System.Windows.Forms.ComboBox();
            this.RegionLabel = new System.Windows.Forms.Label();
            this.AboutButton = new System.Windows.Forms.Button();
            this.SaveCloseButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.DecryptCheckBox = new System.Windows.Forms.CheckBox();
            this.HideCheckBox = new System.Windows.Forms.CheckBox();
            this.LoadCheckBox = new System.Windows.Forms.CheckBox();
            this.EmbedCheckBox = new System.Windows.Forms.CheckBox();
            this.ArchivedCheckBox = new System.Windows.Forms.CheckBox();
            this.CleanupCheckBox = new System.Windows.Forms.CheckBox();
            this.OrigCheckBox = new System.Windows.Forms.CheckBox();
            this.FAQButton = new System.Windows.Forms.Button();
            this.LanguageLabel = new System.Windows.Forms.Label();
            this.LanguageBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // RegionBox
            // 
            this.RegionBox.FormattingEnabled = true;
            this.RegionBox.Items.AddRange(new object[] {
            "USA",
            "EUR",
            "JPN"});
            this.RegionBox.Location = new System.Drawing.Point(30, 33);
            this.RegionBox.Name = "RegionBox";
            this.RegionBox.Size = new System.Drawing.Size(121, 21);
            this.RegionBox.TabIndex = 0;
            // 
            // RegionLabel
            // 
            this.RegionLabel.AutoSize = true;
            this.RegionLabel.Location = new System.Drawing.Point(13, 13);
            this.RegionLabel.Name = "RegionLabel";
            this.RegionLabel.Size = new System.Drawing.Size(262, 13);
            this.RegionLabel.TabIndex = 1;
            this.RegionLabel.Text = "Region - Select a region to download versionlists from:";
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(12, 274);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(46, 23);
            this.AboutButton.TabIndex = 2;
            this.AboutButton.Text = "About";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // SaveCloseButton
            // 
            this.SaveCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.SaveCloseButton.Location = new System.Drawing.Point(246, 274);
            this.SaveCloseButton.Name = "SaveCloseButton";
            this.SaveCloseButton.Size = new System.Drawing.Size(97, 23);
            this.SaveCloseButton.TabIndex = 3;
            this.SaveCloseButton.Text = "Save and Close";
            this.SaveCloseButton.UseVisualStyleBackColor = true;
            this.SaveCloseButton.Click += new System.EventHandler(this.SaveCloseButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.UpdateButton.Location = new System.Drawing.Point(135, 274);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(105, 23);
            this.UpdateButton.TabIndex = 4;
            this.UpdateButton.Text = "Check for Updates";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // DecryptCheckBox
            // 
            this.DecryptCheckBox.AutoSize = true;
            this.DecryptCheckBox.Location = new System.Drawing.Point(16, 113);
            this.DecryptCheckBox.Name = "DecryptCheckBox";
            this.DecryptCheckBox.Size = new System.Drawing.Size(180, 17);
            this.DecryptCheckBox.TabIndex = 5;
            this.DecryptCheckBox.Text = "Automatically decrypt downloads";
            this.DecryptCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideCheckBox
            // 
            this.HideCheckBox.AutoSize = true;
            this.HideCheckBox.Location = new System.Drawing.Point(16, 159);
            this.HideCheckBox.Name = "HideCheckBox";
            this.HideCheckBox.Size = new System.Drawing.Size(228, 17);
            this.HideCheckBox.TabIndex = 6;
            this.HideCheckBox.Text = "Hide NUSgrabber/wget while downloading";
            this.HideCheckBox.UseVisualStyleBackColor = true;
            // 
            // LoadCheckBox
            // 
            this.LoadCheckBox.AutoSize = true;
            this.LoadCheckBox.Location = new System.Drawing.Point(16, 136);
            this.LoadCheckBox.Name = "LoadCheckBox";
            this.LoadCheckBox.Size = new System.Drawing.Size(124, 17);
            this.LoadCheckBox.TabIndex = 7;
            this.LoadCheckBox.Text = "Load titles on startup";
            this.LoadCheckBox.UseVisualStyleBackColor = true;
            // 
            // EmbedCheckBox
            // 
            this.EmbedCheckBox.AutoSize = true;
            this.EmbedCheckBox.Checked = true;
            this.EmbedCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.EmbedCheckBox.Location = new System.Drawing.Point(16, 251);
            this.EmbedCheckBox.Name = "EmbedCheckBox";
            this.EmbedCheckBox.Size = new System.Drawing.Size(167, 17);
            this.EmbedCheckBox.TabIndex = 8;
            this.EmbedCheckBox.Text = "Use experimental LibWiiSharp";
            this.EmbedCheckBox.UseVisualStyleBackColor = true;
            this.EmbedCheckBox.CheckedChanged += new System.EventHandler(this.EmbedCheckBox_CheckedChanged);
            // 
            // ArchivedCheckBox
            // 
            this.ArchivedCheckBox.AutoSize = true;
            this.ArchivedCheckBox.Location = new System.Drawing.Point(16, 205);
            this.ArchivedCheckBox.Name = "ArchivedCheckBox";
            this.ArchivedCheckBox.Size = new System.Drawing.Size(218, 17);
            this.ArchivedCheckBox.TabIndex = 9;
            this.ArchivedCheckBox.Text = "Load archived database instead of latest";
            this.ArchivedCheckBox.UseVisualStyleBackColor = true;
            // 
            // CleanupCheckBox
            // 
            this.CleanupCheckBox.AutoSize = true;
            this.CleanupCheckBox.Location = new System.Drawing.Point(16, 228);
            this.CleanupCheckBox.Name = "CleanupCheckBox";
            this.CleanupCheckBox.Size = new System.Drawing.Size(148, 17);
            this.CleanupCheckBox.TabIndex = 10;
            this.CleanupCheckBox.Text = "Cleanup resources on exit";
            this.CleanupCheckBox.UseVisualStyleBackColor = true;
            // 
            // OrigCheckBox
            // 
            this.OrigCheckBox.AutoSize = true;
            this.OrigCheckBox.Location = new System.Drawing.Point(16, 182);
            this.OrigCheckBox.Name = "OrigCheckBox";
            this.OrigCheckBox.Size = new System.Drawing.Size(145, 17);
            this.OrigCheckBox.TabIndex = 11;
            this.OrigCheckBox.Text = "Use original NUSGrabber";
            this.OrigCheckBox.UseVisualStyleBackColor = true;
            // 
            // FAQButton
            // 
            this.FAQButton.Location = new System.Drawing.Point(64, 274);
            this.FAQButton.Name = "FAQButton";
            this.FAQButton.Size = new System.Drawing.Size(65, 23);
            this.FAQButton.TabIndex = 12;
            this.FAQButton.Text = "Help/FAQ";
            this.FAQButton.UseVisualStyleBackColor = true;
            this.FAQButton.Click += new System.EventHandler(this.FAQButton_Click);
            // 
            // LanguageLabel
            // 
            this.LanguageLabel.AutoSize = true;
            this.LanguageLabel.Location = new System.Drawing.Point(13, 64);
            this.LanguageLabel.Name = "LanguageLabel";
            this.LanguageLabel.Size = new System.Drawing.Size(220, 13);
            this.LanguageLabel.TabIndex = 14;
            this.LanguageLabel.Text = "Language - Select a language to translate to:";
            // 
            // LanguageBox
            // 
            this.LanguageBox.FormattingEnabled = true;
            this.LanguageBox.Items.AddRange(new object[] {
            "en"});
            this.LanguageBox.Location = new System.Drawing.Point(30, 84);
            this.LanguageBox.Name = "LanguageBox";
            this.LanguageBox.Size = new System.Drawing.Size(121, 21);
            this.LanguageBox.TabIndex = 13;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.SaveCloseButton;
            this.ClientSize = new System.Drawing.Size(355, 309);
            this.Controls.Add(this.LanguageLabel);
            this.Controls.Add(this.LanguageBox);
            this.Controls.Add(this.FAQButton);
            this.Controls.Add(this.OrigCheckBox);
            this.Controls.Add(this.CleanupCheckBox);
            this.Controls.Add(this.ArchivedCheckBox);
            this.Controls.Add(this.EmbedCheckBox);
            this.Controls.Add(this.LoadCheckBox);
            this.Controls.Add(this.HideCheckBox);
            this.Controls.Add(this.DecryptCheckBox);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.SaveCloseButton);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.RegionLabel);
            this.Controls.Add(this.RegionBox);
            this.Name = "SettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox RegionBox;
        private Label RegionLabel;
        private Button AboutButton;
        private Button SaveCloseButton;
        private Button UpdateButton;
        private CheckBox DecryptCheckBox;
        private CheckBox HideCheckBox;
        private CheckBox LoadCheckBox;
        private CheckBox EmbedCheckBox;
        private CheckBox ArchivedCheckBox;
        private CheckBox CleanupCheckBox;
        private CheckBox OrigCheckBox;
        private Button FAQButton;
        private Label LanguageLabel;
        private ComboBox LanguageBox;
    }
}