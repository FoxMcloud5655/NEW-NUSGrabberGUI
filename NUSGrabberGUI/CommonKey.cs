namespace libWiiSharp
{
    public class CommonKey
    {
        private static byte[] wiiuKey = new byte[16];
        public static byte[] GetStandardKey()
        {
            if (!System.IO.File.Exists(NUSGrabberGUI.Properties.Settings.Default.CommonKeyPath))
            {
                System.Windows.Forms.OpenFileDialog FileDialog = new System.Windows.Forms.OpenFileDialog();
                FileDialog.Filter = "WiiU Common Key|*.bin";
                FileDialog.FileName = "ckey.bin";
                if (FileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    NUSGrabberGUI.Properties.Settings.Default.CommonKeyPath = FileDialog.FileName;
                    NUSGrabberGUI.Properties.Settings.Default.Save();
                }
                else return null;
            }
            try
            {
                System.IO.FileStream ckey = System.IO.File.OpenRead(NUSGrabberGUI.Properties.Settings.Default.CommonKeyPath);
                if (ckey.Length == 16)
                    try { ckey.Read(wiiuKey, 0, 16); }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Unable to parse saved common key.  Make sure it is valid!");
                        return null;
                    }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Your common key is not valid!  Please make sure it truly is the common key.");
                    return null;
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Unable to open saved common key.  Make sure it is readable and still there!");
                return null;
            }
            return wiiuKey;
        }
    }
}
