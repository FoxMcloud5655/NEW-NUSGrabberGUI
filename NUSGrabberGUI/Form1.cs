using libWiiSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;

namespace NUSGrabberGUI
{
    public partial class NUSGrabberForm : Form
    {
        #region Global Variables
        #if DEBUG
            bool debug = true;
        #else
            bool debug = false;
        #endif
        List<ListItem> GameUpdates = new List<ListItem>();
        List<ListItem> SystemTitles = new List<ListItem>();
        List<int> badversionlists = new List<int> {1, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 280, 281,
            282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 299, 300, 301, 302, 303, 304, 305, 306,
            307, 308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 326, 327, 328,
            329, 330, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 655, 656, 657, 658, 659, 660, 661, 662, 663, 664};
        List<int> keyHashes = new List<int> { 487391367, -1394384166, 585460703 };
        SortedDictionary<string, List<string>> titlelist = new SortedDictionary<string, List<string>>();
        FileSystemWatcher DownloadWatcher = new FileSystemWatcher();
        FrmDownloadLog DownloadLog = new FrmDownloadLog();
        bool canwritedebug = true;
        string filepath = "";
        string language = "";
        #endregion

        #region Public Fuctions

        public NUSGrabberForm()
        {
            DownloadWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            Properties.Settings.Default.Debug = debug;
            Application.ApplicationExit += CleanupOnExit;
            Application.Idle += LoadTitleInfo;
            if (File.Exists("NEW-NUSGrabberGUI.exe"))
            {
                ExtractResources();
                try
                {
                    language = File.ReadAllText("lang/" + Properties.Settings.Default.Language + ".resx");
                }
                catch
                {
                    MessageBox.Show("Can't find specified language file.  Program will now exit and default to english.");
                    Properties.Settings.Default.Language = "en";
                    Properties.Settings.Default.Save();
                    Process.GetCurrentProcess().Kill();
                }
                if (File.Exists("HtmlAgilityPack.dll"))
                {
                    CheckForUpdates(true);
                    InitializeComponent();
                    EnableUI(true, true);
                    if (debug)
                    {
                        WriteDebugLog("\nNEW-NUSGrabberGUI v" + (float)Properties.Settings.Default.Version / 100 + " " +
                            Properties.Settings.Default.VersionType + GetLanguageString("debug_start", true) +
                            DateTime.Now.ToString() + "\n", false);
                        Text += " DEBUG";
                        GUExportButton.Visible = true;
                        STExportButton.Visible = true;
                        FTExportButton.Visible = true;
                    }
                }
                else
                {
                    MessageBox.Show("HtmlAgilityPack.dll " + GetLanguageString("not_found", false));
                    Process.GetCurrentProcess().Kill();
                }
            }
            else if (File.Exists("NUSGrabberGUI.exe") || File.Exists("NUtSGrabberGUI.exe") || File.Exists("DeezNUSGrabberGUI.exe"))
            {
                Properties.Settings.Default.Debug = true;
                ExtractResources();
                if (File.Exists("HtmlAgilityPack.dll"))
                {
                    InitializeComponent();
                    EnableUI(true, true);
                    Text += " PSUDO-DEBUG";
                    GUExportButton.Visible = true;
                    STExportButton.Visible = true;
                    FTExportButton.Visible = true;
                    WriteDebugLog("\nNEW-NUSGrabberGUI v" + (float)Properties.Settings.Default.Version / 100 + " " +
                            Properties.Settings.Default.VersionType + GetLanguageString("debug_start_emu", true) +
                            DateTime.Now.ToString() + "\n", false);
                }
                else
                {
                    MessageBox.Show("HtmlAgilityPack.dll " + GetLanguageString("not_found", false));
                    Process.GetCurrentProcess().Kill();
                }
            }
            else
            {
                MessageBox.Show(GetLanguageString("dont_rename", false) + " NEW-NUSGrabberGUI.exe!");
                Process.GetCurrentProcess().Kill();
            }
        }

        public NUSGrabberForm(bool temp) { }

        private void LoadTitleInfo(object sender, EventArgs e)
        {
            Application.Idle -= LoadTitleInfo;
            if (Properties.Settings.Default.LoadTitles) GetTitleInfo();
        }

        #endregion

        #region Search and Version Processing

        private void GUTitleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            GUVersionList.BeginUpdate();
            GUVersionList.Items.Clear();
            GUExportButton.Enabled = true;
            string versions = (GUTitleList.SelectedItem as ListItem).Versions.ToString();
            GUVersionList.Items.Add("Latest");
            if (versions != "-")
            {
                string[] versions_split = versions.Split(',');
                foreach (string v in versions_split)
                {
                    if (v != null && v != "" && v != " ")
                    {
                        GUVersionList.Items.Add(v.TrimStart(' '));
                    }
                }
            }
            GUVersionList.SelectedIndex = 0;
            try { GUTitleIDLabel.Text = "Title ID: " + (GUTitleList.SelectedItem as ListItem).Title_ID.ToString(); }
            catch { }
            GUVersionList.EndUpdate();
        }

        private void GUSearchBox_TextChanged(object sender, EventArgs e)
        {
            if (!GUSearchBox.Text.Contains("..."))
            {
                GUTitleList.BeginUpdate();
                GUTitleList.Items.Clear();
                GUExportButton.Enabled = false;
                string region = Properties.Settings.Default.Region;
                if (GUSearchBox.Text != "")
                {
                    foreach (ListItem t in GameUpdates)
                        if (t.Desc != null && t.Versions.ToString() != "v1" && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                            if (t.Desc.IndexOf(GUSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                t.Title_ID.ToString().IndexOf(GUSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                                GUTitleList.Items.Add(t);
                    if (GUTitleList.Items.Count != 0)
                    {
                        GUTitleList.SelectedIndex = 0;
                        EnableUI(true);
                    }
                    else
                    {
                        GUVersionList.Items.Clear();
                        GUTitleIDLabel.Text = "Title ID:";
                        DownloadButton.Enabled = false;
                    }
                }
                else
                {
                    foreach (ListItem t in GameUpdates)
                        if (t.Desc != null && t.Versions.ToString() != "v1" && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                            GUTitleList.Items.Add(t);
                    if (GUTitleList.Items.Count != 0)
                    {
                        GUTitleList.SelectedIndex = 0;
                        EnableUI(true);
                    }
                }

                GUTitleList.EndUpdate();
            }
        }

        private void STTitleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            STVersionList.BeginUpdate();
            STVersionList.Items.Clear();
            STExportButton.Enabled = true;
            string versions;
            try { versions = (STTitleList.SelectedItem as ListItem).Versions.ToString(); }
            catch { versions = null; }
            STVersionList.Items.Add("Latest");
            string[] versions_split = versions.Split(',');
            foreach (string v in versions_split)
            {
                if (v != null && v != "v0" && v != "" && v != " ")
                {
                    try { STVersionList.Items.Add(v.TrimStart(' ').Substring(0, (v.LastIndexOf('(') > -1 ? v.LastIndexOf('(') : v.Length) - 1)); }
                    catch { STVersionList.Items.Add(v.TrimStart(' ')); }
                }
            }
            STVersionList.SelectedIndex = 0;
            try { STTitleIDLabel.Text = "Title ID: " + (STTitleList.SelectedItem as ListItem).Title_ID.ToString(); }
            catch { }
            STVersionList.EndUpdate();
        }

        private void STSearchBox_TextChanged(object sender, EventArgs e)
        {
            EnableUI(false);
            STTitleList.BeginUpdate();
            STTitleList.Items.Clear();
            STExportButton.Enabled = false;
            string region = Properties.Settings.Default.Region;
            if (STSearchBox.Text != "")
            {
                foreach (ListItem t in SystemTitles)
                    if (t.Desc != null && t.Versions.ToString() != "v1" && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                        if (t.Desc.IndexOf(STSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                t.Title_ID.ToString().IndexOf(GUSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                            STTitleList.Items.Add(t);
                if (STTitleList.Items.Count != 0)
                {
                    STTitleList.SelectedIndex = 0;
                }
                else
                {
                    STVersionList.Items.Clear();
                    STTitleIDLabel.Text = "Title ID:";
                    DownloadButton.Enabled = false;
                }
            }
            else
            {
                foreach (ListItem t in SystemTitles)
                    if (t.Desc != null && t.Versions.ToString() != "v1" && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                        STTitleList.Items.Add(t);
                if (STTitleList.Items.Count != 0)
                {
                    STTitleList.SelectedIndex = 0;
                }
            }

            EnableUI(true);
            STTitleList.EndUpdate();
        }

        private void FTTitleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            FTExportButton.Enabled = true;
            try { FTTitleIDLabel.Text = "Title ID: " + (FTTitleList.SelectedItem as ListItem).Title_ID.ToString(); }
            catch { }
        }

        private void FTSearchBox_TextChanged(object sender, EventArgs e)
        {
            FTTitleList.BeginUpdate();
            FTTitleList.Items.Clear();
            FTExportButton.Enabled = false;
            string region = Properties.Settings.Default.Region;
            if (FTSearchBox.Text != "")
            {
                foreach (ListItem t in GameUpdates)
                    if (t.Desc != null && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                        if (t.Desc.IndexOf(FTSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                t.Title_ID.ToString().IndexOf(GUSearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                            FTTitleList.Items.Add(t);
                if (FTTitleList.Items.Count != 0)
                {
                    GUTitleList.SelectedIndex = 0;
                    EnableUI(true);
                }
                else
                {
                    GUVersionList.Items.Clear();
                    FTTitleIDLabel.Text = "Title ID:";
                    DownloadButton.Enabled = false;
                }
            }
            else
            {
                foreach (ListItem t in GameUpdates)
                    if (t.Desc != null && (t.Desc.ToString().StartsWith(region) || t.Desc.ToString().StartsWith("all")))
                        FTTitleList.Items.Add(t);
                if (FTTitleList.Items.Count != 0)
                {
                    FTTitleList.SelectedIndex = 0;
                    EnableUI(true);
                }
            }
            FTTitleList.EndUpdate();
        }

        private void NUSTabs_IndexChanged(object sender, EventArgs e)
        {
            //This is to enable/disable the download button every time you switch the tab.
            switch (NUSTabs.SelectedIndex)
            {
                case 0:
                    GUSearchBox_TextChanged(sender, e);
                    break;
                case 1:
                    STSearchBox_TextChanged(sender, e);
                    break;
                case 2:
                    FTSearchBox_TextChanged(sender, e);
                    break;
            }
        }

        #endregion

        #region Button Processing

        private void DownloadButton_Click(object sender, EventArgs e)
        {
            string[] args = { "", "" };
            if (NUSTabs.SelectedTab.TabIndex == 0)
            {
                try
                {
                    args[0] = "0005000E";
                    args[0] += (GUTitleList.SelectedItem as ListItem).Title_ID.ToString();
                    string selectversion = GUVersionList.SelectedItem.ToString();
                    if (selectversion != "Latest")
                    {
                        args[1] = selectversion.TrimStart('v');
                    }
                }
                catch { args = null; }
            }
            else if (NUSTabs.SelectedTab.TabIndex == 1)
            {
                try
                {
                    args[0] = "00050010";
                    args[0] += (STTitleList.SelectedItem as ListItem).Title_ID.ToString();
                    string selectversion = STVersionList.SelectedItem.ToString();
                    if (selectversion != "Latest")
                    {
                        args[1] = selectversion.TrimStart('v');
                    }
                    throw new NotImplementedException();
                }
                catch
                {
                    args = null; MessageBox.Show("Feature not implemented yet.  Select a game update or full title instead.");
                }
            }
            else if (NUSTabs.SelectedTab.TabIndex == 2)
            {
                try
                {
                    args[0] = "00050000";
                    args[0] += (FTTitleList.SelectedItem as ListItem).Title_ID.ToString();
                }
                catch { args = null; }
            }
            if (args != null && args[0] != "")
            {

                filepath = Environment.CurrentDirectory + '\\' + args[0] + '\\' + (!args[1].Equals("") ? args[1] + '\\' : "");
                if (!Directory.Exists(filepath))
                {
                    if (!File.Exists(filepath + "title.tmd"))
                    {
                        EnableUI(false);
                        NUSTabs.SelectedIndex = 0;
                        ForceRefresh(GUSearchBox, "Downloading...  Please wait.");
                        if (Properties.Settings.Default.UseEmbedNUS)
                        {
                            EmbedNUSGrabber.RunWorkerAsync(args);
                        }
                        else
                        {
                            Process nusgrabber = new Process();
                            nusgrabber.StartInfo.FileName = "NUSGrabber.exe";
                            nusgrabber.StartInfo.Arguments = args[0] + (args[1] != "" ? ' ' + args[1] : "");
                            nusgrabber.StartInfo.RedirectStandardOutput = true;
                            nusgrabber.StartInfo.RedirectStandardError = true;
                            nusgrabber.OutputDataReceived += Nusgrabber_OutputDataReceived;
                            nusgrabber.ErrorDataReceived += Nusgrabber_OutputDataReceived;
                            nusgrabber.StartInfo.UseShellExecute = false;
                            nusgrabber.StartInfo.CreateNoWindow = Properties.Settings.Default.HideNUS;
                            try
                            {
                                WriteDebugLog("Assumed path to the downloaded files is \"" + filepath + '\"');
                                WriteDebugLog("Starting \"" + nusgrabber.StartInfo.FileName + "\" with \"" + nusgrabber.StartInfo.Arguments + "\" as arguments.");
                                DownloadLog.Show(this);
                                DownloadLog.ClearLog();
                                nusgrabber.Start();                                
                                nusgrabber.BeginOutputReadLine();
                                nusgrabber.BeginErrorReadLine();
                                System.Threading.Thread.Sleep(2000);
                                while (!nusgrabber.HasExited)
                                {
                                    try
                                    {
                                        FileInfo DownloadedFile = GetFiles(filepath, ".app", ".h3").OrderByDescending(f => f.LastWriteTime).First();
                                        if (!DownloadedFile.Equals(null))
                                        {
                                            ForceRefresh(GUSearchBox, "Downloading: " + DownloadedFile.Name + "...");
                                        }
                                    }
                                    catch { System.Threading.Thread.Sleep(500); }
                                }
                                WriteDebugLog("Detected that NUSGrabber.exe has closed with exit code of " + nusgrabber.ExitCode + ".  Preforming after-processing.");
                                if (nusgrabber.ExitCode == 1)
                                {
                                    if (!Directory.Exists(filepath))
                                    {
                                        MessageBox.Show("Title not found on Nintendo's servers.");
                                        WriteDebugLog("Title not found.");
                                    }
                                    else if (File.Exists(filepath + "title.tmd"))
                                    {
                                        if (Properties.Settings.Default.AutoDecrypt && File.Exists(filepath + "title.tik"))
                                        {
                                            WriteDebugLog("Auto-decrypting title.");
                                            DecryptButton_Click(sender, e);
                                        }
                                        else if (Properties.Settings.Default.AutoDecrypt)
                                        {
                                            MessageBox.Show("Title successfully downloaded, but no decryption key was found.  Can't decrypt automatically.");
                                            WriteDebugLog("Couldn't find \"title.tik\".  Skipping auto-decryption.\nTitle downloaded successfully.");
                                        }
                                        else
                                        {
                                            MessageBox.Show("Title successfully downloaded.");
                                            WriteDebugLog("Title downloaded successfully.");
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Incomplete download detected.  Delete \"" + filepath + "\" and try downloading the title again.");
                                        WriteDebugLog("Possible incomplete download.");
                                    }
                                }
                                else
                                {
                                    if (nusgrabber.ExitCode == -1073741510)
                                    {
                                        MessageBox.Show("NUSGrabber exited without finishing the download.  Please do not exit NUSGrabber while it's downloading!");
                                        WriteDebugLog("User closed NUSGrabber.  Program exited without finishing download.");
                                    }
                                    else if (nusgrabber.ExitCode == -1073741701)
                                    {
                                        MessageBox.Show("Detected that NUSGrabber didn't run.  Install Visual Studio C++ (x86) and try again.");
                                        WriteDebugLog("Detected that NUSGrabber didn't run.  Installation of Visual Studio C++ x86 (32-bit) is required.");
                                        WriteDebugLog("Status Code: STATUS_INVALID_IMAGE_FORMAT");
                                    }
                                    else if (nusgrabber.ExitCode == -1073741515)
                                    {
                                        MessageBox.Show("Unable to locate a .dll file.  Perhaps you are running WINE?");
                                        WriteDebugLog("Detected that NUSGrabber didn't run.  Unable to find a required .dll.");
                                        WriteDebugLog("Status Code: STATUS_DLL_NOT_FOUND");
                                    }
                                    else
                                    {
                                        MessageBox.Show("There was an unknown problem while downloading.");
                                        WriteDebugLog("Unknown error occurred while downloading.");
                                    }
                                }
                                GUSearchBox.Text = "";
                                filepath = "";
                                EnableUI(true);

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("There was a problem starting NUSgrabber.  Ensure that you have NUSgrabber in the root " +
                                    "directory that this program is stored in.\n" + ex.ToString());
                                WriteDebugLog("Error starting " + nusgrabber.StartInfo.FileName + ".\n" + ex.ToString());
                            }
                            finally
                            {
                                DownloadLog.Hide();
                            }
                        }
                    }
                }
                else if (File.Exists(filepath + "tmd") || File.Exists(filepath + "DELETEME"))
                {
                    if (File.Exists(filepath + "tmd")) WriteDebugLog("Incomplete download detected.  Deleting previous files.");
                    List<FileInfo> files = GetFiles(filepath, ".h3", ".app", "", ".tmd", ".tik", ".cert");
                    foreach (FileInfo file in files)
                        DeleteFile(file.FullName);
                    try
                    {
                        Directory.Delete(filepath);
                        DownloadButton_Click(sender, e);
                    }
                    catch
                    {
                        Process.Start("explorer.exe", filepath);
                        MessageBox.Show("Delete Incomplete Folder", "Couldn't remove folder after deletion.  Ensure there is no other data in \"" + filepath + "\" and try again.", MessageBoxButtons.OK, MessageBoxIcon.Error);                        
                        WriteDebugLog("Couldn't delete directory " + filepath + ".");
                    }
                }
                else if (MessageBox.Show("You have already downloaded this title.  Are you sure you wish to redownload it?",
                    "Confirm Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    try
                    {
                        File.Create(filepath + "DELETEME");
                        WriteDebugLog("Deleting previously downloaded files by user request.");
                        DownloadButton_Click(sender, e);
                    }
                    catch { WriteDebugLog("Couldn't access " + filepath + " for deletion."); }
                }
            }
            else
            {
                MessageBox.Show("There was a problem converting the selected item.  Note your selected item, contact " +
                    "FoxMcloud5655 on GBATemp, and tell him to look into this problem!");
                WriteDebugLog("Error converting title name into a ListItem.");
            }
        }

        private void Nusgrabber_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            DownloadLog.AppendLog(e.Data);
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Region != "")
            {
                if (MessageBox.Show("Are you sure you want to update the VersionList for all titles in " +
                    Properties.Settings.Default.Region + "?  This will take some time.",
                    "Confirm Update", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    EnableUI(false);
                    NUSTabs.SelectedIndex = 0;
                    string tmp = GUSearchBox.Text;
                    ForceRefresh(GUSearchBox, "Connecting to the Internet...");
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };  //A workaround to get through their secure connection invalidation.
                    titlelist.Clear();
                    bool canwrite = false;
                    try
                    {
                        using (FileStream versionlist_file = File.Open("wiiu_versionlist.txt", FileMode.OpenOrCreate, FileAccess.Write))
                        { }  //This touches the file to make sure we can write to it BEFORE we do all of our processing.
                        canwrite = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to create a new file or overwrite the current one.  Ensure that you have " +
                            "permission to write to the root directory that this GUI is stored in.\n\n" + ex.ToString());
                        WriteDebugLog("Unable to open/create wiiu_versionlist.txt.  Skipping update.");
                    }

                    if (canwrite)
                    {
                        string region = "/" + Properties.Settings.Default.Region + "/" + Properties.Settings.Default.Region.Substring(0, 2) + "/";
                        string request = "https://tagaya.wup.shop.nintendo.net/tagaya/versionlist" + region + "latest_version";
                        WriteDebugLog("Connecting to " + request + ".");
                        WebRequest req = WebRequest.Create(request);
                        WebResponse response = req.GetResponse();
                        XDocument xdoc = XDocument.Load(response.GetResponseStream());
                        int latestversion = 0;
                        string failedversionlists = "";
                        try
                        {
                            latestversion = int.Parse(xdoc.Element("version_list_info").Element("version").Value);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to parse data.  Either you aren't connected to the internet, " +
                                "or Nintendo has changed their format.\n\n" + ex.ToString());
                            WriteDebugLog("Failed to retrieve/parse data from " + request + ".");
                        }
                        if (latestversion > 0)
                        {
                            for (int i = 1; i <= latestversion; i++)
                            {
                                if (!badversionlists.Contains(i))
                                {
                                    ForceRefresh(GUSearchBox, "Parsing list " + i + " of " + latestversion + " for " + region.Substring(1, 3) + "...");
                                    try
                                    {
                                        request = "https://tagaya.wup.shop.nintendo.net/tagaya/versionlist" + region + "list/" + i + ".versionlist";
                                        WriteDebugLog("Retrieving list " + i + " of " + latestversion + " from " + request + ".");
                                        req = WebRequest.Create(request);
                                        response = req.GetResponse();
                                        xdoc = XDocument.Load(response.GetResponseStream());
                                        foreach (XElement title in xdoc.Elements("version_list").Elements("titles").Elements("title"))
                                        {
                                            string id = title.Element("id").Value.Substring(8);
                                            List<string> versions = new List<string>();
                                            foreach (XElement version in title.Elements("version"))
                                            {
                                                if (version.Value != "0")
                                                    versions.Add(version.Value);
                                            }
                                            if (versions.Count != 0)
                                            {
                                                if (titlelist.ContainsKey(id))
                                                {
                                                    List<string> tmplist = titlelist[id];
                                                    titlelist.Remove(id);
                                                    foreach (string tmpstring in tmplist)
                                                    {
                                                        if (!versions.Contains(tmpstring))
                                                            versions.Add(tmpstring);
                                                    }
                                                }
                                                versions.Sort(new SortByNumber());
                                                titlelist.Add(id, versions);
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        //Invalid versionlist, just keep going (and log if debuging).
                                        if (Properties.Settings.Default.Debug)
                                        {
                                            failedversionlists += i + ", ";
                                            WriteDebugLog("Bad versionlist \"" + i + "\".");
                                        }

                                    }
                                }
                            }
                            if (failedversionlists.Contains(","))
                            {
                                FTSearchBox.Text = failedversionlists;
                                EnableUI(true);
                            }
                        }
                        GUSearchBox.Text = tmp;
                        WriteDebugLog("Parse complete.  Writing data to wiiu_versionlist.txt.");
                        try
                        {
                            FileStream versionlist_file = File.Open("wiiu_versionlist.txt", FileMode.Create, FileAccess.Write);
                            StreamWriter versionlist_write = new StreamWriter(versionlist_file);
                            foreach (string id in titlelist.Keys)
                            {
                                versionlist_write.WriteLine(id);
                                foreach (string version in titlelist[id])
                                {
                                    versionlist_write.WriteLine('\t' + version);
                                }
                            }
                            versionlist_write.Flush();
                            versionlist_write.Close();
                            WriteDebugLog("Write successful.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to create a new file or overwrite the current one.  Please don't mess with " +
                                "the program's directories IN THE MIDDLE of an important operation!  You'll have to " +
                                "redownload the versionlists again if you want to save them.\n\n" + ex.ToString());
                            WriteDebugLog("Write unsuccessful.  Keeping versionlists in memory until next reload.");
                        }

                        ReloadButton_Click(sender, e);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please set your region in the settings before selecting this option!");
            }
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            GetTitleInfo();
        }

        private void DecryptButton_Click(object sender, EventArgs e)
        {
            //CDecrypt.exe "title.tmd" "title.tik" "ckey.bin"
            string args = "title.tmd title.tik ckey.bin";
            OpenFileDialog.CheckFileExists = true;
            FolderBrowserDialog titlefolder = new FolderBrowserDialog();
            titlefolder.ShowNewFolderButton = false;
            titlefolder.SelectedPath = Environment.CurrentDirectory;
            string workingdir = Environment.CurrentDirectory;
            DialogResult isOK = new DialogResult();
            if (filepath == "") isOK = titlefolder.ShowDialog();
            if (isOK == DialogResult.OK || filepath != "")
            {
                try
                {
                    if (filepath != "") Environment.CurrentDirectory = filepath;
                    else Environment.CurrentDirectory = titlefolder.SelectedPath;
                    bool continue_executing = true;
                    if (File.Exists("title.tmd"))
                    {
                        if (File.Exists("title.tik"))
                        {
                            bool ckey_exists = true;
                            if (File.Exists("ckey.bin"))
                            {
                                if (MessageBox.Show("ckey.bin is already in the folder set for decryption.  Would you like to delete it?  " +
                                      "If not, the file will be used instead.", "Confirm Deletion", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    DeleteFile("ckey.bin");
                                }
                            }
                            if (!File.Exists("ckey.bin") && !File.Exists(Properties.Settings.Default.CommonKeyPath))
                            {
                                ckey_exists = false;
                                OpenFileDialog.Filter = "WiiU Common Key|*.bin";
                                OpenFileDialog.FileName = "ckey.bin";
                                if (OpenFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    try
                                    {
                                        Properties.Settings.Default.CommonKeyPath = OpenFileDialog.FileName;
                                        Properties.Settings.Default.Save();
                                        File.Copy(OpenFileDialog.FileName, "ckey.bin");
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Unable to copy common key.  Ensure you have access to read and write to the " +
                                        "directory that you specified and that your common key actually exists.");
                                        WriteDebugLog("Couldn't copy common key to " + Environment.CurrentDirectory + ".");
                                        continue_executing = false;
                                    }
                                }
                                else continue_executing = false;
                            }
                            else if (!File.Exists("ckey.bin") && File.Exists(Properties.Settings.Default.CommonKeyPath))
                            {
                                try { File.Copy(Properties.Settings.Default.CommonKeyPath, "ckey.bin"); }
                                catch
                                {
                                    MessageBox.Show("Unable to copy saved common key.  Ensure you have access to read and write to the " +
                                        "directory that you specified.");
                                    WriteDebugLog("Couldn't copy saved common key to " + Environment.CurrentDirectory + ".");
                                    continue_executing = false;
                                }
                            }
                            if (continue_executing)
                            {
                                try
                                {
                                    File.Copy(workingdir + "\\CDecrypt.exe", "CDecrypt.exe");
                                    //File.Copy(workingdir + "\\MSVCR120.dll", "MSVCR120.dll");
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Unable to copy files.  Ensure you have access to read and write to the " +
                                    "directory that you specified and that CDecrypt.exe is in the root " +
                                    "directory of the directory that this program is stored in.\n\n" + ex.ToString());
                                    WriteDebugLog("Unable to copy CDecrypt.exe to " + Environment.CurrentDirectory + ".");
                                }

                                if (File.Exists("CDecrypt.exe"))
                                {
                                    Process cdecrypt = new Process();
                                    cdecrypt.StartInfo.FileName = "CDecrypt.exe";
                                    cdecrypt.StartInfo.Arguments = args;
                                    cdecrypt.StartInfo.RedirectStandardOutput = true;
                                    cdecrypt.StartInfo.UseShellExecute = false;
                                    cdecrypt.StartInfo.CreateNoWindow = true;
                                    try
                                    {
                                        EnableUI(false);
                                        NUSTabs.SelectedIndex = 0;
                                        string tmp = GUSearchBox.Text;
                                        ForceRefresh(GUSearchBox, "Decrypting...");
                                        WriteDebugLog("Starting decryption.");
                                        cdecrypt.Start();
                                        string output = "";
                                        while (!cdecrypt.StandardOutput.EndOfStream)
                                        {
                                            output += cdecrypt.StandardOutput.ReadLine() + '\n';
                                        }
                                        cdecrypt.WaitForExit();
                                        cdecrypt.Dispose();
                                        GUSearchBox.Text = tmp;
                                        if (output.Contains("00000000"))
                                        {
                                            //File.Delete("*.app");
                                            //File.Delete("*.h3");
                                            //File.Delete("*.cert");
                                            //File.Delete("*.tik");
                                            //File.Delete("*.tmd");
                                            MessageBox.Show("Title successfully decrypted.");
                                            WriteDebugLog("Decryption successful.");
                                        }
                                        else if (output == "")
                                        {
                                            MessageBox.Show("Error detected: MSVRC120.dll not found.  Please ensure you don't mess " +
                                                "with files as they are being used next time!");
                                            WriteDebugLog("Error detected: MSVRC120.dll not found.");
                                        }
                                        else
                                        {
                                            MessageBox.Show("There was an unknown problem while decrypting.");
                                            WriteDebugLog("Error unknown.  Decryption was unsuccessful.");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("There was a problem starting CDecrypt.  Ensure you have permission to execute " +
                                            "files in the directory that you specified.\n\n" + ex.ToString());
                                        WriteDebugLog("Error attempting to start " + Environment.CurrentDirectory + "\\CDecrypt.exe.");
                                    }
                                }
                            }
                            try
                            {
                                if (!ckey_exists)
                                    DeleteFile("ckey.bin");
                                else WriteDebugLog("Common key was already part of " + Environment.CurrentDirectory + ".  Skipping removal.");
                                DeleteFile("CDecrypt.exe");
                            }
                            catch { }
                        }
                        else
                        {
                            MessageBox.Show("Unable to find \"title.tik\".  Did you download a full title?  Ensure you have this " +
                                        "file in the directory that you specified.");
                            WriteDebugLog("Couldn't find " + Environment.CurrentDirectory + "\\title.tik.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to find \"title.tmd\".  Ensure this is truly a WiiU title.");
                        WriteDebugLog("Couldn't find " + Environment.CurrentDirectory + "\\title.tmd.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to set working directory.  Ensure you have access to read and write to the " +
                        "directory that you specified.\n\n" + ex.ToString());
                    WriteDebugLog("Couldn't set working directory to target directory.\n" + ex.ToString());
                }
            }
            Environment.CurrentDirectory = workingdir;
            EnableUI(true);
        }

        private void OptionsButton_Click(object sender, EventArgs e)
        {
            Point screenPoint = OptionsButton.PointToScreen(new Point(OptionsButton.Left, OptionsButton.Bottom));

            if (screenPoint.Y + MenuContext.Size.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {

                MenuContext.Show(OptionsButton, new Point(0, -MenuContext.Size.Height));
            }
            else
            {
                MenuContext.Show(OptionsButton, new Point(0, OptionsButton.Height));
            }
        }

        private void MenuSettings_Click(object sender, EventArgs e)
        {
            string Region = Properties.Settings.Default.Region;
            bool ArchivedDatabase = Properties.Settings.Default.ArchivedDatabase;
            bool UseOrigNUS = Properties.Settings.Default.UseOrigNUS;
            SettingsForm settings = new SettingsForm(debug);
            settings.ShowDialog();
            if (ArchivedDatabase != Properties.Settings.Default.ArchivedDatabase || Region != Properties.Settings.Default.Region)
                GetTitleInfo();
            if (UseOrigNUS != Properties.Settings.Default.UseOrigNUS)
            {
                DeleteFile("NUSgrabber.exe");
                WriteDebugLog("Replacing NUSgrabber.exe with " + (Properties.Settings.Default.UseOrigNUS ? "original" : "hacked") + " version.");
                File.WriteAllBytes("NUSgrabber.exe", Properties.Settings.Default.UseOrigNUS ? Properties.Resources.NUSgrabberORIG : Properties.Resources.NUSgrabber);
            }
        }

        private void MenuCreateCommonKey_Click(object sender, EventArgs e)
        {
            try
            {
                string Key = string.Empty;

                if (InputBox("Common Key", "Please enter your common key to be converted to binary:", ref Key) == DialogResult.OK)
                {
                    Key = Key.Trim();

                    if (string.IsNullOrWhiteSpace(Key))
                    {
                        return;
                    }
                    else if (Key.Length != 32)
                    {
                        MessageBox.Show("Your common key length does not look correct.", 
                            "Write Binary Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if (!keyHashes.Contains(Key.GetHashCode()))
                    {
                        if (MessageBox.Show("Your common key hash doesn't seem to match a valid one, do you still want to continue?", 
                            "Write Binary Key", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            return;
                        }
                    }

                    using (SaveFileDialog Diag = new SaveFileDialog())
                    {
                        Diag.Title = "Choose where you would like to save the binary key.";
                        Diag.Filter = "Binary Files|*.bin";
                        Diag.FileName = "ckey.bin";

                        if (Diag.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(Diag.FileName, StringToByteArray(Key));
                            MessageBox.Show("The binary key file was successfully written to \"" + Diag.FileName + "\"", "Write Binary Key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }//using
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error ocurred writing your binary file: " + ex.Message, "Write Binary Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuShowDownloadLog_Click(object sender, EventArgs e)
        {
            DownloadLog.Show();
        }

        private void GUExportButton_Click(object sender, EventArgs e)
        {
            ListItem title = GUTitleList.SelectedItem as ListItem;
            WriteDebugLog("Title Name: " + title.Desc.ToString());
            WriteDebugLog("Title Region: " + title.Region.ToString());
            WriteDebugLog("Title ID: " + title.Title_ID.ToString());
            WriteDebugLog("Title Versions: " + title.Versions.ToString());
        }

        private void STExportButton_Click(object sender, EventArgs e)
        {
            ListItem title = STTitleList.SelectedItem as ListItem;
            WriteDebugLog("Title Name: " + title.Desc.ToString());
            WriteDebugLog("Title Region: " + title.Region.ToString());
            WriteDebugLog("Title ID: " + title.Title_ID.ToString());
            WriteDebugLog("Title Versions: " + title.Versions.ToString());
        }

        private void FTExportButton_Click(object sender, EventArgs e)
        {
            ListItem title = FTTitleList.SelectedItem as ListItem;
            WriteDebugLog("Title Name: " + title.Desc.ToString());
            WriteDebugLog("Title Region: " + title.Region.ToString());
            WriteDebugLog("Title ID: " + title.Title_ID.ToString());
            WriteDebugLog("Title Versions: " + title.Versions.ToString());
        }

        #endregion

        #region Extra Functions
        //This code was taken from this site: "http://www.csharp-examples.net/inputbox/"
        //I claim no credit, except where modifications are made.
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        //This function was taken from this site: "https://stackoverflow.com/questions/6397235/write-bytes-to-file"
        //I claim no credit, except where modifications are made.
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        //This class was taken from this site: "http://stackoverflow.com/questions/9988937/sort-string-numbers"
        //I claim no credit, except where modifications are made.

        public class SortByNumber : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var regex = new System.Text.RegularExpressions.Regex("^(\\d+)");

                // run the regex on both strings
                var xRegexResult = regex.Match(x);
                var yRegexResult = regex.Match(y);

                // check if they are both numbers
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
                }

                // otherwise return as string comparison
                return x.CompareTo(y);
            }
        }

        //The original code was taken from the old GUI by Arndroid94 on GitHub, and I am
        //not responsible for creating any of the logic behind it, except for where
        //modifications are made.

        private void GetTitleInfo()
        {
            NUSTabs.SelectedIndex = 0;
            ReloadButton.Visible = false;
            EnableUI(false);
            GUSearchBox.Text = "Loading Versionlists...";
            ForceRefresh(GUSearchBox, "Started load of versionlists.");
            bool usingversionlist = true;
            if (titlelist.Count == 0)
            {
                try
                {
                    using (FileStream versionlist_file = File.Open("wiiu_versionlist.txt", FileMode.Open, FileAccess.Read))
                    {
                        titlelist.Clear();
                        StreamReader versionlist_read = new StreamReader(versionlist_file);
                        while (!versionlist_read.EndOfStream)
                        {
                            string id = versionlist_read.ReadLine();
                            List<string> versions = new List<string>();
                            while (versionlist_read.Peek() == '\t')
                            {
                                versionlist_read.Read();
                                versions.Add(versionlist_read.ReadLine());
                            }
                            titlelist.Add(id, versions);
                        }
                    }
                    WriteDebugLog("Completed load successfully.");
                }
                catch (FileNotFoundException)
                {
                    usingversionlist = false;
                    WriteDebugLog("wiiu_versionlist.txt not found.  Skipping load.");
                }
                catch (IOException)
                {
                    WriteDebugLog("wiiu_versionlist.txt was modified externally.  Skipping load.");
                    MessageBox.Show("The versionlist file was modified externally.  Either fix it or redownload it.");
                    usingversionlist = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unknown error while loading the version list!/n/n" + ex.ToString());
                    usingversionlist = false;
                    WriteDebugLog("Unknown error ocurred while trying to load wiiu_versionlist.txt.  Skipping load.");
                }
            }
            else WriteDebugLog("Titlelist already in memory.  Skipping load.");
            GameUpdates.Clear();
            SystemTitles.Clear();
            ForceRefresh(GUSearchBox, "Connecting to the Internet...");
            string request = "http://wiiubrew.org/wiki/Title_database";
            if (Properties.Settings.Default.ArchivedDatabase) request = "http://wiiubrew.org/w/index.php?title=Title_database&oldid=1575";
            WriteDebugLog("Connecting to " + request + ".");
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument hdoc = new HtmlAgilityPack.HtmlDocument();
            try { hdoc = web.Load(request); }
            catch
            {
                MessageBox.Show("Could not connect to the internet.  Make sure you have internet access before loading titles!");
                WriteDebugLog("Couldn't connect to the internet.  Not loading titles.");
            }
            if (hdoc.DocumentNode.HasChildNodes)
            {
                ForceRefresh(GUSearchBox, "Parsing Data...");
                WriteDebugLog("Parsing data gathered from " + request + ".");
                int tablecount = 1;
                foreach (HtmlAgilityPack.HtmlNode table in hdoc.GetElementbyId("mw-content-text").Elements("table"))
                {
                    switch (tablecount)
                    {
                        case 7:
                            {
                                foreach (HtmlAgilityPack.HtmlNode tr in table.Elements("tr"))
                                {
                                    int columncount = 1;
                                    ListItem cbi = new ListItem();
                                    foreach (HtmlAgilityPack.HtmlNode td in tr.Elements("td"))
                                    {
                                        //TITLE
                                        if (columncount == 1)
                                        {
                                            string title = null;
                                            try { title = td.InnerText.Trim().Substring(9); } catch (Exception) { cbi.Title_ID = title; cbi.Desc = title; cbi.Versions = title; }
                                            cbi.Title_ID = title;
                                        }
                                        //DESC
                                        if (columncount == 2)
                                        {
                                            string desc = null;
                                            try { desc = td.InnerText.Trim().Replace("amp;", "").Replace("&#160;", ""); } catch (Exception) { cbi.Title_ID = desc; cbi.Desc = desc; cbi.Versions = desc; }
                                            cbi.Desc = desc;
                                        }
                                        //VER
                                        if (columncount == 6)
                                        {
                                            try
                                            {
                                                string id = cbi.Title_ID.ToString();
                                                if (usingversionlist && titlelist.ContainsKey(id))
                                                {
                                                    string versions = "";
                                                    foreach (string v in titlelist[id])
                                                    {
                                                        versions += v + ", ";
                                                    }
                                                    cbi.Versions = versions;
                                                }
                                                else
                                                {
                                                    string ver = null;
                                                    try { ver = td.InnerText.Trim(); } catch (Exception) { cbi.Title_ID = ver; cbi.Desc = ver; cbi.Versions = ver; }
                                                    cbi.Versions = ver;
                                                }
                                            }
                                            catch
                                            {
                                                //TODO: Handle
                                            }
                                        }
                                        //REG
                                        if (columncount == 7)
                                        {
                                            string reg = null;
                                            try { reg = td.InnerText.Trim(); } catch (Exception) { cbi.Title_ID = reg; cbi.Desc = reg; cbi.Versions = reg; }
                                            if (cbi.Desc != null)
                                            {
                                                if (reg == "JAP") reg = "JPN";  //Quick-fix for japanese titles
                                                cbi.Desc = reg + " - " + cbi.Desc;
                                            }
                                            cbi.Region = reg;
                                        }
                                        columncount++;
                                    }
                                    if (cbi.Desc != null && !cbi.Versions.ToString().Contains("v0"))
                                        GameUpdates.Add(cbi);
                                }
                                tablecount++;
                                break;
                            }
                        case 3:
                            {
                                foreach (HtmlAgilityPack.HtmlNode tr in table.Elements("tr"))
                                {
                                    int columncount = 1;
                                    ListItem cbisys = new ListItem();
                                    foreach (HtmlAgilityPack.HtmlNode td in tr.Elements("td"))
                                    {
                                        //TITLE
                                        if (columncount == 1)
                                        {
                                            string title = null;
                                            try { title = td.InnerText.Trim().Substring(9); } catch (Exception) { cbisys.Title_ID = title; cbisys.Desc = title; cbisys.Versions = title; }
                                            cbisys.Title_ID = title;
                                        }
                                        //DESC
                                        if (columncount == 2)
                                        {
                                            string desc = null;
                                            try { desc = td.InnerText.Trim().Replace("amp;", ""); } catch (Exception) { cbisys.Title_ID = desc; cbisys.Desc = desc; cbisys.Versions = desc; }
                                            cbisys.Desc = desc;
                                        }
                                        //VER
                                        if (columncount == 4)
                                        {
                                            try
                                            {
                                                string id = cbisys.Title_ID.ToString();
                                                if (usingversionlist && titlelist.ContainsKey(id))
                                                {
                                                    string versions = "";
                                                    foreach (string v in titlelist[id])
                                                    {
                                                        versions += v + ", ";
                                                    }
                                                    cbisys.Versions = versions;
                                                }
                                                else
                                                {
                                                    string ver = null;
                                                    try { ver = td.InnerText.Trim(); } catch (Exception) { cbisys.Title_ID = ver; cbisys.Desc = ver; cbisys.Versions = ver; }
                                                    cbisys.Versions = ver;
                                                }
                                            }
                                            catch
                                            {
                                                //TODO: Handle
                                            }
                                        }
                                        //REG
                                        if (columncount == 5)
                                        {
                                            string reg = null;
                                            try { reg = td.InnerText.Trim(); } catch (Exception) { cbisys.Title_ID = reg; cbisys.Desc = reg; cbisys.Versions = reg; }
                                            if (cbisys.Desc != null)
                                            {
                                                cbisys.Desc = reg + " - " + cbisys.Desc;
                                            }
                                            cbisys.Region = reg;
                                        }
                                        columncount++;
                                    }
                                    if (cbisys.Desc != null)
                                    {
                                        SystemTitles.Add(cbisys);
                                    }
                                }
                                tablecount++;
                                break;
                            }
                        default:
                            {
                                tablecount++;
                                break;
                            }
                    }
                }
                ForceRefresh(GUSearchBox, "Sorting Titles...");
                WriteDebugLog("Parse complete.  Sorting internal titlelist.");
                GameUpdates.Sort(delegate (ListItem c1, ListItem c2) { return c1.Desc.CompareTo(c2.Desc); });
                foreach (ListItem t in GameUpdates)
                    if (t.Desc != null)
                    {
                        GUTitleList.Items.Add(t);
                        FTTitleList.Items.Add(t);
                    }
                try
                {
                    GUTitleList.SelectedIndex = 0;
                    FTTitleList.SelectedIndex = 0;
                }
                catch { }

                SystemTitles.Sort(delegate (ListItem cs1, ListItem cs2) { return cs1.Desc.CompareTo(cs2.Desc); });
                foreach (ListItem s in SystemTitles)
                {
                    if (s.Desc != null)
                    {
                        STTitleList.Items.Add(s);
                    }
                }

                try { STTitleList.SelectedIndex = 0; }
                catch { }
                GUSearchBox.Text = "";
                WriteDebugLog("Sort complete.");
                if (GameUpdates.Count != 0)
                {
                    ReloadButton.Visible = false;
                }
                else
                {
                    WriteDebugLog("No titles in titlelist.  Disabling UI and showing reload button.");
                    EnableUI(false);
                    ReloadButton.Visible = true;
                }
            }
            else ReloadButton.Visible = true;
        }

        private void EnableUI(bool enable, bool first)
        {
            Cursor = enable ? Cursors.Default : Cursors.WaitCursor;
            NUSTabs.Enabled = enable;
            if ((File.Exists("NUSgrabber.exe") || debug || !enable) && !first)
                DownloadButton.Enabled = enable;
            OptionsButton.Enabled = enable;
            UpdateButton.Enabled = enable;
            if (File.Exists("CDecrypt.exe") || debug || !enable)
                DecryptButton.Enabled = enable;
            GUExportButton.Enabled = enable;
        }

        private void EnableUI(bool enable)
        {
            EnableUI(enable, false);
        }

        //Includes a little hack I learned to force the application to refresh on the spot.
        private void ForceRefresh(TextBox txtbox, string text)
        {
            if (InvokeReq(txtbox, () => ForceRefresh(txtbox, text))) return;
            txtbox.Text = text;
            txtbox.Invalidate();
            txtbox.Update();
            txtbox.Refresh();
            Application.DoEvents();
        }

        public void CheckForUpdates(bool silent)
        {
            if (!Properties.Settings.Default.Debug)
            {
                const string request = "https://dl.dropboxusercontent.com/u/41125193/NUSGrabber/latestversion.xml";
                try
                {
                    WebRequest req = WebRequest.Create(request);
                    req.Timeout = 3000;
                    WebResponse response = req.GetResponse();
                    XDocument xdoc = XDocument.Load(response.GetResponseStream());
                    int newversion;
                    if (int.TryParse(xdoc.Element("version").Value, out newversion))
                    {
                        if (newversion > Properties.Settings.Default.Version || !silent)
                        {
                            float newversion_float = (float)newversion / 100;
                            float oldversion_float = (float)Properties.Settings.Default.Version / 100;
                            if (MessageBox.Show("Your current version is " + oldversion_float + " and the newest version is " +
                                newversion_float + ".  " + (newversion <= Properties.Settings.Default.Version ? "You are already up to date.  " :
                                "") + "Would you like to update" + (newversion <= Properties.Settings.Default.Version ? " anyways" :
                                "") + "?", "Update Program", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                try
                                {
                                    int times = 10;
                                    while (File.Exists("NEW-NUSGrabberGUI.exe") && times != 0)
                                    {
                                        WriteDebugLog("Attempting to rename original file - try #" + times + ".");
                                        File.Move("NEW-NUSGrabberGUI.exe", "NEW-NUSGrabberGUIbackup.exe");
                                        times--;
                                    }
                                    if (times == 0)
                                    {
                                        MessageBox.Show("Failed to update the exe.  You can still manually update using this link:\nhttps://dl.dropboxusercontent.com/u/41125193/NUSGrabber/NEW-NUSGrabberGUI.exe");
                                        WriteDebugLog("Failed to rename self.");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            WebClient webClient = new WebClient();
                                            webClient.DownloadFile("https://dl.dropboxusercontent.com/u/41125193/NUSGrabber/NEW-NUSGrabberGUI.exe", "NEW-NUSGrabberGUI.exe");
                                            MessageBox.Show("Successfully obtained update.  Application will now restart.");
                                            WriteDebugLog("Update successful.");
                                        }
                                        catch
                                        {
                                            MessageBox.Show("Failed to download the update.  You can still manually update from here:\nhttps://dl.dropboxusercontent.com/u/41125193/NUSGrabber/NEW-NUSGrabberGUI.exe\nThe program will now exit.");
                                            WriteDebugLog("Failed to download.  Exiting.");
                                            times = 10;
                                            while (File.Exists("NEW-NUSGrabberGUIbackup.exe") && times != 0)
                                            {
                                                File.Move("NEW-NUSGrabberGUIbackup.exe", "NEW-NUSGrabberGUI.exe");
                                                times--;
                                            }
                                            if (times == 0)
                                            {
                                                MessageBox.Show("Failed to rename the exe back.  Program will now exit, and you'll need to rename it yourself.");
                                                WriteDebugLog("Failed to rename self back to normal.");
                                                Process.GetCurrentProcess().Kill();
                                            }
                                        }
                                        finally
                                        {
                                            try
                                            {
                                                string batchCommands = "";  //The self deleting batch file idea was taken from this site: http://stackoverflow.com/questions/19689054/is-it-possible-for-a-c-sharp-built-exe-to-self-delete
                                                batchCommands += "@ECHO OFF\n";                         // Do not show any output
                                                batchCommands += "ping 127.0.0.1 -n 2> nul\n";              // Wait approximately 4 seconds (so that the process is terminated by the time this executes)
                                                batchCommands += "echo j | del /F ";                    // Delete the executeable
                                                batchCommands += Environment.CurrentDirectory + "\\NEW-NUSGrabberGUIbackup.exe\n";
                                                batchCommands += "echo j | del selfdelete.vbs\n";    // Delete the windows script file that hides the execution of this bat file
                                                batchCommands += "echo j | del selfdelete.bat\n";    // Delete this bat file

                                                string vbsCommands = "";
                                                vbsCommands += "Set WshShell = CreateObject(\"WScript.Shell\") \n";
                                                vbsCommands += "WshShell.Run chr(34) & \"" + Environment.CurrentDirectory + "\\selfdelete.bat\" & Chr(34), 0, false\n";
                                                vbsCommands += "Set WshShell = Nothing\n";

                                                WriteDebugLog("Creating self-deletion scripts.");
                                                File.WriteAllText("selfdelete.bat", batchCommands);
                                                File.WriteAllText("selfdelete.vbs", vbsCommands);
                                                WriteDebugLog("Successfully created self-deletion scripts.");

                                                Process selfdel = new Process();
                                                selfdel.StartInfo.FileName = "selfdelete.vbs";
                                                selfdel.StartInfo.CreateNoWindow = true;
                                                WriteDebugLog("Running self-delete script.");
                                                selfdel.Start();

                                                WriteDebugLog("Starting new version and exiting.");
                                                Process.Start("NEW-NUSGrabberGUI.exe");
                                                Process.GetCurrentProcess().Kill();
                                            }
                                            catch
                                            {
                                                MessageBox.Show("Failed to restart program. You'll need to exit and delete the old version, then load the program again.");
                                                WriteDebugLog("Failed to restart.");
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show("Failed to rename old exe.  You'll need to manually update from here:\nhttps://dl.dropboxusercontent.com/u/41125193/NUSGrabber/NEW-NUSGrabberGUI.exe");
                                    WriteDebugLog("Failed to rename self.");
                                }
                            }
                        }
                    }
                    else if (!silent)
                    {
                        MessageBox.Show("There was an error checking for the newest version.");
                        WriteDebugLog("Couldn't determine the newest version.");
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to connect to the internet to check for updates.");
                    WriteDebugLog("Couldn't connect to the internet to check for updates.");
                }
            }
        }

        public void ExtractResources()
        {
            try
            {
                if (!File.Exists("HtmlAgilityPack.dll"))
                {
                    WriteDebugLog("Couldn't find HtmlAgilityPack.dll.  Extracting resource.");
                    File.WriteAllBytes("HtmlAgilityPack.dll", Properties.Resources.HtmlAgilityPack);
                }
                if (!File.Exists("CDecrypt.exe"))
                {
                    WriteDebugLog("Couldn't find CDecrypt.exe.  Extracting resource.");
                    File.WriteAllBytes("CDecrypt.exe", Properties.Resources.CDecrypt);
                }
                if (!File.Exists("libeay32.dll"))
                {
                    WriteDebugLog("Couldn't find libeay32.dll.  Extracting resource.");
                    File.WriteAllBytes("libeay32.dll", Properties.Resources.libeay32);
                }
                if (!File.Exists("NUSgrabber.exe"))
                {
                    WriteDebugLog("Couldn't find NUSgrabber.exe.  Extracting resource.");
                    File.WriteAllBytes("NUSgrabber.exe", Properties.Settings.Default.UseOrigNUS ? Properties.Resources.NUSgrabberORIG : Properties.Resources.NUSgrabber);
                }
                if (!File.Exists("vcruntime140.dll"))
                {
                    WriteDebugLog("Couldn't find vcruntime140.dll.  Extracting resource.");
                    File.WriteAllBytes("vcruntime140.dll", Properties.Resources.vcruntime140);
                }
                if (!File.Exists("wget.exe"))
                {
                    WriteDebugLog("Couldn't find wget.exe.  Extracting resource.");
                    File.WriteAllBytes("wget.exe", Properties.Resources.wget);
                }
                if (!File.Exists("wiiu_versionlist.txt"))
                {
                    WriteDebugLog("Couldn't find wiiu_versionlist.txt.  Extracting default versionlist.");
                    File.WriteAllText("wiiu_versionlist.txt", Properties.Resources.wiiu_versionlist);
                }
                if (!File.Exists("lang/en.resx"))
                {
                    WriteDebugLog("Couldn't find lang/en.resx.  Extracting english language set.");
                    Directory.CreateDirectory("lang");
                    File.WriteAllText("lang/en.resx", Properties.Resources.en);
                }
            }
            catch
            {
                MessageBox.Show("Couldn't extract/read base files.  Make sure this program has read/write access to the directory it's stored in.");
                WriteDebugLog("Error extracting resources.");
            }
        }

        public void WriteDebugLog(string text, bool showdate)
        {
            if (canwritedebug && Properties.Settings.Default.Debug)
            {
                try
                {
                    StreamWriter debug_write = File.AppendText("debug.log");
                    string[] text_split = text.Split('\n');
                    foreach (string split in text_split)
                    {
                        if (split != "")
                        {
                            debug_write.WriteLine((showdate ? DateTime.Now.ToString() + " - " : "") + split);
                        }
                        else debug_write.WriteLine();
                    }
                    debug_write.Flush();
                    debug_write.Close();
                }
                catch
                {
                    MessageBox.Show("Unable to write debug log.  Make sure you don't have it open somewhere and that you have " +
                        "write access to the root directory that this program is stored in.");
                    canwritedebug = false;
                }
            }
        }

        public void WriteDebugLog(string text)
        {
            WriteDebugLog(text, true);
        }

        public void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                WriteDebugLog("Cleaning up unneeded resource: " + Environment.CurrentDirectory + '\\' + file + ".");
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch
                {
                    WriteDebugLog("Couldn't delete resource: " + Environment.CurrentDirectory + '\\' + file + ".");
                }
            }
        }

        private void CleanupOnExit(object sender, EventArgs e)
        {
            //Dispose            
            DownloadLog.Dispose();

            if (!Properties.Settings.Default.Debug)
                DeleteFile("debug.log");
            else WriteDebugLog("Preforming shut down process.\n");
            if (Properties.Settings.Default.Cleanup == true && debug == false)
            {
                DeleteFile("CDecrypt.exe");
                DeleteFile("msvcr120.dll");
                DeleteFile("msvcp140.dll");
                DeleteFile("libeay32.dll");
                DeleteFile("NUSgrabber.exe");
                DeleteFile("vcruntime140.dll");
                DeleteFile("wget.exe");
            }
            DeleteFile("selfdelete.bat");
            DeleteFile("selfdelete.vbs");
            Properties.Settings.Default.UseEmbedNUS = false;
            Properties.Settings.Default.Save();
        }

        public List<FileInfo> GetFiles(string path, params string[] extensions)
        {
            List<FileInfo> list = new List<FileInfo>();
            foreach (string ext in extensions)
                list.AddRange(new DirectoryInfo(path).GetFiles("*" + ext).Where(p =>
                      p.Extension.Equals(ext, StringComparison.CurrentCultureIgnoreCase))
                      .ToArray());
            return list;
        }

        public bool InvokeReq(Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate
            {
                a();
            }));
            else return false;

            return true;
        }

        public string GetLanguageString(string name, bool addwhitespace)
        {
            string value = XDocument.Parse(language).Descendants().FirstOrDefault(_ => _.Attributes().Any(a => a.Value == name))?.Value;
            value.Trim(' ', '\n', '\r'); //TODO: Fix
            if (addwhitespace)
                value = value.Insert(0, " ").Insert(value.Length - 1, " ");
            GUSearchBox.Text = value;
            return value;

        }

        #endregion

        #region Background Workers
        #pragma warning disable CS0162 // Unreachable code detected

        private void EmbedNUSGrabber_Work(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string[] args = (string[])e.Argument;
            NusClient nusClient = new NusClient();
            nusClient.UseLocalFiles = true;
            nusClient.ContinueWithoutTicket = true;
            nusClient.SetToWiiServer();
            nusClient.Progress += nusClient_Progress;
            nusClient.Debug += nusClient_Debug;
            NUSGrabberProgress.Visible = true;
            StoreType[] storeTypes = new StoreType[3];
            if (false) storeTypes[0] = StoreType.WAD; else storeTypes[0] = StoreType.Empty;
            if (false) storeTypes[1] = StoreType.DecryptedContent; else storeTypes[1] = StoreType.Empty;
            if (true) storeTypes[2] = StoreType.EncryptedContent; else storeTypes[2] = StoreType.Empty;
            WriteDebugLog("Starting background download using libWiiSharp with \"" + args[0] + "\" and \"" + args[1] + "\" as arguments.");
            try { nusClient.DownloadTitle(args[0], args[1], Environment.CurrentDirectory, args[0], storeTypes); }
            catch (Exception ex) { WriteDebugLog(ex.ToString()); }
        }

        private void nusClient_Debug(object sender, MessageEventArgs e)
        {
            WriteDebugLog(e.Message);
        }

        private void EmbedNUSGrabber_Done(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("An error has occured while trying to run the embeded NUSGrabber.\n\n" + e.Error.ToString());
                WriteDebugLog("Error while running libWiiSharp:\n" + e.Error.ToString());
            }
            EnableUI(true);
            filepath = "";
            GUSearchBox.Text = "";
            NUSGrabberProgress.Visible = false;
        }

        private void nusClient_Progress(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            NUSGrabberProgress.Value = e.ProgressPercentage;
        }

        #endregion
    }

    #region ListItem

    class ListItem
    {
        public string Desc { get; set; }
        public object Title_ID { get; set; }
        public object Versions { get; set; }
        public object Region { get; set; }
        public override string ToString()
        {
            Desc = !string.IsNullOrEmpty(Desc) ? Desc : "null";
            return Desc;
        }
    }

    #endregion
}
