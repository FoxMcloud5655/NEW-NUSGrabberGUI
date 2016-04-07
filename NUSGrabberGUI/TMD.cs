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
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public enum ContentType : ushort
    {
        Normal = 0x0001,
        DLC = 0x4001, //Seen this in a DLC wad...
        Shared = 0x8001,
    }

    public enum Region : ushort
    {
        Japan = 0,
        USA = 1,
        Europe = 2,
        Free = 3,
    }

    public class TMD : IDisposable
    {
        private bool fakeSign = false;

        private uint signatureExponent = 0x00010001;
        private byte[] signature = new byte[256];
        private byte[] padding = new byte[60];
        private byte[] issuer = new byte[64];
        private byte version;
        private byte caCrlVersion;
        private byte signerCrlVersion;
        private byte paddingByte;
        private ulong startupIos;
        private ulong titleId;
        private uint titleType;
        private ushort groupId;
        private ushort padding2;
        private ushort region;
        private byte[] reserved = new byte[58];
        private uint accessRights;
        private ushort titleVersion;
        private ushort numOfContents;
        private ushort bootIndex;
        private ushort padding3;
        private List<TMD_Content> contents;

        /// <summary>
        /// The region of the title.
        /// </summary>
        public Region Region { get { return (Region)region; } set { region = (ushort)value; } }
        /// <summary>
        /// The IOS the title is launched with.
        /// </summary>
        public ulong StartupIOS { get { return startupIos; } set { startupIos = value; } }
        /// <summary>
        /// The Title ID.
        /// </summary>
        public ulong TitleID { get { return titleId; } set { titleId = value; } }
        /// <summary>
        /// The Title Version.
        /// </summary>
        public ushort TitleVersion { get { return titleVersion; } set { titleVersion = value; } }
        /// <summary>
        /// The Number of Contents.
        /// </summary>
        public ushort NumOfContents { get { return numOfContents; } }
        /// <summary>
        /// The boot index. Represents the index of the nand loader.
        /// </summary>
        public ushort BootIndex { get { return bootIndex; } set {  if (value <= numOfContents) bootIndex = value; } }
        /// <summary>
        /// The content descriptions in the TMD.
        /// </summary>
        public TMD_Content[] Contents { get { return contents.ToArray(); } set { contents = new List<TMD_Content>(value); numOfContents = (ushort)value.Length; } }
        /// <summary>
        /// If true, the TMD will be fakesigned while saving.
        /// </summary>
        public bool FakeSign { get { return fakeSign; } set { fakeSign = value; } }

		#region IDisposable Members
        private bool isDisposed = false;

        ~TMD()
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
                signature = null;
                padding = null;
                issuer = null;
                reserved = null;

                contents.Clear();
                contents = null;
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public static TMD Load(string pathToTmd)
        {
            return Load(File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="tmdFile"></param>
        /// <returns></returns>
        public static TMD Load(byte[] tmdFile)
        {
            TMD t = new TMD();
            MemoryStream ms = new MemoryStream(tmdFile);

            try { t.parseTmd(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
            return t;
        }

        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="tmd"></param>
        /// <returns></returns>
        public static TMD Load(Stream tmd)
        {
            TMD t = new TMD();
            t.parseTmd(tmd);
            return t;
        }



        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="pathToTmd"></param>
        public void LoadFile(string pathToTmd)
        {
            LoadFile(File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="tmdFile"></param>
        public void LoadFile(byte[] tmdFile)
        {
            MemoryStream ms = new MemoryStream(tmdFile);

            try { parseTmd(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
        }

        /// <summary>
        /// Loads a tmd file.
        /// </summary>
        /// <param name="tmd"></param>
        public void LoadFile(Stream tmd)
        {
            parseTmd(tmd);
        }



        /// <summary>
        /// Saves the TMD.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            Save(savePath, false);
        }

        /// <summary>
        /// Saves the TMD. If fakeSign is true, the Ticket will be fakesigned.
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="fakeSign"></param>
        public void Save(string savePath, bool fakeSign)
        {
            if (fakeSign) this.fakeSign = true;
            if (File.Exists(savePath)) File.Delete(savePath);

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
                writeToStream(fs);
        }

        /// <summary>
        /// Returns the TMD as a memory stream.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToMemoryStream()
        {
            return ToMemoryStream(false);
        }

        /// <summary>
        /// Returns the TMD as a memory stream. If fakeSign is true, the Ticket will be fakesigned.
        /// </summary>
        /// <param name="fakeSign"></param>
        /// <returns></returns>
        public MemoryStream ToMemoryStream(bool fakeSign)
        {
            if (fakeSign) this.fakeSign = true;
            MemoryStream ms = new MemoryStream();

            try { writeToStream(ms); }
            catch { ms.Dispose(); throw; }

            return ms;
        }

        /// <summary>
        /// Returns the TMD as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ToByteArray(false);
        }

        /// <summary>
        /// Returns the TMD as a byte array. If fakeSign is true, the Ticket will be fakesigned.
        /// </summary>
        /// <param name="fakeSign"></param>
        /// <returns></returns>
        public byte[] ToByteArray(bool fakeSign)
        {
            if (fakeSign) this.fakeSign = true;
            MemoryStream ms = new MemoryStream();

            try { writeToStream(ms); }
            catch { ms.Dispose(); throw; }

            byte[] res = ms.ToArray();
            ms.Dispose();
            return res;
        }

        /// <summary>
        /// Updates the content entries.
        /// </summary>
        /// <param name="contentDir"></param>
        /// <param name="namedContentId">True if you use the content ID as name (e.g. 0000008a.app).
        /// False if you use the index as name (e.g. 00000000.app)</param>
        public void UpdateContents(string contentDir)
        {
            bool namedContentId = true;
            for (int i = 0; i < contents.Count; i++)
                if (!File.Exists(contentDir + Path.DirectorySeparatorChar + contents[i].ContentID.ToString("x8") + ".app"))
                { namedContentId = false; break; }

            if (!namedContentId)
                for (int i = 0; i < contents.Count; i++)
                    if (!File.Exists(contentDir + Path.DirectorySeparatorChar + contents[i].ContentID.ToString("x8") + ".app"))
                        throw new Exception("Couldn't find all content files!");

            byte[][] conts = new byte[contents.Count][];

            for (int i = 0; i < contents.Count; i++)
            {
                string file = contentDir + Path.DirectorySeparatorChar + ((namedContentId) ? contents[i].ContentID.ToString("x8") : contents[i].Index.ToString("x8")) + ".app";
                conts[i] = File.ReadAllBytes(file);
            }

            updateContents(conts);
        }

        /// <summary>
        /// Updates the content entries.
        /// </summary>
        /// <param name="contentDir"></param>
        /// <param name="namedContentId">True if you use the content ID as name (e.g. 0000008a.app).
        /// False if you use the index as name (e.g. 00000000.app)</param>
        public void UpdateContents(byte[][] contents)
        {
            updateContents(contents);
        }

        /// <summary>
        /// Returns the Upper Title ID as a string.
        /// </summary>
        /// <returns></returns>
        public string GetUpperTitleID()
        {
            byte[] titleBytes = BitConverter.GetBytes(Shared.Swap((uint)titleId));
            return new string(new char[] { (char)titleBytes[0], (char)titleBytes[1], (char)titleBytes[2], (char)titleBytes[3] });
        }

        /// <summary>
        /// The Number of memory blocks the content will take.
        /// </summary>
        /// <returns></returns>
        public string GetNandBlocks()
        {
            return calculateNandBlocks();
        }

        /// <summary>
        /// Adds a TMD content.
        /// </summary>
        /// <param name="content"></param>
        public void AddContent(TMD_Content content)
        {
            contents.Add(content);

            numOfContents = (ushort)contents.Count;
        }

        /// <summary>
        /// Removes the content with the given index.
        /// </summary>
        /// <param name="contentIndex"></param>
        public void RemoveContent(int contentIndex)
        {
            for (int i = 0; i < numOfContents; i++)
                if (contents[i].Index == contentIndex)
                { contents.RemoveAt(i); break; }

            numOfContents = (ushort)contents.Count;
        }

        /// <summary>
        /// Removes the content with the given ID.
        /// </summary>
        /// <param name="contentId"></param>
        public void RemoveContentByID(int contentId)
        {
            for (int i = 0; i < numOfContents; i++)
                if (contents[i].ContentID == contentId)
                { contents.RemoveAt(i); break; }

            numOfContents = (ushort)contents.Count;
        }
        #endregion

        #region Private Functions
        private void writeToStream(Stream writeStream)
        {
            fireDebug("Writing TMD...");

            if (fakeSign)
            { fireDebug("   Clearing Signature..."); signature = new byte[256]; } //Clear Signature if we fake Sign

            MemoryStream ms = new MemoryStream();
            ms.Seek(0, SeekOrigin.Begin);

            fireDebug("   Writing Signature Exponent... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(signatureExponent)), 0, 4);

            fireDebug("   Writing Signature... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(signature, 0, signature.Length);

            fireDebug("   Writing Padding... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(padding, 0, padding.Length);

            fireDebug("   Writing Issuer... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(issuer, 0, issuer.Length);

            fireDebug("   Writing Version... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(version);

            fireDebug("   Writing CA Crl Version... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(caCrlVersion);

            fireDebug("   Writing Signer Crl Version... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(signerCrlVersion);

            fireDebug("   Writing Padding Byte... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(paddingByte);

            fireDebug("   Writing Startup IOS... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(startupIos)), 0, 8);

            fireDebug("   Writing Title ID... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(titleId)), 0, 8);

            fireDebug("   Writing Title Type... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(titleType)), 0, 4);

            fireDebug("   Writing Group ID... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(groupId)), 0, 2);

            fireDebug("   Writing Padding2... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(padding2)), 0, 2);

            fireDebug("   Writing Region... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(region)), 0, 2);

            fireDebug("   Writing Reserved... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(reserved, 0, reserved.Length);

            fireDebug("   Writing Access Rights... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(accessRights)), 0, 4);

            fireDebug("   Writing Title Version... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(titleVersion)), 0, 2);

            fireDebug("   Writing NumOfContents... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(numOfContents)), 0, 2);

            fireDebug("   Writing Boot Index... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(bootIndex)), 0, 2);

            fireDebug("   Writing Padding3... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(padding3)), 0, 2);

            //Write Contents
            List<ContentIndices> contentList = new List<ContentIndices>();
            for (int i = 0; i < contents.Count; i++)
                contentList.Add(new ContentIndices(i, contents[i].Index));

            contentList.Sort();

            for (int i = 0; i < contentList.Count; i++)
            {
                fireDebug("   Writing Content #{1} of {2}... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper().ToUpper(), i + 1, numOfContents);

                ms.Write(BitConverter.GetBytes(Shared.Swap(contents[contentList[i].Index].ContentID)), 0, 4);
                ms.Write(BitConverter.GetBytes(Shared.Swap(contents[contentList[i].Index].Index)), 0, 2);
                ms.Write(BitConverter.GetBytes(Shared.Swap((ushort)contents[contentList[i].Index].Type)), 0, 2);
                ms.Write(BitConverter.GetBytes(Shared.Swap(contents[contentList[i].Index].Size)), 0, 8);

                ms.Write(contents[contentList[i].Index].Hash, 0, contents[contentList[i].Index].Hash.Length);
            }

            //fake Sign
            byte[] tmd = ms.ToArray();
            ms.Dispose();

            if (fakeSign)
            {
                fireDebug("   Fakesigning TMD...");

                byte[] hash = new byte[20];
                SHA1 s = SHA1.Create();

                for (ushort i = 0; i < 0xFFFF; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(i);
                    tmd[482] = bytes[1]; tmd[483] = bytes[0];

                    hash = s.ComputeHash(tmd);
                    if (hash[0] == 0x00)
                    { fireDebug("   -> Signed ({0})", i); break; } //Win! It's signed...

                    if (i == 0xFFFF - 1)
                    { fireDebug("    -> Signing Failed..."); throw new Exception("Fakesigning failed..."); }
                }

                s.Clear();
            }

            writeStream.Seek(0, SeekOrigin.Begin);
            writeStream.Write(tmd, 0, tmd.Length);

            fireDebug("Writing TMD Finished...");
        }

        private void updateContents(byte[][] conts)
        {
            SHA1 s = SHA1.Create();

            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].Size = (ulong)conts[i].Length;
                contents[i].Hash = s.ComputeHash(conts[i]);
            }

            s.Clear();
        }

        private void parseTmd(Stream tmdFile)
        {
            fireDebug("Pasing TMD...");

            tmdFile.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[8];

            fireDebug("   Reading Signature Exponent... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 4);
            signatureExponent = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading Signature... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(signature, 0, signature.Length);

            fireDebug("   Reading Padding... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(padding, 0, padding.Length);

            fireDebug("   Reading Issuer... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(issuer, 0, issuer.Length);

            fireDebug("   Reading Version... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading CA Crl Version... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading Signer Crl Version... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading Padding Byte... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 4);
            version = temp[0]; 
            caCrlVersion = temp[1];
            signerCrlVersion = temp[2];
            paddingByte = temp[3];

            fireDebug("   Reading Startup IOS... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 8);
            startupIos = Shared.Swap(BitConverter.ToUInt64(temp, 0));

            fireDebug("   Reading Title ID... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 8);
            titleId = Shared.Swap(BitConverter.ToUInt64(temp, 0));

            fireDebug("   Reading Title Type... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 4);
            titleType = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading Group ID... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 2);
            groupId = Shared.Swap(BitConverter.ToUInt16(temp, 0));

            fireDebug("   Reading Padding2... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 2);
            padding2 = Shared.Swap(BitConverter.ToUInt16(temp, 0));

            fireDebug("   Reading Region... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 2);
            region = Shared.Swap(BitConverter.ToUInt16(temp, 0));

            fireDebug("   Reading Reserved... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(reserved, 0, reserved.Length);

            fireDebug("   Reading Access Rights... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 4);
            accessRights = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading Title Version... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading NumOfContents... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading Boot Index... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading Padding3... (Offset: 0x{0})", tmdFile.Position.ToString("x8").ToUpper());
            tmdFile.Read(temp, 0, 8);
            titleVersion = Shared.Swap(BitConverter.ToUInt16(temp, 0));
            numOfContents = Shared.Swap(BitConverter.ToUInt16(temp, 2));
            bootIndex = Shared.Swap(BitConverter.ToUInt16(temp, 4));
            padding3 = Shared.Swap(BitConverter.ToUInt16(temp, 6));
            tmdFile.Position = 0xb04;

            contents = new List<TMD_Content>();

            //Read Contents
            for (int i = 0; i < numOfContents; i++)
            {
                fireDebug("   Reading Content #{0} of {1}... (Offset: 0x{2})", i + 1, numOfContents, tmdFile.Position.ToString("x8").ToUpper().ToUpper());

                TMD_Content tempContent = new TMD_Content();
                tempContent.Hash = new byte[20];

                tmdFile.Read(temp, 0, 8);
                tempContent.ContentID = Shared.Swap(BitConverter.ToUInt32(temp, 0));
                tempContent.Index = Shared.Swap(BitConverter.ToUInt16(temp, 4));
                tempContent.Type = (ContentType)Shared.Swap(BitConverter.ToUInt16(temp, 6));

                tmdFile.Read(temp, 0, 8);
                tempContent.Size = Shared.Swap(BitConverter.ToUInt64(temp, 0));

                tmdFile.Read(tempContent.Hash, 0, tempContent.Hash.Length);

                contents.Add(tempContent);
                byte[] paddingcontent = new byte[12];
                tmdFile.Read(paddingcontent, 0, 12);
            }

            fireDebug("Pasing TMD Finished...");
        }

        private string calculateNandBlocks()
        {
            int nandSizeMin = 0;
            int nandSizeMax = 0;

            for (int i = 0; i < numOfContents; i++)
            {
                nandSizeMax += (int)contents[i].Size;
                if (contents[i].Type == ContentType.Normal) nandSizeMin += (int)contents[i].Size;
            }

            int blocksMin = (int)Math.Ceiling((double)((double)nandSizeMin / (128 * 1024)));
            int blocksMax = (int)Math.Ceiling((double)((double)nandSizeMax / (128 * 1024)));

            if (blocksMin == blocksMax) return blocksMax.ToString();
            else return string.Format("{0} - {1}", blocksMin, blocksMax);
        }
        #endregion

        #region Events
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
        #endregion
    }

    public class TMD_Content
    {
        private uint contentId;
        private ushort index;
        private ushort type;
        private ulong size;
        private byte[] hash = new byte[20];

        public uint ContentID { get { return contentId; } set { contentId = value; } }
        public ushort Index { get { return index; } set { index = value; } }
        public ContentType Type { get { return (ContentType)type; } set { type = (ushort)value; } }
        public ulong Size { get { return size; } set { size = value; } }
        public byte[] Hash { get { return hash; } set { hash = value; } }
    }
}
