using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace NUSGrabberGUI
{
    partial class NUSGrabberForm
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
            this.components = new System.ComponentModel.Container();
            this.NUSTabs = new System.Windows.Forms.TabControl();
            this.GameUpdateTab = new System.Windows.Forms.TabPage();
            this.GUExportButton = new System.Windows.Forms.Button();
            this.GUTitleIDLabel = new System.Windows.Forms.Label();
            this.NUSGrabberProgress = new System.Windows.Forms.ProgressBar();
            this.GUVersionList = new System.Windows.Forms.ListBox();
            this.GUVersionsLabel = new System.Windows.Forms.Label();
            this.GUTitlesLabel = new System.Windows.Forms.Label();
            this.GUSearchLabel = new System.Windows.Forms.Label();
            this.GUSearchBox = new System.Windows.Forms.TextBox();
            this.GUTitleList = new System.Windows.Forms.ListBox();
            this.SystemTab = new System.Windows.Forms.TabPage();
            this.STExportButton = new System.Windows.Forms.Button();
            this.STTitleIDLabel = new System.Windows.Forms.Label();
            this.STVersionList = new System.Windows.Forms.ListBox();
            this.STVersionsLabel = new System.Windows.Forms.Label();
            this.STTitlesLabel = new System.Windows.Forms.Label();
            this.STSearchLabel = new System.Windows.Forms.Label();
            this.STSearchBox = new System.Windows.Forms.TextBox();
            this.STTitleList = new System.Windows.Forms.ListBox();
            this.FullListTab = new System.Windows.Forms.TabPage();
            this.FTExportButton = new System.Windows.Forms.Button();
            this.FTTitleIDLabel = new System.Windows.Forms.Label();
            this.FTTitlesLabel = new System.Windows.Forms.Label();
            this.FTSearchLabel = new System.Windows.Forms.Label();
            this.FTSearchBox = new System.Windows.Forms.TextBox();
            this.FTTitleList = new System.Windows.Forms.ListBox();
            this.DownloadButton = new System.Windows.Forms.Button();
            this.UpdateButton = new System.Windows.Forms.Button();
            this.AboutButton = new System.Windows.Forms.Button();
            this.ReloadButton = new System.Windows.Forms.Button();
            this.DecryptButton = new System.Windows.Forms.Button();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.EmbedNUSGrabber = new System.ComponentModel.BackgroundWorker();
            this.MenuContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuCreateCommonKey = new System.Windows.Forms.ToolStripMenuItem();
            this.NUSTabs.SuspendLayout();
            this.GameUpdateTab.SuspendLayout();
            this.SystemTab.SuspendLayout();
            this.FullListTab.SuspendLayout();
            this.MenuContext.SuspendLayout();
            this.SuspendLayout();
            // 
            // NUSTabs
            // 
            this.NUSTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NUSTabs.Controls.Add(this.GameUpdateTab);
            this.NUSTabs.Controls.Add(this.SystemTab);
            this.NUSTabs.Controls.Add(this.FullListTab);
            this.NUSTabs.Location = new System.Drawing.Point(12, 12);
            this.NUSTabs.Name = "NUSTabs";
            this.NUSTabs.SelectedIndex = 0;
            this.NUSTabs.Size = new System.Drawing.Size(455, 376);
            this.NUSTabs.TabIndex = 0;
            this.NUSTabs.SelectedIndexChanged += new System.EventHandler(this.NUSTabs_IndexChanged);
            // 
            // GameUpdateTab
            // 
            this.GameUpdateTab.BackColor = System.Drawing.Color.LightGreen;
            this.GameUpdateTab.Controls.Add(this.GUExportButton);
            this.GameUpdateTab.Controls.Add(this.GUTitleIDLabel);
            this.GameUpdateTab.Controls.Add(this.NUSGrabberProgress);
            this.GameUpdateTab.Controls.Add(this.GUVersionList);
            this.GameUpdateTab.Controls.Add(this.GUVersionsLabel);
            this.GameUpdateTab.Controls.Add(this.GUTitlesLabel);
            this.GameUpdateTab.Controls.Add(this.GUSearchLabel);
            this.GameUpdateTab.Controls.Add(this.GUSearchBox);
            this.GameUpdateTab.Controls.Add(this.GUTitleList);
            this.GameUpdateTab.ForeColor = System.Drawing.SystemColors.ControlText;
            this.GameUpdateTab.Location = new System.Drawing.Point(4, 22);
            this.GameUpdateTab.Name = "GameUpdateTab";
            this.GameUpdateTab.Padding = new System.Windows.Forms.Padding(3);
            this.GameUpdateTab.Size = new System.Drawing.Size(447, 350);
            this.GameUpdateTab.TabIndex = 0;
            this.GameUpdateTab.Text = "Game Updates";
            // 
            // GUExportButton
            // 
            this.GUExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GUExportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.GUExportButton.Enabled = false;
            this.GUExportButton.Location = new System.Drawing.Point(267, 324);
            this.GUExportButton.Name = "GUExportButton";
            this.GUExportButton.Size = new System.Drawing.Size(174, 23);
            this.GUExportButton.TabIndex = 8;
            this.GUExportButton.Text = "Export Title to Debug Log";
            this.GUExportButton.UseVisualStyleBackColor = true;
            this.GUExportButton.Visible = false;
            this.GUExportButton.Click += new System.EventHandler(this.GUExportButton_Click);
            // 
            // GUTitleIDLabel
            // 
            this.GUTitleIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GUTitleIDLabel.AutoSize = true;
            this.GUTitleIDLabel.Location = new System.Drawing.Point(9, 326);
            this.GUTitleIDLabel.Name = "GUTitleIDLabel";
            this.GUTitleIDLabel.Size = new System.Drawing.Size(44, 13);
            this.GUTitleIDLabel.TabIndex = 7;
            this.GUTitleIDLabel.Text = "Title ID:";
            // 
            // NUSGrabberProgress
            // 
            this.NUSGrabberProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NUSGrabberProgress.Location = new System.Drawing.Point(56, 6);
            this.NUSGrabberProgress.Name = "NUSGrabberProgress";
            this.NUSGrabberProgress.Size = new System.Drawing.Size(385, 20);
            this.NUSGrabberProgress.TabIndex = 6;
            this.NUSGrabberProgress.Visible = false;
            // 
            // GUVersionList
            // 
            this.GUVersionList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GUVersionList.FormattingEnabled = true;
            this.GUVersionList.Location = new System.Drawing.Point(384, 53);
            this.GUVersionList.Name = "GUVersionList";
            this.GUVersionList.Size = new System.Drawing.Size(57, 264);
            this.GUVersionList.TabIndex = 5;
            // 
            // GUVersionsLabel
            // 
            this.GUVersionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GUVersionsLabel.AutoSize = true;
            this.GUVersionsLabel.Location = new System.Drawing.Point(387, 37);
            this.GUVersionsLabel.Name = "GUVersionsLabel";
            this.GUVersionsLabel.Size = new System.Drawing.Size(50, 13);
            this.GUVersionsLabel.TabIndex = 4;
            this.GUVersionsLabel.Text = "Versions:";
            // 
            // GUTitlesLabel
            // 
            this.GUTitlesLabel.AutoSize = true;
            this.GUTitlesLabel.Location = new System.Drawing.Point(120, 37);
            this.GUTitlesLabel.Name = "GUTitlesLabel";
            this.GUTitlesLabel.Size = new System.Drawing.Size(35, 13);
            this.GUTitlesLabel.TabIndex = 3;
            this.GUTitlesLabel.Text = "Titles:";
            // 
            // GUSearchLabel
            // 
            this.GUSearchLabel.AutoSize = true;
            this.GUSearchLabel.Location = new System.Drawing.Point(6, 9);
            this.GUSearchLabel.Name = "GUSearchLabel";
            this.GUSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.GUSearchLabel.TabIndex = 2;
            this.GUSearchLabel.Text = "Search:";
            // 
            // GUSearchBox
            // 
            this.GUSearchBox.Location = new System.Drawing.Point(56, 6);
            this.GUSearchBox.Name = "GUSearchBox";
            this.GUSearchBox.Size = new System.Drawing.Size(290, 20);
            this.GUSearchBox.TabIndex = 1;
            this.GUSearchBox.TextChanged += new System.EventHandler(this.GUSearchBox_TextChanged);
            // 
            // GUTitleList
            // 
            this.GUTitleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GUTitleList.FormattingEnabled = true;
            this.GUTitleList.Location = new System.Drawing.Point(6, 53);
            this.GUTitleList.Name = "GUTitleList";
            this.GUTitleList.Size = new System.Drawing.Size(372, 264);
            this.GUTitleList.TabIndex = 0;
            this.GUTitleList.SelectedIndexChanged += new System.EventHandler(this.GUTitleList_SelectedIndexChanged);
            // 
            // SystemTab
            // 
            this.SystemTab.BackColor = System.Drawing.Color.LightCoral;
            this.SystemTab.Controls.Add(this.STExportButton);
            this.SystemTab.Controls.Add(this.STTitleIDLabel);
            this.SystemTab.Controls.Add(this.STVersionList);
            this.SystemTab.Controls.Add(this.STVersionsLabel);
            this.SystemTab.Controls.Add(this.STTitlesLabel);
            this.SystemTab.Controls.Add(this.STSearchLabel);
            this.SystemTab.Controls.Add(this.STSearchBox);
            this.SystemTab.Controls.Add(this.STTitleList);
            this.SystemTab.Location = new System.Drawing.Point(4, 22);
            this.SystemTab.Name = "SystemTab";
            this.SystemTab.Padding = new System.Windows.Forms.Padding(3);
            this.SystemTab.Size = new System.Drawing.Size(352, 257);
            this.SystemTab.TabIndex = 1;
            this.SystemTab.Text = "System Titles";
            // 
            // STExportButton
            // 
            this.STExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.STExportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.STExportButton.Enabled = false;
            this.STExportButton.Location = new System.Drawing.Point(172, 231);
            this.STExportButton.Name = "STExportButton";
            this.STExportButton.Size = new System.Drawing.Size(174, 23);
            this.STExportButton.TabIndex = 13;
            this.STExportButton.Text = "Export Title to Debug Log";
            this.STExportButton.UseVisualStyleBackColor = true;
            this.STExportButton.Visible = false;
            this.STExportButton.Click += new System.EventHandler(this.STExportButton_Click);
            // 
            // STTitleIDLabel
            // 
            this.STTitleIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.STTitleIDLabel.AutoSize = true;
            this.STTitleIDLabel.Location = new System.Drawing.Point(9, 233);
            this.STTitleIDLabel.Name = "STTitleIDLabel";
            this.STTitleIDLabel.Size = new System.Drawing.Size(44, 13);
            this.STTitleIDLabel.TabIndex = 12;
            this.STTitleIDLabel.Text = "Title ID:";
            // 
            // STVersionList
            // 
            this.STVersionList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.STVersionList.FormattingEnabled = true;
            this.STVersionList.Location = new System.Drawing.Point(272, 53);
            this.STVersionList.Name = "STVersionList";
            this.STVersionList.Size = new System.Drawing.Size(74, 173);
            this.STVersionList.TabIndex = 11;
            // 
            // STVersionsLabel
            // 
            this.STVersionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.STVersionsLabel.AutoSize = true;
            this.STVersionsLabel.Location = new System.Drawing.Point(285, 37);
            this.STVersionsLabel.Name = "STVersionsLabel";
            this.STVersionsLabel.Size = new System.Drawing.Size(50, 13);
            this.STVersionsLabel.TabIndex = 10;
            this.STVersionsLabel.Text = "Versions:";
            // 
            // STTitlesLabel
            // 
            this.STTitlesLabel.AutoSize = true;
            this.STTitlesLabel.Location = new System.Drawing.Point(111, 37);
            this.STTitlesLabel.Name = "STTitlesLabel";
            this.STTitlesLabel.Size = new System.Drawing.Size(35, 13);
            this.STTitlesLabel.TabIndex = 9;
            this.STTitlesLabel.Text = "Titles:";
            // 
            // STSearchLabel
            // 
            this.STSearchLabel.AutoSize = true;
            this.STSearchLabel.Location = new System.Drawing.Point(6, 9);
            this.STSearchLabel.Name = "STSearchLabel";
            this.STSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.STSearchLabel.TabIndex = 8;
            this.STSearchLabel.Text = "Search:";
            // 
            // STSearchBox
            // 
            this.STSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.STSearchBox.Location = new System.Drawing.Point(56, 6);
            this.STSearchBox.Name = "STSearchBox";
            this.STSearchBox.Size = new System.Drawing.Size(290, 20);
            this.STSearchBox.TabIndex = 7;
            this.STSearchBox.TextChanged += new System.EventHandler(this.STSearchBox_TextChanged);
            // 
            // STTitleList
            // 
            this.STTitleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.STTitleList.FormattingEnabled = true;
            this.STTitleList.Location = new System.Drawing.Point(6, 53);
            this.STTitleList.Name = "STTitleList";
            this.STTitleList.Size = new System.Drawing.Size(260, 173);
            this.STTitleList.TabIndex = 6;
            this.STTitleList.SelectedIndexChanged += new System.EventHandler(this.STTitleList_SelectedIndexChanged);
            // 
            // FullListTab
            // 
            this.FullListTab.BackColor = System.Drawing.Color.Turquoise;
            this.FullListTab.Controls.Add(this.FTExportButton);
            this.FullListTab.Controls.Add(this.FTTitleIDLabel);
            this.FullListTab.Controls.Add(this.FTTitlesLabel);
            this.FullListTab.Controls.Add(this.FTSearchLabel);
            this.FullListTab.Controls.Add(this.FTSearchBox);
            this.FullListTab.Controls.Add(this.FTTitleList);
            this.FullListTab.Location = new System.Drawing.Point(4, 22);
            this.FullListTab.Name = "FullListTab";
            this.FullListTab.Padding = new System.Windows.Forms.Padding(3);
            this.FullListTab.Size = new System.Drawing.Size(352, 257);
            this.FullListTab.TabIndex = 2;
            this.FullListTab.Text = "Full Titles";
            // 
            // FTExportButton
            // 
            this.FTExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FTExportButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.FTExportButton.Enabled = false;
            this.FTExportButton.Location = new System.Drawing.Point(172, 231);
            this.FTExportButton.Name = "FTExportButton";
            this.FTExportButton.Size = new System.Drawing.Size(174, 23);
            this.FTExportButton.TabIndex = 11;
            this.FTExportButton.Text = "Export Title to Debug Log";
            this.FTExportButton.UseVisualStyleBackColor = true;
            this.FTExportButton.Visible = false;
            this.FTExportButton.Click += new System.EventHandler(this.FTExportButton_Click);
            // 
            // FTTitleIDLabel
            // 
            this.FTTitleIDLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.FTTitleIDLabel.AutoSize = true;
            this.FTTitleIDLabel.Location = new System.Drawing.Point(9, 233);
            this.FTTitleIDLabel.Name = "FTTitleIDLabel";
            this.FTTitleIDLabel.Size = new System.Drawing.Size(44, 13);
            this.FTTitleIDLabel.TabIndex = 10;
            this.FTTitleIDLabel.Text = "Title ID:";
            // 
            // FTTitlesLabel
            // 
            this.FTTitlesLabel.AutoSize = true;
            this.FTTitlesLabel.Location = new System.Drawing.Point(152, 37);
            this.FTTitlesLabel.Name = "FTTitlesLabel";
            this.FTTitlesLabel.Size = new System.Drawing.Size(35, 13);
            this.FTTitlesLabel.TabIndex = 9;
            this.FTTitlesLabel.Text = "Titles:";
            // 
            // FTSearchLabel
            // 
            this.FTSearchLabel.AutoSize = true;
            this.FTSearchLabel.Location = new System.Drawing.Point(6, 9);
            this.FTSearchLabel.Name = "FTSearchLabel";
            this.FTSearchLabel.Size = new System.Drawing.Size(44, 13);
            this.FTSearchLabel.TabIndex = 8;
            this.FTSearchLabel.Text = "Search:";
            // 
            // FTSearchBox
            // 
            this.FTSearchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FTSearchBox.Location = new System.Drawing.Point(56, 6);
            this.FTSearchBox.Name = "FTSearchBox";
            this.FTSearchBox.Size = new System.Drawing.Size(290, 20);
            this.FTSearchBox.TabIndex = 7;
            this.FTSearchBox.TextChanged += new System.EventHandler(this.FTSearchBox_TextChanged);
            // 
            // FTTitleList
            // 
            this.FTTitleList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FTTitleList.FormattingEnabled = true;
            this.FTTitleList.Location = new System.Drawing.Point(6, 53);
            this.FTTitleList.Name = "FTTitleList";
            this.FTTitleList.Size = new System.Drawing.Size(340, 173);
            this.FTTitleList.TabIndex = 6;
            this.FTTitleList.SelectedIndexChanged += new System.EventHandler(this.FTTitleList_SelectedIndexChanged);
            // 
            // DownloadButton
            // 
            this.DownloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DownloadButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.DownloadButton.Enabled = false;
            this.DownloadButton.Location = new System.Drawing.Point(360, 394);
            this.DownloadButton.Name = "DownloadButton";
            this.DownloadButton.Size = new System.Drawing.Size(110, 23);
            this.DownloadButton.TabIndex = 3;
            this.DownloadButton.Text = "&Download";
            this.DownloadButton.UseVisualStyleBackColor = true;
            this.DownloadButton.Click += new System.EventHandler(this.DownloadButton_Click);
            // 
            // UpdateButton
            // 
            this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.UpdateButton.Enabled = false;
            this.UpdateButton.Location = new System.Drawing.Point(128, 394);
            this.UpdateButton.Name = "UpdateButton";
            this.UpdateButton.Size = new System.Drawing.Size(110, 23);
            this.UpdateButton.TabIndex = 4;
            this.UpdateButton.Text = "&Update VersionList";
            this.UpdateButton.UseVisualStyleBackColor = true;
            this.UpdateButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // AboutButton
            // 
            this.AboutButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AboutButton.Enabled = false;
            this.AboutButton.Location = new System.Drawing.Point(12, 394);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(110, 23);
            this.AboutButton.TabIndex = 5;
            this.AboutButton.Text = "&Options";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
            // 
            // ReloadButton
            // 
            this.ReloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ReloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadButton.Location = new System.Drawing.Point(383, 10);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(82, 22);
            this.ReloadButton.TabIndex = 6;
            this.ReloadButton.Text = "&Load Titles";
            this.ReloadButton.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // DecryptButton
            // 
            this.DecryptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DecryptButton.Enabled = false;
            this.DecryptButton.Location = new System.Drawing.Point(244, 394);
            this.DecryptButton.Name = "DecryptButton";
            this.DecryptButton.Size = new System.Drawing.Size(110, 23);
            this.DecryptButton.TabIndex = 7;
            this.DecryptButton.Text = "D&ecrypt Download";
            this.DecryptButton.UseVisualStyleBackColor = true;
            this.DecryptButton.Click += new System.EventHandler(this.DecryptButton_Click);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.FileName = "ckey.bin";
            // 
            // EmbedNUSGrabber
            // 
            this.EmbedNUSGrabber.DoWork += new System.ComponentModel.DoWorkEventHandler(this.EmbedNUSGrabber_Work);
            this.EmbedNUSGrabber.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.EmbedNUSGrabber_Done);
            // 
            // MenuContext
            // 
            this.MenuContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuSettings,
            this.MenuTools});
            this.MenuContext.Name = "MenuContext";
            this.MenuContext.Size = new System.Drawing.Size(117, 48);
            this.MenuContext.Text = "Settings";
            // 
            // MenuSettings
            // 
            this.MenuSettings.Name = "MenuSettings";
            this.MenuSettings.Size = new System.Drawing.Size(116, 22);
            this.MenuSettings.Text = "&Settings";
            this.MenuSettings.Click += new System.EventHandler(this.MenuSettings_Click);
            // 
            // MenuTools
            // 
            this.MenuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuCreateCommonKey});
            this.MenuTools.Name = "MenuTools";
            this.MenuTools.Size = new System.Drawing.Size(116, 22);
            this.MenuTools.Text = "Tools";
            // 
            // MenuCreateCommonKey
            // 
            this.MenuCreateCommonKey.Name = "MenuCreateCommonKey";
            this.MenuCreateCommonKey.Size = new System.Drawing.Size(184, 22);
            this.MenuCreateCommonKey.Text = "Create Common Key";
            this.MenuCreateCommonKey.Click += new System.EventHandler(this.MenuCreateCommonKey_Click);
            // 
            // NUSGrabberForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(479, 429);
            this.Controls.Add(this.DecryptButton);
            this.Controls.Add(this.ReloadButton);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.UpdateButton);
            this.Controls.Add(this.DownloadButton);
            this.Controls.Add(this.NUSTabs);
            this.Name = "NUSGrabberForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NEW-NUSGrabberGUI";
            this.NUSTabs.ResumeLayout(false);
            this.GameUpdateTab.ResumeLayout(false);
            this.GameUpdateTab.PerformLayout();
            this.SystemTab.ResumeLayout(false);
            this.SystemTab.PerformLayout();
            this.FullListTab.ResumeLayout(false);
            this.FullListTab.PerformLayout();
            this.MenuContext.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private TabControl NUSTabs;
        private TabPage GameUpdateTab;
        private ListBox GUVersionList;
        private Label GUVersionsLabel;
        private Label GUTitlesLabel;
        private Label GUSearchLabel;
        private TextBox GUSearchBox;
        private ListBox GUTitleList;
        private TabPage SystemTab;
        private TabPage FullListTab;
        private Button DownloadButton;
        private ListBox STVersionList;
        private Label STVersionsLabel;
        private Label STTitlesLabel;
        private Label STSearchLabel;
        private TextBox STSearchBox;
        private ListBox STTitleList;
        private Label FTTitlesLabel;
        private Label FTSearchLabel;
        private TextBox FTSearchBox;
        private ListBox FTTitleList;
        private Button UpdateButton;
        private Button AboutButton;
        private Button ReloadButton;
        private Button DecryptButton;
        private OpenFileDialog OpenFileDialog;
        private BackgroundWorker EmbedNUSGrabber;
        private ProgressBar NUSGrabberProgress;
        private Label GUTitleIDLabel;
        private Label STTitleIDLabel;
        private Label FTTitleIDLabel;
        private Button GUExportButton;
        private Button STExportButton;
        private Button FTExportButton;
        private ContextMenuStrip MenuContext;
        private ToolStripMenuItem MenuSettings;
        private ToolStripMenuItem MenuTools;
        private ToolStripMenuItem MenuCreateCommonKey;
    }
}

