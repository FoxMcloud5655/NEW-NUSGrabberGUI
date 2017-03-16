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
            LanguageBox.Text = Properties.Settings.Default.Language;
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
            if (!LanguageBox.Items.Contains(LanguageBox.Text))
                if (MessageBox.Show("You have entered in a manual override to the language.  If not an actual language, this can and " +
                    "will cause errors.  Are you sure you want to do this?", "Confirm Custom Language",
                    MessageBoxButtons.YesNo) == DialogResult.No) { LanguageBox.Text = "en"; return; }
            Properties.Settings.Default.Region = RegionBox.Text;
            Properties.Settings.Default.Language = LanguageBox.Text;
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

        private void FAQButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "How to download a game update:\n" + 
                "1.  Update your versionlists by hitting \"Update Versionlists\" on the main screen of the program.  " +
                "(You can skip updating the versionlists if you know they are up to date.)\n" +
                "2.  Search for the title that you want to update and click on it.\n" +
                "3.  Select a version from the list on the right side. (This step is optional if you want the latest version.)\n" +
                "4.  Hit the download button and wait.\n\n" +

                "How to install a game update that was downloaded:\n" +
                "You'll need to put ALL the files that you downloaded (stored in the root directory of the program, in a subdirectory " +
                "with the titleid, and optionally a version if you selected one. Example: \"\\rootdir\\titleid\\version\\\") " +
                "from this program (WITHOUT decrypting them) onto your SD Card in a folder called \"install\".  So, let's say that " +
                "your SD Card was mounted as the F: drive.  An example of how the folder structure should look like this: (F:\\install\\)\n\n" +

                "How to decrypt a game update for use with Loadiine:\n" +
                "1.  Hit the decrypt button.\n" +
                "2.  Navigate to folder which was just created (if you selected a specific version, then it would be \"\\rootdir\\titleid\\version\\\", " + 
                "but if you got the latest version, then it would be \"\\rootdir\\titleid\\\").\n" +
                "3.  Select the \".bin\" of the Wii U Common key (which you have to obtain yourself).\n" +
                "4.  Wait for the decryption to complete.\n\n" +

                "How to install a decrypted game update:\n" +
                "1.  Copy all of the folders that were created in the decrypt process (located in the root directory of the program, in a " +
                "subdirectory with the titleid, and optionally a version if you selected one. [Example: \"\\rootdir\\titleid\\version\\\".  " +
                "If you got the latest version, then instead it would be \"\\rootdir\\titleid\\\"])\n" +
                "2.  Optionally, make a backup of the original files for the decrypted game on your SD Card.\n" +
                "3.  Paste and overwrite the folders inside the game you are updating on your SD Card.\n\n" +

                "How to download a system title: (Instructions are a work in progress.)\n" +
                "Coming Soon...  This feature isn't ready yet, but if you'd like to speed this part up, let me know on GitHub by opening an issue!\n\n" +

                "How to download a full title:\n" +
                "1.  Search for the title that you want to download and click on it.\n" +
                "2.  Hit the download button and wait.\n" +
                "Decrypting the full title uses the same process as the game updates.\n\n" +

                "How to install a full title: (Instructions are a work in progress.)\n" +
                "1.  Copy all the folders that were created when you decrypted the title (stored in the root directory of the program, in a " +
                "subdirectory with the titleid. Example: \"\\rootdir\\titleid\\\").\n" +
                "2.  Create a new directory on your SD Card where Loadiine loads from.\n" +
                "3.  Paste the folders in your newly created directory.\n\n" +

                "If a question you have isn't answered here, hit me up on GBATemp!  My username is FoxMcloud5655.");
        }
    }
}
