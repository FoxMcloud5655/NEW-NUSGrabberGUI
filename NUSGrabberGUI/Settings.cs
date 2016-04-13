using System;
using System.Windows.Forms;

namespace NUSGrabberGUI
{
    public partial class SettingsForm : Form
    {
        public SettingsForm(bool debug)
        {
            InitializeComponent();
            UpdateButton.Enabled = !Properties.Settings.Default.Debug;
            RegionBox.Text = Properties.Settings.Default.Region;
            HideCheckBox.Checked = Properties.Settings.Default.HideNUS;
            LoadCheckBox.Checked = Properties.Settings.Default.LoadTitles;
            DecryptCheckBox.Checked = Properties.Settings.Default.AutoDecrypt;
            EmbedCheckBox.Checked = Properties.Settings.Default.UseEmbedNUS;
            ArchivedCheckBox.Checked = Properties.Settings.Default.ArchivedDatabase;
            if (Properties.Settings.Default.Debug) Properties.Settings.Default.Cleanup = false;
            CleanupCheckBox.Checked = Properties.Settings.Default.Cleanup;
            OrigCheckBox.Checked = Properties.Settings.Default.UseOrigNUS;
            CleanupCheckBox.Enabled = !debug;
            EmbedCheckBox.Visible = Properties.Settings.Default.Debug;
            HideCheckBox.Enabled = !EmbedCheckBox.Checked;
        }

        private void SaveCloseButton_Click(object sender, EventArgs e)
        {
            if (!RegionBox.Items.Contains(RegionBox.Text))
                if (MessageBox.Show("You have entered in a manual override to the region.  If not an actual region, this can and will " +
                    "cause errors.  Are you sure you want to do this?", "Confirm Non-Standard Region",
                    MessageBoxButtons.YesNo) == DialogResult.No) { RegionBox.Text = "USA"; return; }
            Properties.Settings.Default.Region = RegionBox.Text;
            Properties.Settings.Default.HideNUS = HideCheckBox.Checked;
            Properties.Settings.Default.AutoDecrypt = DecryptCheckBox.Checked;
            Properties.Settings.Default.LoadTitles = LoadCheckBox.Checked;
            Properties.Settings.Default.UseEmbedNUS = EmbedCheckBox.Checked;
            Properties.Settings.Default.ArchivedDatabase = ArchivedCheckBox.Checked;
            Properties.Settings.Default.Cleanup = CleanupCheckBox.Checked;
            Properties.Settings.Default.UseOrigNUS = OrigCheckBox.Checked;
            Properties.Settings.Default.Save();
            Close();
        }

        private void AboutButton_Click(object sender, EventArgs e)
        {
            float version_float = (float)Properties.Settings.Default.Version / 100;
            MessageBox.Show("Created by: FoxMcloud5655\nNUSgrabber/CDecrypt by: crediar\nOriginal GUI by: Adr990\n\nVersion " +
                version_float + ' ' + Properties.Settings.Default.VersionType + (Properties.Settings.Default.Debug ? " DEBUG" : ""));
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            NUSGrabberForm update = new NUSGrabberForm(true);
            update.CheckForUpdates(false);
            update.Close();
        }

        private void EmbedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EmbedCheckBox.Checked)
            {
                if (MessageBox.Show("This is an experimental feature which is not useful to the general public right now, as it " + 
                    "is configured to use the old method of file storage.  Are you sure you want to disable using NUSGrabber.exe?",
                    "Confirm Experimental Testing", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    EmbedCheckBox.Checked = false;
                }
            }
            HideCheckBox.Enabled = !EmbedCheckBox.Checked;
        }
    }
}
