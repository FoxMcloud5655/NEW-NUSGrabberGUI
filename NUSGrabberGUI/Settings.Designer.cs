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
            RegionBox = new ComboBox();
            RegionLabel = new Label();
            AboutButton = new Button();
            SaveCloseButton = new Button();
            UpdateButton = new Button();
            DecryptCheckBox = new CheckBox();
            HideCheckBox = new CheckBox();
            LoadCheckBox = new CheckBox();
            EmbedCheckBox = new CheckBox();
            ArchivedCheckBox = new CheckBox();
            CleanupCheckBox = new CheckBox();
            OrigCheckBox = new CheckBox();
            FAQButton = new Button();
            SuspendLayout();
            // 
            // RegionBox
            // 
            RegionBox.FormattingEnabled = true;
            RegionBox.Items.AddRange(new object[] {
            "USA",
            "EUR",
            "JPN"});
            RegionBox.Location = new Point(30, 33);
            RegionBox.Name = "RegionBox";
            RegionBox.Size = new Size(121, 21);
            RegionBox.TabIndex = 0;
            // 
            // RegionLabel
            // 
            RegionLabel.AutoSize = true;
            RegionLabel.Location = new Point(13, 13);
            RegionLabel.Name = "RegionLabel";
            RegionLabel.Size = new Size(262, 13);
            RegionLabel.TabIndex = 1;
            RegionLabel.Text = "Region - Select a region to download versionlists from.";
            // 
            // AboutButton
            // 
            AboutButton.Location = new Point(12, 226);
            AboutButton.Name = "AboutButton";
            AboutButton.Size = new Size(46, 23);
            AboutButton.TabIndex = 2;
            AboutButton.Text = "About";
            AboutButton.UseVisualStyleBackColor = true;
            AboutButton.Click += new EventHandler(AboutButton_Click);
            // 
            // SaveCloseButton
            // 
            SaveCloseButton.DialogResult = DialogResult.Cancel;
            SaveCloseButton.Location = new Point(246, 226);
            SaveCloseButton.Name = "SaveCloseButton";
            SaveCloseButton.Size = new Size(97, 23);
            SaveCloseButton.TabIndex = 3;
            SaveCloseButton.Text = "Save and Close";
            SaveCloseButton.UseVisualStyleBackColor = true;
            SaveCloseButton.Click += new EventHandler(SaveCloseButton_Click);
            // 
            // UpdateButton
            // 
            UpdateButton.DialogResult = DialogResult.Cancel;
            UpdateButton.Location = new Point(135, 226);
            UpdateButton.Name = "UpdateButton";
            UpdateButton.Size = new Size(105, 23);
            UpdateButton.TabIndex = 4;
            UpdateButton.Text = "Check for Updates";
            UpdateButton.UseVisualStyleBackColor = true;
            UpdateButton.Click += new EventHandler(UpdateButton_Click);
            // 
            // DecryptCheckBox
            // 
            DecryptCheckBox.AutoSize = true;
            DecryptCheckBox.Location = new Point(16, 61);
            DecryptCheckBox.Name = "DecryptCheckBox";
            DecryptCheckBox.Size = new Size(180, 17);
            DecryptCheckBox.TabIndex = 5;
            DecryptCheckBox.Text = "Automatically decrypt downloads";
            DecryptCheckBox.UseVisualStyleBackColor = true;
            // 
            // HideCheckBox
            // 
            HideCheckBox.AutoSize = true;
            HideCheckBox.Location = new Point(16, 107);
            HideCheckBox.Name = "HideCheckBox";
            HideCheckBox.Size = new Size(228, 17);
            HideCheckBox.TabIndex = 6;
            HideCheckBox.Text = "Hide NUSgrabber/wget while downloading";
            HideCheckBox.UseVisualStyleBackColor = true;
            // 
            // LoadCheckBox
            // 
            LoadCheckBox.AutoSize = true;
            LoadCheckBox.Location = new Point(16, 84);
            LoadCheckBox.Name = "LoadCheckBox";
            LoadCheckBox.Size = new Size(124, 17);
            LoadCheckBox.TabIndex = 7;
            LoadCheckBox.Text = "Load titles on startup";
            LoadCheckBox.UseVisualStyleBackColor = true;
            // 
            // EmbedCheckBox
            // 
            EmbedCheckBox.AutoSize = true;
            EmbedCheckBox.Checked = true;
            EmbedCheckBox.CheckState = CheckState.Checked;
            EmbedCheckBox.Location = new Point(16, 199);
            EmbedCheckBox.Name = "EmbedCheckBox";
            EmbedCheckBox.Size = new Size(167, 17);
            EmbedCheckBox.TabIndex = 8;
            EmbedCheckBox.Text = "Use experimental LibWiiSharp";
            EmbedCheckBox.UseVisualStyleBackColor = true;
            EmbedCheckBox.CheckedChanged += new EventHandler(EmbedCheckBox_CheckedChanged);
            // 
            // ArchivedCheckBox
            // 
            ArchivedCheckBox.AutoSize = true;
            ArchivedCheckBox.Location = new Point(16, 153);
            ArchivedCheckBox.Name = "ArchivedCheckBox";
            ArchivedCheckBox.Size = new Size(218, 17);
            ArchivedCheckBox.TabIndex = 9;
            ArchivedCheckBox.Text = "Load archived database instead of latest";
            ArchivedCheckBox.UseVisualStyleBackColor = true;
            // 
            // CleanupCheckBox
            // 
            CleanupCheckBox.AutoSize = true;
            CleanupCheckBox.Location = new Point(16, 176);
            CleanupCheckBox.Name = "CleanupCheckBox";
            CleanupCheckBox.Size = new Size(148, 17);
            CleanupCheckBox.TabIndex = 10;
            CleanupCheckBox.Text = "Cleanup resources on exit";
            CleanupCheckBox.UseVisualStyleBackColor = true;
            // 
            // OrigCheckBox
            // 
            OrigCheckBox.AutoSize = true;
            OrigCheckBox.Location = new Point(16, 130);
            OrigCheckBox.Name = "OrigCheckBox";
            OrigCheckBox.Size = new Size(145, 17);
            OrigCheckBox.TabIndex = 11;
            OrigCheckBox.Text = "Use original NUSGrabber";
            OrigCheckBox.UseVisualStyleBackColor = true;
            // 
            // FAQButton
            // 
            FAQButton.Location = new Point(64, 226);
            FAQButton.Name = "FAQButton";
            FAQButton.Size = new Size(65, 23);
            FAQButton.TabIndex = 12;
            FAQButton.Text = "Help/FAQ";
            FAQButton.UseVisualStyleBackColor = true;
            FAQButton.Click += new EventHandler(FAQButton_Click);
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = SaveCloseButton;
            ClientSize = new Size(355, 261);
            Controls.Add(FAQButton);
            Controls.Add(OrigCheckBox);
            Controls.Add(CleanupCheckBox);
            Controls.Add(ArchivedCheckBox);
            Controls.Add(EmbedCheckBox);
            Controls.Add(LoadCheckBox);
            Controls.Add(HideCheckBox);
            Controls.Add(DecryptCheckBox);
            Controls.Add(UpdateButton);
            Controls.Add(SaveCloseButton);
            Controls.Add(AboutButton);
            Controls.Add(RegionLabel);
            Controls.Add(RegionBox);
            Name = "SettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings";
            ResumeLayout(false);
            PerformLayout();

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
    }
}