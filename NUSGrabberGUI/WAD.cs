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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public enum LowerTitleID : uint
    {
        SystemTitles = 0x00000001,
        SystemChannels = 0x00010002,
        Channel = 0x00010001,
        GameChannel = 0x00010004,
        DLC = 0x00010005,
        HiddenChannels = 0x00010008,
    }

    public class WAD : IDisposable
    {
        private SHA1 sha = SHA1.Create();
        private DateTime creationTimeUTC = new DateTime(1970, 1, 1);
        private bool hasBanner = false;
        private bool lz77CompressBannerAndIcon = true;
        private bool lz77DecompressBannerAndIcon = false;
        private bool keepOriginalFooter = false;

        private WAD_Header wadHeader;
        private CertificateChain cert = new CertificateChain();
        private Ticket tik = new Ticket();
        private TMD tmd = new TMD();
        private List<byte[]> contents;
        private U8 bannerApp = new U8();
        private byte[] footer = new byte[0];

        /// <summary>
        /// The region of the title.
        /// </summary>
        public Region Region { get { return tmd.Region; } set { tmd.Region = value; } }
        /// <summary>
        /// The Number of contents.
        /// </summary>
        public int NumOfContents { get { return tmd.NumOfContents; } }
        /// <summary>
        /// The content of the WAD.
        /// </summary>
        public byte[][] Contents { get { return contents.ToArray(); } }
        /// <summary>
        /// If true, the Ticket and TMD will be fakesigned.
        /// </summary>
        public bool FakeSign { get { return (tik.FakeSign && tmd.FakeSign); } set { tik.FakeSign = value; tmd.FakeSign = value; } }
        /// <summary>
        /// The banner app file (aka 00000000.app). Will be empty if HasBanner is false.
        /// </summary>
        public U8 BannerApp { get { return bannerApp; } set { bannerApp = value; } }
        /// <summary>
        /// The IOS the Title is launched with.
        /// </summary>
        public ulong StartupIOS { get { return tmd.StartupIOS; } set { tmd.StartupIOS = value; } }
        /// <summary>
        /// The Title ID.
        /// </summary>
        public ulong TitleID { get { return tik.TitleID; } set { tik.TitleID = value; tmd.TitleID = value; } }
        /// <summary>
        /// The upper Title ID as string.
        /// </summary>
        public string UpperTitleID { get { return tik.GetUpperTitleID(); } }
        /// <summary>
        /// The Title Version.
        /// </summary>
        public ushort TitleVersion { get { return tmd.TitleVersion; } set { tmd.TitleVersion = value; } }
        /// <summary>
        /// The boot index. Represents the index of the nand loader.
        /// </summary>
        public ushort BootIndex { get { return tmd.BootIndex; } set { tmd.BootIndex = value; } }
        /// <summary>
        /// The creation time of the Title. Will be 1/1/1970 if no Timestamp footer was found.
        /// </summary>
        public DateTime CreationTimeUTC { get { return creationTimeUTC; } }
        /// <summary>
        /// True if the WAD has a banner.
        /// </summary>
        public bool HasBanner { get { return hasBanner; } }
        /// <summary>
        /// If true, the banner.bin and icon.bin files will be Lz77 compressed while saving the WAD.
        /// </summary>
        public bool Lz77CompressBannerAndIcon { get { return lz77CompressBannerAndIcon; } set { lz77CompressBannerAndIcon = value; if (value) lz77DecompressBannerAndIcon = false; } }
        /// <summary>
        /// If true, the banner.bin and icon.bin files will be Lz77 decompressed while saving the WAD.
        /// </summary>
        public bool Lz77DecompressBannerAndIcon { get { return lz77DecompressBannerAndIcon; } set { lz77DecompressBannerAndIcon = value; if (value) lz77CompressBannerAndIcon = false; } }
        /// <summary>
        /// The Number of memory blocks the content will take.
        /// Might be inaccurate due to Lz77 (de)compression while saving.
        /// </summary>
        public string NandBlocks { get { return tmd.GetNandBlocks(); } }
        /// <summary>
        /// All Channel Titles as a string array. Will be empty if HasBanner is false.
        /// </summary>
        public string[] ChannelTitles { get { if (hasBanner) return ((Headers.IMET)bannerApp.Header).AllTitles; else return new string[0]; } set { ChangeChannelTitles(value); } }
        /// <summary>
        /// If false, a timestamp will be added as footer (64 bytes).
        /// Else, the original footer will be kept or the one you provided.
        /// </summary>
        public bool KeepOriginalFooter { get { return keepOriginalFooter; } set { keepOriginalFooter = value; } }
        /// <summary>
        /// The TMDs content entries.
        /// </summary>
        public TMD_Content[] TmdContents { get { return tmd.Contents; } }

        public WAD()
        {
            cert.Debug += new EventHandler<MessageEventArgs>(cert_Debug);
            tik.Debug += new EventHandler<MessageEventArgs>(tik_Debug);
            tmd.Debug += new EventHandler<MessageEventArgs>(tmd_Debug);
            bannerApp.Debug += new EventHandler<MessageEventArgs>(bannerApp_Debug);
            bannerApp.Warning += new EventHandler<MessageEventArgs>(bannerApp_Warning);
        }

		#region IDisposable Members
        private bool isDisposed = false;

        ~WAD()
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
                sha.Clear();
                sha = null;

                wadHeader = null;

                cert.Dispose();
                tik.Dispose();
                tmd.Dispose();

                contents.Clear();
                contents = null;

                bannerApp.Dispose();

                footer = null;
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="pathToWad"></param>
        /// <returns></returns>
        public static WAD Load(string pathToWad)
        {
            return Load(File.ReadAllBytes(pathToWad));
        }

        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="wadFile"></param>
        /// <returns></returns>
        public static WAD Load(byte[] wadFile)
        {
            WAD w = new WAD();
            MemoryStream ms = new MemoryStream(wadFile);

            try { w.parseWad(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
            return w;
        }

        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="wad"></param>
        /// <returns></returns>
        public static WAD Load(Stream wad)
        {
            WAD w = new WAD();
            w.parseWad(wad);
            return w;
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="contentDir"></param>
        /// <returns></returns>
        public static WAD Create(string contentDir)
        {
            string[] certPath = Directory.GetFiles(contentDir, "*cert*");
            string[] tikPath = Directory.GetFiles(contentDir, "*tik*");
            string[] tmdPath = Directory.GetFiles(contentDir, "*tmd*");

            CertificateChain _cert = CertificateChain.Load(certPath[0]);
            Ticket _tik = Ticket.Load(tikPath[0]);
            TMD _tmd = TMD.Load(tmdPath[0]);

            bool namedContentId = true;
            for (int i = 0; i < _tmd.Contents.Length; i++)
                if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].ContentID.ToString("x8") + ".app"))
                { namedContentId = false; break; }

            if (!namedContentId)
                for (int i = 0; i < _tmd.Contents.Length; i++)
                    if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].Index.ToString("x8") + ".app"))
                        throw new Exception("Couldn't find all content files!");

            byte[][] contents = new byte[_tmd.Contents.Length][];

            for (int i = 0; i < _tmd.Contents.Length; i++)
            {
                string file = contentDir + Path.DirectorySeparatorChar + ((namedContentId) ? _tmd.Contents[i].ContentID.ToString("x8") : _tmd.Contents[i].Index.ToString("x8")) + ".app";
                contents[i] = File.ReadAllBytes(file);
            }

            return Create(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="pathToCert"></param>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <param name="contentDir"></param>
        /// <returns></returns>
        public static WAD Create(string pathToCert, string pathToTik, string pathToTmd, string contentDir)
        {
            CertificateChain _cert = CertificateChain.Load(pathToCert);
            Ticket _tik = Ticket.Load(pathToTik);
            TMD _tmd = TMD.Load(pathToTmd);

            bool namedContentId = true;
            for (int i = 0; i < _tmd.Contents.Length; i++)
                if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].ContentID.ToString("x8") + ".app"))
                { namedContentId = false; break; }

            if (!namedContentId)
                for (int i = 0; i < _tmd.Contents.Length; i++)
                    if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].Index.ToString("x8") + ".app"))
                        throw new Exception("Couldn't find all content files!");

            byte[][] contents = new byte[_tmd.Contents.Length][];

            for (int i = 0; i < _tmd.Contents.Length; i++)
            {
                string file = contentDir + Path.DirectorySeparatorChar + ((namedContentId) ? _tmd.Contents[i].ContentID.ToString("x8") : _tmd.Contents[i].Index.ToString("x8")) + ".app";
                contents[i] = File.ReadAllBytes(file);
            }

            return Create(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static WAD Create(byte[] cert, byte[] tik, byte[] tmd, byte[][] contents)
        {
            CertificateChain _cert = CertificateChain.Load(cert);
            Ticket _tik = Ticket.Load(tik);
            TMD _tmd = TMD.Load(tmd);

            return Create(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public static WAD Create(CertificateChain cert, Ticket tik, TMD tmd, byte[][] contents)
        {
            WAD w = new WAD();
            w.cert = cert;
            w.tik = tik;
            w.tmd = tmd;
            w.contents = new List<byte[]>(contents);

            w.wadHeader = new WAD_Header();
            w.wadHeader.TmdSize = (uint)(484 + (tmd.Contents.Length * 36));

            int contentSize = 0;
            for (int i = 0; i < contents.Length - 1; i++)
                contentSize += Shared.AddPadding(contents[i].Length);

            contentSize += contents[contents.Length - 1].Length;

            w.wadHeader.ContentSize = (uint)contentSize;

            for (int i = 0; i < w.tmd.Contents.Length; i++)
                if (w.tmd.Contents[i].Index == 0x0000)
                {
                    try { w.bannerApp.LoadFile(contents[i]); w.hasBanner = true; }
                    catch { w.hasBanner = false; } //Probably System Wad => No Banner App...
                    break;
                }

            return w;
        }



        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="pathToWad"></param>
        public void LoadFile(string pathToWad)
        {
            LoadFile(File.ReadAllBytes(pathToWad));
        }

        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="wadFile"></param>
        public void LoadFile(byte[] wadFile)
        {
            MemoryStream ms = new MemoryStream(wadFile);

            try { parseWad(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
        }

        /// <summary>
        /// Loads a WAD file.
        /// </summary>
        /// <param name="wad"></param>
        public void LoadFile(Stream wad)
        {
            parseWad(wad);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="contentDir"></param>
        public void CreateNew(string contentDir)
        {
            string[] certPath = Directory.GetFiles(contentDir, "*cert*");
            string[] tikPath = Directory.GetFiles(contentDir, "*tik*");
            string[] tmdPath = Directory.GetFiles(contentDir, "*tmd*");

            CertificateChain _cert = CertificateChain.Load(certPath[0]);
            Ticket _tik = Ticket.Load(tikPath[0]);
            TMD _tmd = TMD.Load(tmdPath[0]);

            bool namedContentId = true;
            for (int i = 0; i < _tmd.Contents.Length; i++)
                if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].ContentID.ToString("x8") + ".app"))
                { namedContentId = false; break; }

            if (!namedContentId)
                for (int i = 0; i < _tmd.Contents.Length; i++)
                    if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].Index.ToString("x8") + ".app"))
                        throw new Exception("Couldn't find all content files!");

            byte[][] contents = new byte[_tmd.Contents.Length][];

            for (int i = 0; i < _tmd.Contents.Length; i++)
            {
                string file = contentDir + Path.DirectorySeparatorChar + ((namedContentId) ? _tmd.Contents[i].ContentID.ToString("x8") : _tmd.Contents[i].Index.ToString("x8")) + ".app";
                contents[i] = File.ReadAllBytes(file);
            }

            CreateNew(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="pathToCert"></param>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <param name="contentDir"></param>
        public void CreateNew(string pathToCert, string pathToTik, string pathToTmd, string contentDir)
        {
            CertificateChain _cert = CertificateChain.Load(pathToCert);
            Ticket _tik = Ticket.Load(pathToTik);
            TMD _tmd = TMD.Load(pathToTmd);

            bool namedContentId = true;
            for (int i = 0; i < _tmd.Contents.Length; i++)
                if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].ContentID.ToString("x8") + ".app"))
                { namedContentId = false; break; }

            if (!namedContentId)
                for (int i = 0; i < _tmd.Contents.Length; i++)
                    if (!File.Exists(contentDir + Path.DirectorySeparatorChar + _tmd.Contents[i].Index.ToString("x8") + ".app"))
                        throw new Exception("Couldn't find all content files!");

            byte[][] contents = new byte[_tmd.Contents.Length][];

            for (int i = 0; i < _tmd.Contents.Length; i++)
            {
                string file = contentDir + Path.DirectorySeparatorChar + ((namedContentId) ? _tmd.Contents[i].ContentID.ToString("x8") : _tmd.Contents[i].Index.ToString("x8")) + ".app";
                contents[i] = File.ReadAllBytes(file);
            }

            CreateNew(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <param name="contents"></param>
        public void CreateNew(byte[] cert, byte[] tik, byte[] tmd, byte[][] contents)
        {
            CertificateChain _cert = CertificateChain.Load(cert);
            Ticket _tik = Ticket.Load(tik);
            TMD _tmd = TMD.Load(tmd);

            CreateNew(_cert, _tik, _tmd, contents);
        }

        /// <summary>
        /// Creates a WAD file from contents.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <param name="contents"></param>
        public void CreateNew(CertificateChain cert, Ticket tik, TMD tmd, byte[][] contents)
        {
            this.cert = cert;
            this.tik = tik;
            this.tmd = tmd;
            this.contents = new List<byte[]>(contents);

            this.wadHeader = new WAD_Header();
            this.wadHeader.TmdSize = (uint)(484 + (tmd.Contents.Length * 36));

            int contentSize = 0;
            for (int i = 0; i < contents.Length - 1; i++)
                contentSize += Shared.AddPadding(contents[i].Length);

            contentSize += contents[contents.Length - 1].Length;

            this.wadHeader.ContentSize = (uint)contentSize;

            for (int i = 0; i < this.tmd.Contents.Length; i++)
                if (this.tmd.Contents[i].Index == 0x0000)
                {
                    try { this.bannerApp.LoadFile(contents[i]); hasBanner = true; }
                    catch { hasBanner = false; } //Probably System Wad => No Banner App...
                    break;
                }
        }


        /// <summary>
        /// Saves the WAD file to the given location.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            if (File.Exists(savePath)) File.Delete(savePath);

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
                writeToStream(fs);
        }

        /// <summary>
        /// Returns the WAD file as a memory stream.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToMemoryStream()
        {
            MemoryStream ms = new MemoryStream();

            try { writeToStream(ms); }
            catch { ms.Dispose(); throw; }

            return ms;
        }

        /// <summary>
        /// Returns the WAD file as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            MemoryStream ms = new MemoryStream();

            try { writeToStream(ms); }
            catch { ms.Dispose(); throw; }

            byte[] res = ms.ToArray();
            ms.Dispose();
            return res;
        }

        /// <summary>
        /// Changes the Title ID of the WAD file.
        /// </summary>
        /// <param name="lowerID"></param>
        /// <param name="upperID"></param>
        public void ChangeTitleID(LowerTitleID lowerID, string upperID)
        {
            if (upperID.Length != 4) throw new Exception("Upper Title ID must be 4 characters long!");

            byte[] temp = new byte[4];
            temp[0] = (byte)upperID[3];
            temp[1] = (byte)upperID[2];
            temp[2] = (byte)upperID[1];
            temp[3] = (byte)upperID[0];
            uint upper = BitConverter.ToUInt32(temp, 0);

            ulong newId = ((ulong)lowerID << 32) | upper;

            tik.TitleID = newId;
            tmd.TitleID = newId;
        }

        /// <summary>
        /// Changes the IOS the Title is launched with.
        /// </summary>
        /// <param name="newIos"></param>
        public void ChangeStartupIOS(int newIos)
        {
            StartupIOS = ((ulong)0x00000001 << 32) | (uint)newIos;
        }

        /// <summary>
        /// Changes the Title Key in the Ticket.
        /// The given value will be the encrypted Key (i.e. what you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void ChangeTitleKey(string newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        /// <summary>
        /// Changes the Title Key in the Ticket.
        /// The given value will be the encrypted Key (i.e. what you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void ChangeTitleKey(char[] newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        /// <summary>
        /// Changes the Title Key in the Ticket.
        /// The given value will be the encrypted Key (i.e. what you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void ChangeTitleKey(byte[] newTitleKey)
        {
            tik.SetTitleKey(newTitleKey);
        }

        /// <summary>
        /// Returns a content by it's TMD index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] GetContentByIndex(int index)
        {
            for (int i = 0; i < tmd.NumOfContents; i++)
                if (tmd.Contents[i].Index == index)
                    return contents[i];

            throw new Exception(string.Format("Content with index {0} not found!", index));
        }

        /// <summary>
        /// Returns a content by it's content ID.
        /// </summary>
        /// <param name="contentID"></param>
        /// <returns></returns>
        public byte[] GetContentByID(int contentID)
        {
            for (int i = 0; i < tmd.NumOfContents; i++)
                if (tmd.Contents[i].Index == contentID)
                    return contents[i];

            throw new Exception(string.Format("Content with content ID {0} not found!", contentID));
        }

        /// <summary>
        /// Changes the Channel Titles (Only if HasBanner is true).
        /// 0: Japanese,
        /// 1: English,
        /// 2: German,
        /// 3: French,
        /// 4: Spanish,
        /// 5: Italian,
        /// 6: Dutch,
        /// 7: Korean
        /// </summary>
        /// <param name="newTitles"></param>
        public void ChangeChannelTitles(params string[] newTitles)
        {
            if (hasBanner)
                ((Headers.IMET)bannerApp.Header).ChangeTitles(newTitles);
        }

        /// <summary>
        /// Adds a content to the WAD.
        /// </summary>
        /// <param name="newContent"></param>
        /// <param name="contentID"></param>
        /// <param name="index"></param>
        /// <param name="type"></param>
        public void AddContent(byte[] newContent, int contentID, int index, ContentType type = ContentType.Normal)
        {
            TMD_Content temp = new TMD_Content();
            temp.ContentID = (uint)contentID;
            temp.Index = (ushort)index;
            temp.Type = type;
            temp.Size = (ulong)newContent.Length;
            temp.Hash = sha.ComputeHash(newContent);

            tmd.AddContent(temp);
            contents.Add(newContent);

            wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
        }

        /// <summary>
        /// Removes a content from the WAD.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveContent(int index)
        {
            for (int i = 0; i < tmd.Contents.Length; i++)
                if (tmd.Contents[i].Index == index)
                { tmd.RemoveContent(index); contents.RemoveAt(i); wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36); return; }

            throw new Exception(string.Format("Content with index {0} not found!", index));
        }

        /// <summary>
        /// Removes a content by it's content ID.
        /// </summary>
        /// <param name="contentID"></param>
        public void RemoveContentByID(int contentID)
        {
            for (int i = 0; i < tmd.Contents.Length; i++)
                if (tmd.Contents[i].Index == contentID)
                { tmd.RemoveContentByID(contentID); contents.RemoveAt(i); wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36); return; }

            throw new Exception(string.Format("Content with content ID {0} not found!", contentID));
        }

        /// <summary>
        /// Removes all contents from the WAD. If HasBanner is true, the banner content (Index 0) won't be removed!
        /// </summary>
        public void RemoveAllContents()
        {
            if (!hasBanner)
            {
                tmd.Contents = new TMD_Content[0];
                contents = new List<byte[]>();

                wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
            }
            else
            {
                for (int i=0;i<tmd.NumOfContents;i++)
                    if (tmd.Contents[i].Index == 0)
                    {
                        byte[] tmpCont = contents[i];
                        TMD_Content tmpTmdCont = tmd.Contents[i];

                        tmd.Contents = new TMD_Content[0];
                        contents = new List<byte[]>();

                        tmd.AddContent(tmpTmdCont);
                        contents.Add(tmpCont);

                        wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);
                        break;
                    }
            }
        }

        /// <summary>
        /// Unpacks the WAD to the given directory.
        /// If nameContentID is true, contents are named after their content ID, else after their index.
        /// </summary>
        /// <param name="unpackDir"></param>
        /// <param name="nameContentID"></param>
        public void Unpack(string unpackDir, bool nameContentID = false)
        {
            unpackAll(unpackDir, nameContentID);
        }

        /// <summary>
        /// Removes the footer.
        /// </summary>
        public void RemoveFooter()
        {
            this.footer = new byte[0];
            wadHeader.FooterSize = 0;

            this.keepOriginalFooter = true;
        }

        /// <summary>
        /// Adds a footer.
        /// </summary>
        /// <param name="footer"></param>
        public void AddFooter(byte[] footer)
        {
            ChangeFooter(footer);
        }

        /// <summary>
        /// Changes the footer.
        /// </summary>
        /// <param name="newFooter"></param>
        public void ChangeFooter(byte[] newFooter)
        {
            if (newFooter.Length % 64 != 0)
                Array.Resize(ref newFooter, Shared.AddPadding(newFooter.Length));

            this.footer = newFooter;
            wadHeader.FooterSize = (uint)newFooter.Length;

            this.keepOriginalFooter = true;
        }
        #endregion

        #region Private Functions
        private void writeToStream(Stream writeStream)
        {
            fireDebug("Writing Wad...");

            //Create Footer Timestamp
            if (!keepOriginalFooter)
            {
                fireDebug("   Building Footer Timestamp...");
                createFooterTimestamp();
            }

            //Save Banner App
            if (hasBanner)
            {
                //Compress icon.bin and banner.bin
                if (lz77CompressBannerAndIcon || lz77DecompressBannerAndIcon)
                {
                    for (int i = 0; i < bannerApp.Nodes.Count; i++)
                    {
                        if (bannerApp.StringTable[i].ToLower() == "icon.bin" ||
                            bannerApp.StringTable[i].ToLower() == "banner.bin")
                        {
                            if (!Lz77.IsLz77Compressed(bannerApp.Data[i]) && lz77CompressBannerAndIcon)
                            {
                                fireDebug("   Compressing {0}...", bannerApp.StringTable[i]);

                                //Get the data without the IMD5 Header
                                byte[] fileData = new byte[bannerApp.Data[i].Length - 32];
                                Array.Copy(bannerApp.Data[i], 32, fileData, 0, fileData.Length);

                                //Compress the data
                                Lz77 l = new Lz77();
                                fileData = l.Compress(fileData);

                                //Add a new IMD5 Header
                                fileData = Headers.IMD5.AddHeader(fileData);
                                bannerApp.Data[i] = fileData;

                                //Update the node
                                bannerApp.Nodes[i].SizeOfData = (uint)fileData.Length;
                            }
                            else if (Lz77.IsLz77Compressed(bannerApp.Data[i]) && lz77DecompressBannerAndIcon)
                            {
                                fireDebug("   Decompressing {0}...", bannerApp.StringTable[i]);

                                //Get the data without the IMD5 Header
                                byte[] fileData = new byte[bannerApp.Data[i].Length - 32];
                                Array.Copy(bannerApp.Data[i], 32, fileData, 0, fileData.Length);

                                //Decompress the data
                                Lz77 l = new Lz77();
                                fileData = l.Decompress(fileData);

                                //Add a new IMD5 Header
                                fileData = Headers.IMD5.AddHeader(fileData);
                                bannerApp.Data[i] = fileData;

                                //Update the node
                                bannerApp.Nodes[i].SizeOfData = (uint)fileData.Length;
                            }
                        }
                    }
                }

                for (int i = 0; i < contents.Count; i++)
                    if (tmd.Contents[i].Index == 0x0000)
                    { fireDebug("   Saving Banner App..."); contents[i] = bannerApp.ToByteArray(); break; }
            }

            //Update Header (Content Size)
            fireDebug("   Updating Header...");
            int contentSize = 0;
            for (int i = 0; i < contents.Count - 1; i++)
                contentSize += Shared.AddPadding(contents[i].Length);

            contentSize += contents[contents.Count - 1].Length;

            wadHeader.ContentSize = (uint)contentSize;
            wadHeader.TmdSize = (uint)(484 + tmd.NumOfContents * 36);

            //Update Contents
            fireDebug("   Updating TMD Contents...");
            tmd.UpdateContents(contents.ToArray());

            //Write Header
            fireDebug("   Writing Wad Header... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(0, SeekOrigin.Begin);
            wadHeader.Write(writeStream);

            //Write Cert
            fireDebug("   Writing Certificate Chain... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            byte[] temp = cert.ToByteArray();
            writeStream.Write(temp, 0, temp.Length);

            //Write Tik
            fireDebug("   Writing Ticket... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            temp = tik.ToByteArray();
            writeStream.Write(temp, 0, temp.Length);

            //Write TMD
            fireDebug("   Writing TMD... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper());
            writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
            temp = tmd.ToByteArray();
            writeStream.Write(temp, 0, temp.Length);

            //Write Contents
            List<ContentIndices> contentList = new List<ContentIndices>();
            for (int i = 0; i < tmd.Contents.Length; i++)
                contentList.Add(new ContentIndices(i, tmd.Contents[i].Index));

            contentList.Sort();

            for (int i = 0; i < contentList.Count; i++)
            {
                writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);

                fireProgress((i + 1) * 100 / contents.Count);

                fireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper(), i + 1, contents.Count);
                fireDebug("    -> Content ID: 0x{0}", tmd.Contents[contentList[i].Index].ContentID.ToString("x8"));
                fireDebug("    -> Index: 0x{0}", tmd.Contents[contentList[i].Index].Index.ToString("x4"));
                fireDebug("    -> Type: 0x{0} ({1})", ((ushort)tmd.Contents[contentList[i].Index].Type).ToString("x4"), tmd.Contents[contentList[i].Index].Type.ToString());
                fireDebug("    -> Size: {0} bytes", tmd.Contents[contentList[i].Index].Size);
                fireDebug("    -> Hash: {0}", Shared.ByteArrayToString(tmd.Contents[contentList[i].Index].Hash));

                temp = encryptContent(contents[contentList[i].Index], contentList[i].Index);
                writeStream.Write(temp, 0, temp.Length);
            }

            //Write Footer
            if (wadHeader.FooterSize > 0)
            {
                fireDebug("   Writing Footer... (Offset: 0x{0})", writeStream.Position.ToString("x8").ToUpper());
                writeStream.Seek(Shared.AddPadding((int)writeStream.Position), SeekOrigin.Begin);
                writeStream.Write(footer, 0, footer.Length);
            }

            //Padding
            while (writeStream.Position % 64 != 0)
                writeStream.WriteByte(0x00);

            fireDebug("Writing Wad Finished... (Written Bytes: {0})", writeStream.Position);
        }

        private void unpackAll(string unpackDir, bool nameContentId)
        {
            fireDebug("Unpacking Wad to: {0}", unpackDir);

            if (!Directory.Exists(unpackDir)) Directory.CreateDirectory(unpackDir);
            string titleID = tik.TitleID.ToString("x16");

            //Save Cert
            fireDebug("   Saving Certificate Chain: {0}.cert", titleID);
            cert.Save(unpackDir + Path.DirectorySeparatorChar + titleID + ".cert");

            //Save Tik
            fireDebug("   Saving Ticket: {0}.tik", titleID);
            tik.Save(unpackDir + Path.DirectorySeparatorChar + titleID + ".tik");

            //Save TMD
            fireDebug("   Saving TMD: {0}.tmd", titleID);
            tmd.Save(unpackDir + Path.DirectorySeparatorChar + titleID + ".tmd");

            //Save Contents
            for (int i = 0; i < tmd.NumOfContents; i++)
            {
                fireProgress((i + 1) * 100 / tmd.NumOfContents);

                fireDebug("   Saving Content #{0} of {1}: {2}.app", i + 1, tmd.NumOfContents, (nameContentId ? tmd.Contents[i].ContentID.ToString("x8") : tmd.Contents[i].Index.ToString("x8")));
                fireDebug("    -> Content ID: 0x{0}", tmd.Contents[i].ContentID.ToString("x8"));
                fireDebug("    -> Index: 0x{0}", tmd.Contents[i].Index.ToString("x4"));
                fireDebug("    -> Type: 0x{0} ({1})", ((ushort)tmd.Contents[i].Type).ToString("x4"), tmd.Contents[i].Type.ToString());
                fireDebug("    -> Size: {0} bytes", tmd.Contents[i].Size);
                fireDebug("    -> Hash: {0}", Shared.ByteArrayToString(tmd.Contents[i].Hash));

                using (FileStream fs = new FileStream(unpackDir + Path.DirectorySeparatorChar +
                    (nameContentId ? tmd.Contents[i].ContentID.ToString("x8") : tmd.Contents[i].Index.ToString("x8")) + ".app",
                    FileMode.Create))
                    fs.Write(contents[i], 0, contents[i].Length);
            }

            //Save Footer
            fireDebug("   Saving Footer: {0}.footer", titleID);
            using (FileStream fs = new FileStream(unpackDir + Path.DirectorySeparatorChar + titleID + ".footer", FileMode.Create))
                fs.Write(footer, 0, footer.Length);

            fireDebug("Unpacking Wad Finished...");
        }

        private void parseWad(Stream wadFile)
        {
            fireDebug("Parsing Wad...");

            wadFile.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[4];

            wadHeader = new WAD_Header();
            contents = new List<byte[]>();

            //Read Header
            fireDebug("   Parsing Header... (Offset: 0x{0})", wadFile.Position.ToString("x8").ToUpper());
            wadFile.Read(temp, 0, 4);
            if (Shared.Swap(BitConverter.ToUInt32(temp, 0)) != wadHeader.HeaderSize)
                throw new Exception("Invalid Headersize!");

            wadFile.Read(temp, 0, 4);
            wadHeader.WadType = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            wadFile.Seek(12, SeekOrigin.Current);

            wadFile.Read(temp, 0, 4);
            wadHeader.TmdSize = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            wadFile.Read(temp, 0, 4);
            wadHeader.ContentSize = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            wadFile.Read(temp, 0, 4);
            wadHeader.FooterSize = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            //Read Cert
            fireDebug("   Parsing Certificate Chain... (Offset: 0x{0})", wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);

            temp = new byte[wadHeader.CertSize];
            wadFile.Read(temp, 0, temp.Length);
            cert.LoadFile(temp);

            //Read Tik
            fireDebug("   Parsing Ticket... (Offset: 0x{0})", wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);

            temp = new byte[wadHeader.TicketSize];
            wadFile.Read(temp, 0, temp.Length);
            tik.LoadFile(temp);

            //Read Tmd
            fireDebug("   Parsing TMD... (Offset: 0x{0})", wadFile.Position.ToString("x8").ToUpper());
            wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);

            temp = new byte[wadHeader.TmdSize];
            wadFile.Read(temp, 0, temp.Length);
            tmd.LoadFile(temp);

            if (tmd.TitleID != tik.TitleID)
                fireWarning("The Title ID in the Ticket doesn't match the one in the TMD!");

            //Read Content
            for (int i = 0; i < tmd.NumOfContents; i++)
            {
                fireProgress((i + 1) * 100 / tmd.NumOfContents);

                fireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", i + 1, tmd.NumOfContents, wadFile.Position.ToString("x8").ToUpper());
                fireDebug("    -> Content ID: 0x{0}", tmd.Contents[i].ContentID.ToString("x8"));
                fireDebug("    -> Index: 0x{0}", tmd.Contents[i].Index.ToString("x4"));
                fireDebug("    -> Type: 0x{0} ({1})", ((ushort)tmd.Contents[i].Type).ToString("x4"), tmd.Contents[i].Type.ToString());
                fireDebug("    -> Size: {0} bytes", tmd.Contents[i].Size);
                fireDebug("    -> Hash: {0}", Shared.ByteArrayToString(tmd.Contents[i].Hash));

                wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);

                temp = new byte[Shared.AddPadding((int)tmd.Contents[i].Size, 16)];
                wadFile.Read(temp, 0, temp.Length);

                //Decrypt Content
                temp = decryptContent(temp, i);
                Array.Resize(ref temp, (int)tmd.Contents[i].Size);

                byte[] tmdHash = tmd.Contents[i].Hash;
                byte[] newHash = sha.ComputeHash(temp, 0, (int)tmd.Contents[i].Size);

                if (!Shared.CompareByteArrays(tmdHash, newHash))
                {
                    fireDebug(@"/!\ /!\ /!\ Hashes do not match /!\ /!\ /!\");
                    fireWarning(string.Format("Content #{0} (Content ID: 0x{1}; Index: 0x{2}): Hashes do not match! The content might be corrupted!", i + 1, tmd.Contents[i].ContentID.ToString("x8"), tmd.Contents[i].Index.ToString("x4")));
                }

                contents.Add(temp);

                if (tmd.Contents[i].Index == 0x0000)
                {
                    try { bannerApp.LoadFile(temp); hasBanner = true; }
                    catch { hasBanner = false; } //Probably System Wad => No Banner App...
                }
            }

            //Read Footer
            if (wadHeader.FooterSize > 0)
            {
                fireDebug("   Reading Footer... (Offset: 0x{0})", wadFile.Position.ToString("x8").ToUpper());
                footer = new byte[wadHeader.FooterSize];

                wadFile.Seek(Shared.AddPadding((int)wadFile.Position), SeekOrigin.Begin);
                wadFile.Read(footer, 0, footer.Length);

                parseFooterTimestamp();
            }

            fireDebug("Parsing Wad Finished...");
        }

        private byte[] decryptContent(byte[] content, int contentIndex)
        {
            int originalLength = content.Length;
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

            byte[] decCont = new byte[originalLength];
            cs.Read(decCont, 0, decCont.Length);

            cs.Dispose();
            ms.Dispose();

            return decCont;
        }

        private byte[] encryptContent(byte[] content, int contentIndex)
        {
            Array.Resize(ref content, Shared.AddPadding(content.Length, 16));
            byte[] titleKey = tik.TitleKey;
            byte[] iv = new byte[16];

            byte[] tmp = BitConverter.GetBytes(tmd.Contents[contentIndex].Index);
            iv[0] = tmp[1];
            iv[1] = tmp[0];

            RijndaelManaged encrypt = new RijndaelManaged();
            encrypt.Mode = CipherMode.CBC;
            encrypt.Padding = PaddingMode.None;
            encrypt.KeySize = 128;
            encrypt.BlockSize = 128;
            encrypt.Key = titleKey;
            encrypt.IV = iv;

            ICryptoTransform encryptor = encrypt.CreateEncryptor();

            MemoryStream ms = new MemoryStream(content);
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);

            byte[] encCont = new byte[content.Length];
            cs.Read(encCont, 0, encCont.Length);

            cs.Dispose();
            ms.Dispose();

            return encCont;
        }

        private void createFooterTimestamp()
        {
            DateTime dtNow = DateTime.UtcNow;
            TimeSpan tsTimestamp = (dtNow - new DateTime(1970, 1, 1, 0, 0, 0));

            int timeStamp = (int)tsTimestamp.TotalSeconds;
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            byte[] timeStampBytes = enc.GetBytes("TmStmp" + timeStamp.ToString());
            Array.Resize(ref timeStampBytes, 64);

            wadHeader.FooterSize = (uint)timeStampBytes.Length;
            footer = timeStampBytes;
        }

        private void parseFooterTimestamp()
        {
            creationTimeUTC = new DateTime(1970, 1, 1);

            if ((footer[0] == 'C' && footer[1] == 'M' && footer[2] == 'i' &&
                footer[3] == 'i' && footer[4] == 'U' && footer[5] == 'T') ||
                (footer[0] == 'T' && footer[1] == 'm' && footer[2] == 'S' &&
                footer[3] == 't' && footer[4] == 'm' && footer[5] == 'p'))
            {
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                string stringSeconds = enc.GetString(footer, 6, 10);
                int seconds = 0;

                if (int.TryParse(stringSeconds, out seconds))
                    creationTimeUTC = creationTimeUTC.AddSeconds((double)seconds);
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires the Progress of various operations
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> Progress;
        /// <summary>
        /// Fires warnings (e.g. when hashes don't match)
        /// </summary>
        public event EventHandler<MessageEventArgs> Warning;
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

        private void fireWarning(string warningMessage)
        {
            EventHandler<MessageEventArgs> warning = Warning;
            if (warning != null)
                warning(new object(), new MessageEventArgs(warningMessage));
        }

        private void fireProgress(int progressPercentage)
        {
            EventHandler<ProgressChangedEventArgs> progress = Progress;
            if (progress != null)
                progress(new object(), new ProgressChangedEventArgs(progressPercentage, string.Empty));
        }

        private void cert_Debug(object sender, MessageEventArgs e)
        {
            fireDebug("      Certificate Chain: {0}", e.Message);
        }

        private void tik_Debug(object sender, MessageEventArgs e)
        {
            fireDebug("      Ticket: {0}", e.Message);
        }

        private void tmd_Debug(object sender, MessageEventArgs e)
        {
            fireDebug("      TMD: {0}", e.Message);
        }

        void bannerApp_Debug(object sender, MessageEventArgs e)
        {
            fireDebug("      BannerApp: {0}", e.Message);
        }

        void bannerApp_Warning(object sender, MessageEventArgs e)
        {
            fireWarning(e.Message);
        }
        #endregion
    }

    public class WAD_Header
    {
        private uint headerSize = 0x20;
        private uint wadType = 0x49730000;
        private uint certSize = 0xA00;
        private uint reserved = 0x00;
        private uint tikSize = 0x2A4;
        private uint tmdSize;
        private uint contentSize;
        private uint footerSize = 0x00;

        public uint HeaderSize { get { return headerSize; } }
        public uint WadType { get { return wadType; } set { wadType = value; } }
        public uint CertSize { get { return certSize; } }
        public uint Reserved { get { return reserved; } }
        public uint TicketSize { get { return tikSize; } }
        public uint TmdSize { get { return tmdSize; } set { tmdSize = value; } }
        public uint ContentSize { get { return contentSize; } set { contentSize = value; } }
        public uint FooterSize { get { return footerSize; } set { footerSize = value; } }

        public void Write(Stream writeStream)
        {
            writeStream.Seek(0, SeekOrigin.Begin);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(wadType)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(certSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(reserved)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tikSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(tmdSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(contentSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(footerSize)), 0, 4);
        }
    }
}
