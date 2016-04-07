/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * 
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public enum StoreType
    {
        EncryptedContent = 0,
        DecryptedContent = 1,
        WAD = 2,
        All = 3,
        Empty = 4
    }

    public class NusClient : IDisposable
    {
        private const string WII_NUS_URL = "http://nus.cdn.shop.wii.com/ccs/download/";
        private const string DSI_NUS_URL = "http://nus.cdn.t.shop.nintendowifi.net/ccs/download/";

        private const string WII_USER_AGENT = "wii libnup/1.0";
        private const string DSI_USER_AGENT = "Opera/9.50 (Nintendo; Opera/154; U; Nintendo DS; en)";

        private string nusUrl = WII_NUS_URL;
        private WebClient wcNus = new WebClient();
        private bool useLocalFiles = false;
        private bool continueWithoutTicket = false;

        private int titleversion;

        public int TitleVersion { get { return titleversion; } }

        /// <summary>
        /// If true, existing local files will be used.
        /// </summary>
        public bool UseLocalFiles { get { return useLocalFiles; } set { useLocalFiles = value; } }
        /// <summary>
        /// If true, the download will be continued even if no ticket for the title is avaiable (WAD packaging and decryption are disabled).
        /// </summary>
        public bool ContinueWithoutTicket { get { return continueWithoutTicket; } set { continueWithoutTicket = value; } }

		#region IDisposable Members
        private bool isDisposed = false;

        ~NusClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !isDisposed)
            {
                wcNus.Dispose();
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions

        public void ConfigureNusClient(WebClient wcReady)
        {
            wcNus = wcReady;
        }

        public void SetToWiiServer()
        {
            nusUrl = WII_NUS_URL;
            wcNus.Headers.Add("User-Agent", WII_USER_AGENT);
        }

        public void SetToDSiServer()
        {
            nusUrl = DSI_NUS_URL;
            wcNus.Headers.Add("User-Agent", DSI_USER_AGENT);
        }

        /// <summary>
        /// Grabs a title from NUS, you can define several store types.
        /// Leave the title version empty for the latest.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="titleVersion"></param>
        /// <param name="outputDir"></param>
        /// <param name="storeTypes"></param>
        public void DownloadTitle(string titleId, string titleVersion, string outputDir, string wadName, params StoreType[] storeTypes)
        {
            if (titleId.Length != 16) throw new Exception("Title ID must be 16 characters long!");
            downloadTitle(titleId, titleVersion, outputDir, wadName, storeTypes);
        }

        /// <summary>
        /// Grabs a TMD from NUS.
        /// Leave the title version empty for the latest.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="titleVersion"></param>
        /// <returns></returns>
        public TMD DownloadTMD(string titleId, string titleVersion)
        {
            if (titleId.Length != 16) throw new Exception("Title ID must be 16 characters long!");
            return downloadTmd(titleId, titleVersion);
        }

        /// <summary>
        /// Grabs a Ticket from NUS.
        /// </summary>
        /// <param name="titleId"></param>
        /// <returns></returns>
        public Ticket DownloadTicket(string titleId)
        {
            if (titleId.Length != 16) throw new Exception("Title ID must be 16 characters long!");
            return downloadTicket(titleId);
        }

        /// <summary>
        /// Grabs a single content file and decrypts it.        
        /// Leave the title version empty for the latest. 
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="titleVersion"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public byte[] DownloadSingleContent(string titleId, string titleVersion, string contentId)
        {
            if (titleId.Length != 16) throw new Exception("Title ID must be 16 characters long!");
            return downloadSingleContent(titleId, titleVersion, contentId);
        }

        /// <summary>
        /// Grabs a single content file and decrypts it.        
        /// Leave the title version empty for the latest. 
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="titleVersion"></param>
        /// <param name="contentId"></param>
        /// <param name="savePath"></param>
        public void DownloadSingleContent(string titleId, string titleVersion, string contentId, string savePath)
        {
            if (titleId.Length != 16) throw new Exception("Title ID must be 16 characters long!");
            if (!Directory.Exists(Path.GetDirectoryName(savePath))) Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            if (File.Exists(savePath)) File.Delete(savePath);

            byte[] content = downloadSingleContent(titleId, titleVersion, contentId);
            File.WriteAllBytes(savePath, content);
        }
        #endregion

        #region Private Functions
        private byte[] downloadSingleContent(string titleId, string titleVersion, string contentId)
        {
            uint cId = uint.Parse(contentId, System.Globalization.NumberStyles.HexNumber);
            contentId = cId.ToString("x8");

            fireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2}...", contentId, titleId, (string.IsNullOrEmpty(titleVersion)) ? "[Latest]" : titleVersion);

            fireDebug("   Checking for Internet connection...");
            if (!CheckInet())
            { fireDebug("   Connection not found..."); throw new Exception("You're not connected to the internet!"); }

            fireProgress(0);

            string tmdFile = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : string.Format(".{0}", titleVersion));
            string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
            string contentIdString = string.Empty;
            int cIndex = 0;

            //Download TMD
            fireDebug("   Downloading TMD...");
            byte[] tmdArray = wcNus.DownloadData(titleUrl + tmdFile);
            fireDebug("   Parsing TMD...");
            TMD tmd = TMD.Load(tmdArray);

            fireProgress(20);

            //Search for Content ID in TMD
            fireDebug("   Looking for Content ID {0} in TMD...", contentId);
            bool foundContentId = false;
            for (int i = 0; i < tmd.Contents.Length; i++)
                if (tmd.Contents[i].ContentID == cId)
                {
                    fireDebug("   Content ID {0} found in TMD...", contentId);
                    foundContentId = true;
                    contentIdString = tmd.Contents[i].ContentID.ToString("x8");
                    cIndex = i;
                    break;
                }

            if (!foundContentId)
            { fireDebug("   Content ID {0} wasn't found in TMD...", contentId); throw new Exception("Content ID wasn't found in the TMD!"); }

            //Download Ticket
            fireDebug("   Downloading Ticket...");
            byte[] tikArray = wcNus.DownloadData(titleUrl + "cetk");
            fireDebug("   Parsing Ticket...");
            Ticket tik = Ticket.Load(tikArray);

            fireProgress(40);

            //Download and Decrypt Content
            fireDebug("   Downloading Content... ({0} bytes)", tmd.Contents[cIndex].Size);
            byte[] encryptedContent = wcNus.DownloadData(titleUrl + contentIdString);

            fireProgress(80);

            fireDebug("   Decrypting Content...");
            byte[] decryptedContent = decryptContent(encryptedContent, cIndex, tik, tmd);
            Array.Resize(ref decryptedContent, (int)tmd.Contents[cIndex].Size);

            //Check SHA1
            SHA1 s = SHA1.Create();
            byte[] newSha = s.ComputeHash(decryptedContent);

            if (!Shared.CompareByteArrays(newSha, tmd.Contents[cIndex].Hash))
            { fireDebug(@"/!\ /!\ /!\ Hashes do not match /!\ /!\ /!\"); throw new Exception("Hashes do not match!"); }

            fireProgress(100);

            fireDebug("Downloading Content (Content ID: {0}) of Title {1} v{2} Finished...", contentId, titleId, (string.IsNullOrEmpty(titleVersion)) ? "[Latest]" : titleVersion);
            return decryptedContent;
        }

        private Ticket downloadTicket(string titleId)
        {
            if (!CheckInet())
                throw new Exception("You're not connected to the internet!");

            string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
            byte[] tikArray = wcNus.DownloadData(titleUrl + "cetk");

            return Ticket.Load(tikArray);
        }

        private TMD downloadTmd(string titleId, string titleVersion)
        {
            if (!CheckInet())
                throw new Exception("You're not connected to the internet!");

            string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
            string tmdFile = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : string.Format(".{0}", titleVersion));

            byte[] tmdArray = wcNus.DownloadData(titleUrl + tmdFile);

            return TMD.Load(tmdArray);
        }

        private void downloadTitle(string titleId, string titleVersion, string outputDir, string wadName, StoreType[] storeTypes)
        {
            fireDebug("Downloading Title {0} v{1}...", titleId, (string.IsNullOrEmpty(titleVersion)) ? "[Latest]" : titleVersion);

            if (storeTypes.Length < 1)
            { fireDebug("  No store types were defined..."); throw new Exception("You must at least define one store type!"); }

            string titleUrl = string.Format("{0}{1}/", nusUrl, titleId);
            bool storeEncrypted = false;
            bool storeDecrypted = false;
            bool storeWad = false;

            fireProgress(0);

            foreach (StoreType st in storeTypes)
            {
                switch (st)
                {
                    case StoreType.DecryptedContent:
                        fireDebug("    [=] Storing Decrypted Content...");
                        storeDecrypted = true;
                        break;
                    case StoreType.EncryptedContent:
                        fireDebug("    [=] Storing Encrypted Content...");
                        storeEncrypted = true;
                        break;
                    case StoreType.WAD:
                        fireDebug("    [=] Storing WAD...");
                        storeWad = true;
                        break;
                    case StoreType.All:
                        fireDebug("    [=] Storing Decrypted Content...");
                        fireDebug("    [=] Storing Encrypted Content...");
                        fireDebug("    [=] Storing WAD...");
                        storeDecrypted = true;
                        storeEncrypted = true;
                        storeWad = true;
                        break;
                    case StoreType.Empty:
                        break;
                }
            }
            /*
            fireDebug("  - Checking for Internet connection...");
            if (!CheckInet())
            { fireDebug("   + Connection not found..."); throw new Exception("You're not connected to the internet!"); }
            */
            
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
            if (!Directory.Exists(Path.Combine(outputDir, titleId))) Directory.CreateDirectory(Path.Combine(outputDir, titleId));
            outputDir = Path.Combine(outputDir, titleId);

            string tmdFile = "tmd" + (string.IsNullOrEmpty(titleVersion) ? string.Empty : string.Format(".{0}", titleVersion));

            //Download TMD
            fireDebug("  - Downloading TMD...");
            TMD tmd;
            byte[] tmdFileWithCerts;
            try
            {
                tmdFileWithCerts = wcNus.DownloadData(titleUrl + tmdFile);
                tmd = TMD.Load(tmdFileWithCerts);
            }
            catch (Exception ex) { fireDebug("   + Downloading TMD Failed..."); throw new Exception("Downloading TMD Failed:\n" + ex.Message); }

            //Parse TMD
            fireDebug("  - Parsing TMD...");

            if (string.IsNullOrEmpty(titleVersion)) { fireDebug("    + Title Version: {0}", tmd.TitleVersion); }
            fireDebug("    + {0} Contents", tmd.NumOfContents);

            if (!Directory.Exists(Path.Combine(outputDir, tmd.TitleVersion.ToString()))) Directory.CreateDirectory(Path.Combine(outputDir, tmd.TitleVersion.ToString()));
            outputDir = Path.Combine(outputDir, tmd.TitleVersion.ToString());

            this.titleversion = tmd.TitleVersion;

            File.WriteAllBytes(Path.Combine(outputDir, tmdFile), tmd.ToByteArray());

            fireProgress(5);

            //Download cetk
            fireDebug("  - Downloading Ticket...");
            try
            {
                wcNus.DownloadFile(Path.Combine(titleUrl, "cetk"), Path.Combine(outputDir, "cetk"));
            }
            catch (Exception ex)
            {
                if (!continueWithoutTicket || !storeEncrypted)
                {
                    fireDebug("   + Downloading Ticket Failed...");
                    throw new Exception("Downloading Ticket Failed:\n" + ex.Message);
                }

                if (!(File.Exists(Path.Combine(outputDir, "cetk"))))
                {
                    storeDecrypted = false;
                    storeWad = false;
                }
            }

            fireProgress(10);

            // Parse Ticket
            Ticket tik = new Ticket();

            if (File.Exists(Path.Combine(outputDir, "cetk")))
            {
                fireDebug("   + Parsing Ticket...");
                tik = Ticket.Load(Path.Combine(outputDir, "cetk"));

                // DSi ticket? Must make sure to use DSi Key :D
                if (nusUrl == DSI_NUS_URL)
                    tik.DSiTicket = true;
            }
            else
            {
                fireDebug("   + Ticket Unavailable...");
            }

            string[] encryptedContents = new string[tmd.NumOfContents];

            //Download Content
            for (int i = 0; i < tmd.NumOfContents; i++)
            {
                fireDebug("  - Downloading Content #{0} of {1}... ({2} bytes)", i + 1, tmd.NumOfContents, tmd.Contents[i].Size);
                fireProgress(((i + 1) * 60 / tmd.NumOfContents) + 10);

                if (useLocalFiles && File.Exists(Path.Combine(outputDir, tmd.Contents[i].ContentID.ToString("x8"))))
                { fireDebug("   + Using Local File, Skipping..."); continue; }

                try
                {
                    wcNus.DownloadFile(titleUrl + tmd.Contents[i].ContentID.ToString("x8"),
                        Path.Combine(outputDir, tmd.Contents[i].ContentID.ToString("x8")));

                    encryptedContents[i] = tmd.Contents[i].ContentID.ToString("x8");
                }
                catch (Exception ex) { fireDebug("  - Downloading Content #{0} of {1} failed...", i + 1, tmd.NumOfContents); throw new Exception("Downloading Content Failed:\n" + ex.Message); }
            }

            //Decrypt Content
            if (storeDecrypted || storeWad)
            {
                SHA1 s = SHA1.Create();

                for (int i = 0; i < tmd.NumOfContents; i++)
                {
                    fireDebug("  - Decrypting Content #{0} of {1}...", i + 1, tmd.NumOfContents);
                    fireProgress(((i + 1) * 20 / tmd.NumOfContents) + 75);

                    //Decrypt Content
                    byte[] decryptedContent =
                        decryptContent(File.ReadAllBytes(Path.Combine(outputDir, tmd.Contents[i].ContentID.ToString("x8"))), i, tik, tmd);
                    Array.Resize(ref decryptedContent, (int)tmd.Contents[i].Size);

                    //Check SHA1
                    byte[] newSha = s.ComputeHash(decryptedContent);
                    if (!Shared.CompareByteArrays(newSha, tmd.Contents[i].Hash))
                    { 
                        fireDebug(@"   + Hashes do not match! (Invalid Output)"); 
                        //throw new Exception(string.Format("Content #{0}: Hashes do not match!", i));
                    }

                    //Write Decrypted Content
                    File.WriteAllBytes(Path.Combine(outputDir, (tmd.Contents[i].ContentID.ToString("x8") + ".app")), decryptedContent);
                }

                s.Clear();
            }

            //Pack Wad
            if (storeWad)
            {
                fireDebug("  - Building Certificate Chain...");
                CertificateChain cert = CertificateChain.FromTikTmd(Path.Combine(outputDir, "cetk"), tmdFileWithCerts);

                byte[][] contents = new byte[tmd.NumOfContents][];

                for (int i = 0; i < tmd.NumOfContents; i++)
                    contents[i] = File.ReadAllBytes(Path.Combine(outputDir, (tmd.Contents[i].ContentID.ToString("x8") + ".app")));

                fireDebug("  - Creating WAD...");
                WAD wad = WAD.Create(cert, tik, tmd, contents);
                wad.RemoveFooter();
                wadName = wadName.Replace("[v]", "v" + this.TitleVersion.ToString()); // fix by madri2
                if (Path.DirectorySeparatorChar.ToString() != "/" && Path.AltDirectorySeparatorChar.ToString() != "/")
                    wadName = wadName.Replace("/", "");
                if (wadName.Contains(Path.DirectorySeparatorChar.ToString()) || wadName.Contains(Path.AltDirectorySeparatorChar.ToString()))
                    wad.Save(wadName);
                else
                    wad.Save(Path.Combine(outputDir, wadName));
            }

            //Delete not wanted files
            if (!storeEncrypted)
            {
                fireDebug("  - Deleting Encrypted Contents...");
                for (int i = 0; i < encryptedContents.Length; i++)
                    if (File.Exists(Path.Combine(outputDir, encryptedContents[i]))) File.Delete(Path.Combine(outputDir, encryptedContents[i]));
            }

            if (storeWad && !storeDecrypted)
            {
                fireDebug("  - Deleting Decrypted Contents...");
                for (int i = 0; i < encryptedContents.Length; i++)
                    if (File.Exists(Path.Combine(outputDir, (encryptedContents[i] + ".app")))) File.Delete(Path.Combine(outputDir, (encryptedContents[i] + ".app")));
            }

            if (!storeDecrypted && !storeEncrypted)
            {
                fireDebug("  - Deleting TMD and Ticket...");
                File.Delete(Path.Combine(outputDir, tmdFile));
                File.Delete(Path.Combine(outputDir, "cetk"));
            }

            fireDebug("Downloading Title {0} v{1} Finished...", titleId, tmd.TitleVersion /*(string.IsNullOrEmpty(titleVersion)) ? "[Latest]" : titleVersion*/);
            fireProgress(100);
        }

        private byte[] decryptContent(byte[] content, int contentIndex, Ticket tik, TMD tmd)
        {
            Array.Resize(ref content, Shared.AddPadding(content.Length, 16));
            byte[] titleKey = tik.TitleKey;
            byte[] iv = new byte[16];

            byte[] tmp = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
            iv[0] = tmp[1];
            iv[1] = tmp[0];

            RijndaelManaged rm = new RijndaelManaged();
            rm.Mode = CipherMode.CBC;
            rm.Padding = PaddingMode.None;
            rm.KeySize = 128;
            rm.BlockSize = 128;
            rm.Key = titleKey;
            rm.IV = iv;

            ICryptoTransform decryptor = rm.CreateDecryptor();

            MemoryStream ms = new MemoryStream(content);
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            byte[] decCont = new byte[content.Length];
            cs.Read(decCont, 0, decCont.Length);

            cs.Dispose();
            ms.Dispose();

            return decCont;
        }

        private bool CheckInet()
        {
            try
            {
                System.Net.IPHostEntry ipHost = System.Net.Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch { return false; }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires the Progress of various operations
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> Progress;
        /// <summary>
        /// Fires debugging messages. You may write them into a log file or log textbox.
        /// </summary>
        public event EventHandler<MessageEventArgs> Debug;

        private void fireDebug(string debugMessage, params object[] args)
        {
            EventHandler<MessageEventArgs> debug = Debug;
            if (debug != null)
                debug(new object(), new MessageEventArgs(string.Format(debugMessage, args)));
        }

        private void fireProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = Progress;
            if (progress != null)
                progress(new object(), new ProgressChangedEventArgs(progressPercentage, string.Empty));
        }
        #endregion
    }
}
