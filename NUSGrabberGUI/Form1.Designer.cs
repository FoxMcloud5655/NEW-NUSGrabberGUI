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
            NUSTabs = new TabControl();
            GameUpdateTab = new TabPage();
            GUExportButton = new Button();
            GUTitleIDLabel = new Label();
            NUSGrabberProgress = new ProgressBar();
            GUVersionList = new ListBox();
            GUVersionsLabel = new Label();
            GUTitlesLabel = new Label();
            GUSearchLabel = new Label();
            GUSearchBox = new TextBox();
            GUTitleList = new ListBox();
            SystemTab = new TabPage();
            STExportButton = new Button();
            STTitleIDLabel = new Label();
            STVersionList = new ListBox();
            STVersionsLabel = new Label();
            STTitlesLabel = new Label();
            STSearchLabel = new Label();
            STSearchBox = new TextBox();
            STTitleList = new ListBox();
            FullListTab = new TabPage();
            FTExportButton = new Button();
            FTTitleIDLabel = new Label();
            FTTitlesLabel = new Label();
            FTSearchLabel = new Label();
            FTSearchBox = new TextBox();
            FTTitleList = new ListBox();
            DownloadButton = new Button();
            UpdateButton = new Button();
            AboutButton = new Button();
            ReloadButton = new Button();
            DecryptButton = new Button();
            OpenFileDialog = new OpenFileDialog();
            EmbedNUSGrabber = new BackgroundWorker();
            NUSTabs.SuspendLayout();
            GameUpdateTab.SuspendLayout();
            SystemTab.SuspendLayout();
            FullListTab.SuspendLayout();
            SuspendLayout();
            // 
            // NUSTabs
            // 
            NUSTabs.Controls.Add(GameUpdateTab);
            NUSTabs.Controls.Add(SystemTab);
            NUSTabs.Controls.Add(FullListTab);
            NUSTabs.Location = new Point(12, 12);
            NUSTabs.Name = "NUSTabs";
            NUSTabs.SelectedIndex = 0;
            NUSTabs.Size = new Size(360, 283);
            NUSTabs.TabIndex = 0;
            NUSTabs.SelectedIndexChanged += new EventHandler(NUSTabs_IndexChanged);
            // 
            // GameUpdateTab
            // 
            GameUpdateTab.BackColor = Color.LightGreen;
            GameUpdateTab.Controls.Add(GUExportButton);
            GameUpdateTab.Controls.Add(GUTitleIDLabel);
            GameUpdateTab.Controls.Add(NUSGrabberProgress);
            GameUpdateTab.Controls.Add(GUVersionList);
            GameUpdateTab.Controls.Add(GUVersionsLabel);
            GameUpdateTab.Controls.Add(GUTitlesLabel);
            GameUpdateTab.Controls.Add(GUSearchLabel);
            GameUpdateTab.Controls.Add(GUSearchBox);
            GameUpdateTab.Controls.Add(GUTitleList);
            GameUpdateTab.ForeColor = SystemColors.ControlText;
            GameUpdateTab.Location = new Point(4, 22);
            GameUpdateTab.Name = "GameUpdateTab";
            GameUpdateTab.Padding = new Padding(3);
            GameUpdateTab.Size = new Size(352, 257);
            GameUpdateTab.TabIndex = 0;
            GameUpdateTab.Text = "Game Updates";
            // 
            // GUExportButton
            // 
            GUExportButton.DialogResult = DialogResult.Cancel;
            GUExportButton.Enabled = false;
            GUExportButton.Location = new Point(172, 231);
            GUExportButton.Name = "GUExportButton";
            GUExportButton.Size = new Size(174, 23);
            GUExportButton.TabIndex = 8;
            GUExportButton.Text = "Export Title to Debug Log";
            GUExportButton.UseVisualStyleBackColor = true;
            GUExportButton.Visible = false;
            GUExportButton.Click += new EventHandler(GUExportButton_Click);
            // 
            // GUTitleIDLabel
            // 
            GUTitleIDLabel.AutoSize = true;
            GUTitleIDLabel.Location = new Point(9, 233);
            GUTitleIDLabel.Name = "GUTitleIDLabel";
            GUTitleIDLabel.Size = new Size(44, 13);
            GUTitleIDLabel.TabIndex = 7;
            GUTitleIDLabel.Text = "Title ID:";
            // 
            // NUSGrabberProgress
            // 
            NUSGrabberProgress.Location = new Point(56, 6);
            NUSGrabberProgress.Name = "NUSGrabberProgress";
            NUSGrabberProgress.Size = new Size(290, 20);
            NUSGrabberProgress.TabIndex = 6;
            NUSGrabberProgress.Visible = false;
            // 
            // GUVersionList
            // 
            GUVersionList.FormattingEnabled = true;
            GUVersionList.Location = new Point(289, 53);
            GUVersionList.Name = "GUVersionList";
            GUVersionList.Size = new Size(57, 173);
            GUVersionList.TabIndex = 5;
            // 
            // GUVersionsLabel
            // 
            GUVersionsLabel.AutoSize = true;
            GUVersionsLabel.Location = new Point(292, 37);
            GUVersionsLabel.Name = "GUVersionsLabel";
            GUVersionsLabel.Size = new Size(50, 13);
            GUVersionsLabel.TabIndex = 4;
            GUVersionsLabel.Text = "Versions:";
            // 
            // GUTitlesLabel
            // 
            GUTitlesLabel.AutoSize = true;
            GUTitlesLabel.Location = new Point(120, 37);
            GUTitlesLabel.Name = "GUTitlesLabel";
            GUTitlesLabel.Size = new Size(35, 13);
            GUTitlesLabel.TabIndex = 3;
            GUTitlesLabel.Text = "Titles:";
            // 
            // GUSearchLabel
            // 
            GUSearchLabel.AutoSize = true;
            GUSearchLabel.Location = new Point(6, 9);
            GUSearchLabel.Name = "GUSearchLabel";
            GUSearchLabel.Size = new Size(44, 13);
            GUSearchLabel.TabIndex = 2;
            GUSearchLabel.Text = "Search:";
            // 
            // GUSearchBox
            // 
            GUSearchBox.Location = new Point(56, 6);
            GUSearchBox.Name = "GUSearchBox";
            GUSearchBox.Size = new Size(290, 20);
            GUSearchBox.TabIndex = 1;
            GUSearchBox.TextChanged += new EventHandler(GUSearchBox_TextChanged);
            // 
            // GUTitleList
            // 
            GUTitleList.FormattingEnabled = true;
            GUTitleList.Location = new Point(6, 53);
            GUTitleList.Name = "GUTitleList";
            GUTitleList.Size = new Size(277, 173);
            GUTitleList.TabIndex = 0;
            GUTitleList.SelectedIndexChanged += new EventHandler(GUTitleList_SelectedIndexChanged);
            // 
            // SystemTab
            // 
            SystemTab.BackColor = Color.LightCoral;
            SystemTab.Controls.Add(STExportButton);
            SystemTab.Controls.Add(STTitleIDLabel);
            SystemTab.Controls.Add(STVersionList);
            SystemTab.Controls.Add(STVersionsLabel);
            SystemTab.Controls.Add(STTitlesLabel);
            SystemTab.Controls.Add(STSearchLabel);
            SystemTab.Controls.Add(STSearchBox);
            SystemTab.Controls.Add(STTitleList);
            SystemTab.Location = new Point(4, 22);
            SystemTab.Name = "SystemTab";
            SystemTab.Padding = new Padding(3);
            SystemTab.Size = new Size(352, 257);
            SystemTab.TabIndex = 1;
            SystemTab.Text = "System Titles";
            // 
            // STExportButton
            // 
            STExportButton.DialogResult = DialogResult.Cancel;
            STExportButton.Enabled = false;
            STExportButton.Location = new Point(172, 231);
            STExportButton.Name = "STExportButton";
            STExportButton.Size = new Size(174, 23);
            STExportButton.TabIndex = 13;
            STExportButton.Text = "Export Title to Debug Log";
            STExportButton.UseVisualStyleBackColor = true;
            STExportButton.Visible = false;
            STExportButton.Click += new EventHandler(STExportButton_Click);
            // 
            // STTitleIDLabel
            // 
            STTitleIDLabel.AutoSize = true;
            STTitleIDLabel.Location = new Point(9, 233);
            STTitleIDLabel.Name = "STTitleIDLabel";
            STTitleIDLabel.Size = new Size(44, 13);
            STTitleIDLabel.TabIndex = 12;
            STTitleIDLabel.Text = "Title ID:";
            // 
            // STVersionList
            // 
            STVersionList.FormattingEnabled = true;
            STVersionList.Location = new Point(272, 53);
            STVersionList.Name = "STVersionList";
            STVersionList.Size = new Size(74, 173);
            STVersionList.TabIndex = 11;
            // 
            // STVersionsLabel
            // 
            STVersionsLabel.AutoSize = true;
            STVersionsLabel.Location = new Point(285, 37);
            STVersionsLabel.Name = "STVersionsLabel";
            STVersionsLabel.Size = new Size(50, 13);
            STVersionsLabel.TabIndex = 10;
            STVersionsLabel.Text = "Versions:";
            // 
            // STTitlesLabel
            // 
            STTitlesLabel.AutoSize = true;
            STTitlesLabel.Location = new Point(111, 37);
            STTitlesLabel.Name = "STTitlesLabel";
            STTitlesLabel.Size = new Size(35, 13);
            STTitlesLabel.TabIndex = 9;
            STTitlesLabel.Text = "Titles:";
            // 
            // STSearchLabel
            // 
            STSearchLabel.AutoSize = true;
            STSearchLabel.Location = new Point(6, 9);
            STSearchLabel.Name = "STSearchLabel";
            STSearchLabel.Size = new Size(44, 13);
            STSearchLabel.TabIndex = 8;
            STSearchLabel.Text = "Search:";
            // 
            // STSearchBox
            // 
            STSearchBox.Location = new Point(56, 6);
            STSearchBox.Name = "STSearchBox";
            STSearchBox.Size = new Size(290, 20);
            STSearchBox.TabIndex = 7;
            STSearchBox.TextChanged += new EventHandler(STSearchBox_TextChanged);
            // 
            // STTitleList
            // 
            STTitleList.FormattingEnabled = true;
            STTitleList.Location = new Point(6, 53);
            STTitleList.Name = "STTitleList";
            STTitleList.Size = new Size(260, 173);
            STTitleList.TabIndex = 6;
            STTitleList.SelectedIndexChanged += new EventHandler(STTitleList_SelectedIndexChanged);
            // 
            // FullListTab
            // 
            FullListTab.BackColor = Color.Turquoise;
            FullListTab.Controls.Add(FTExportButton);
            FullListTab.Controls.Add(FTTitleIDLabel);
            FullListTab.Controls.Add(FTTitlesLabel);
            FullListTab.Controls.Add(FTSearchLabel);
            FullListTab.Controls.Add(FTSearchBox);
            FullListTab.Controls.Add(FTTitleList);
            FullListTab.Location = new Point(4, 22);
            FullListTab.Name = "FullListTab";
            FullListTab.Padding = new Padding(3);
            FullListTab.Size = new Size(352, 257);
            FullListTab.TabIndex = 2;
            FullListTab.Text = "Full Titles";
            // 
            // FTExportButton
            // 
            FTExportButton.DialogResult = DialogResult.Cancel;
            FTExportButton.Enabled = false;
            FTExportButton.Location = new Point(172, 231);
            FTExportButton.Name = "FTExportButton";
            FTExportButton.Size = new Size(174, 23);
            FTExportButton.TabIndex = 11;
            FTExportButton.Text = "Export Title to Debug Log";
            FTExportButton.UseVisualStyleBackColor = true;
            FTExportButton.Visible = false;
            FTExportButton.Click += new EventHandler(FTExportButton_Click);
            // 
            // FTTitleIDLabel
            // 
            FTTitleIDLabel.AutoSize = true;
            FTTitleIDLabel.Location = new Point(9, 233);
            FTTitleIDLabel.Name = "FTTitleIDLabel";
            FTTitleIDLabel.Size = new Size(44, 13);
            FTTitleIDLabel.TabIndex = 10;
            FTTitleIDLabel.Text = "Title ID:";
            // 
            // FTTitlesLabel
            // 
            FTTitlesLabel.AutoSize = true;
            FTTitlesLabel.Location = new Point(152, 37);
            FTTitlesLabel.Name = "FTTitlesLabel";
            FTTitlesLabel.Size = new Size(35, 13);
            FTTitlesLabel.TabIndex = 9;
            FTTitlesLabel.Text = "Titles:";
            // 
            // FTSearchLabel
            // 
            FTSearchLabel.AutoSize = true;
            FTSearchLabel.Location = new Point(6, 9);
            FTSearchLabel.Name = "FTSearchLabel";
            FTSearchLabel.Size = new Size(44, 13);
            FTSearchLabel.TabIndex = 8;
            FTSearchLabel.Text = "Search:";
            // 
            // FTSearchBox
            // 
            FTSearchBox.Location = new Point(56, 6);
            FTSearchBox.Name = "FTSearchBox";
            FTSearchBox.Size = new Size(290, 20);
            FTSearchBox.TabIndex = 7;
            FTSearchBox.TextChanged += new EventHandler(FTSearchBox_TextChanged);
            // 
            // FTTitleList
            // 
            FTTitleList.FormattingEnabled = true;
            FTTitleList.Location = new Point(6, 53);
            FTTitleList.Name = "FTTitleList";
            FTTitleList.Size = new Size(340, 173);
            FTTitleList.TabIndex = 6;
            FTTitleList.SelectedIndexChanged += new EventHandler(FTTitleList_SelectedIndexChanged);
            // 
            // DownloadButton
            // 
            DownloadButton.DialogResult = DialogResult.Cancel;
            DownloadButton.Enabled = false;
            DownloadButton.Location = new Point(300, 301);
            DownloadButton.Name = "DownloadButton";
            DownloadButton.Size = new Size(72, 23);
            DownloadButton.TabIndex = 3;
            DownloadButton.Text = "Download";
            DownloadButton.UseVisualStyleBackColor = true;
            DownloadButton.Click += new EventHandler(DownloadButton_Click);
            // 
            // UpdateButton
            // 
            UpdateButton.Enabled = false;
            UpdateButton.Location = new Point(72, 301);
            UpdateButton.Name = "UpdateButton";
            UpdateButton.Size = new Size(110, 23);
            UpdateButton.TabIndex = 4;
            UpdateButton.Text = "Update VersionList";
            UpdateButton.UseVisualStyleBackColor = true;
            UpdateButton.Click += new EventHandler(UpdateButton_Click);
            // 
            // AboutButton
            // 
            AboutButton.Enabled = false;
            AboutButton.Location = new Point(12, 301);
            AboutButton.Name = "AboutButton";
            AboutButton.Size = new Size(54, 23);
            AboutButton.TabIndex = 5;
            AboutButton.Text = "Settings";
            AboutButton.UseVisualStyleBackColor = true;
            AboutButton.Click += new EventHandler(AboutButton_Click);
            // 
            // ReloadButton
            // 
            ReloadButton.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            ReloadButton.Location = new Point(288, 10);
            ReloadButton.Name = "ReloadButton";
            ReloadButton.Size = new Size(82, 22);
            ReloadButton.TabIndex = 6;
            ReloadButton.Text = "Load Titles";
            ReloadButton.TextAlign = ContentAlignment.TopCenter;
            ReloadButton.UseVisualStyleBackColor = true;
            ReloadButton.Click += new EventHandler(ReloadButton_Click);
            // 
            // DecryptButton
            // 
            DecryptButton.Enabled = false;
            DecryptButton.Location = new Point(188, 301);
            DecryptButton.Name = "DecryptButton";
            DecryptButton.Size = new Size(106, 23);
            DecryptButton.TabIndex = 7;
            DecryptButton.Text = "Decrypt Download";
            DecryptButton.UseVisualStyleBackColor = true;
            DecryptButton.Click += new EventHandler(DecryptButton_Click);
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.FileName = "ckey.bin";
            // 
            // EmbedNUSGrabber
            // 
            EmbedNUSGrabber.DoWork += new DoWorkEventHandler(EmbedNUSGrabber_Work);
            EmbedNUSGrabber.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EmbedNUSGrabber_Done);
            // 
            // NUSGrabberForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(384, 336);
            Controls.Add(DecryptButton);
            Controls.Add(ReloadButton);
            Controls.Add(AboutButton);
            Controls.Add(UpdateButton);
            Controls.Add(DownloadButton);
            Controls.Add(NUSTabs);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "NUSGrabberForm";
            ShowIcon = false;
            Text = "NEW-NUSGrabberGUI";
            NUSTabs.ResumeLayout(false);
            GameUpdateTab.ResumeLayout(false);
            GameUpdateTab.PerformLayout();
            SystemTab.ResumeLayout(false);
            SystemTab.PerformLayout();
            FullListTab.ResumeLayout(false);
            FullListTab.PerformLayout();
            ResumeLayout(false);

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
    }
}

