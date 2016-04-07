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
using System.IO;
using System.Security.Cryptography;

namespace libWiiSharp
{
    public enum CommonKeyType : byte
    {
        Standard = 0x00,
        Korean = 0x01,
    }

    public class Ticket : IDisposable
    {
        private byte newKeyIndex = (byte)CommonKeyType.Standard;
        private byte[] decryptedTitleKey = new byte[16];
        private bool fakeSign = false;
        private bool titleKeyChanged = false;
        private byte[] newEncryptedTitleKey = new byte[0];
        private bool reDecrypt = false;

        private uint signatureExponent = 0x00010001;
        private byte[] signature = new byte[256];
        private byte[] padding = new byte[60];
        private byte[] issuer = new byte[64];
        private byte[] unknown = new byte[63];
        private byte[] encryptedTitleKey = new byte[16];
        private byte unknown2;
        private ulong ticketId;
        private uint consoleId;
        private ulong titleId;
        private ushort unknown3 = 0xFFFF;
        private ushort numOfDlc;
        private ulong unknown4;
        private byte padding2;
        private byte commonKeyIndex = (byte)CommonKeyType.Standard;
        private byte[] unknown5 = new byte[48];
        private byte[] unknown6 = new byte[32]; //0xFF
        private ushort padding3;
        private uint enableTimeLimit;
        private uint timeLimit;
        private byte[] padding4 = new byte[88];

        private bool dsitik = false;

        /// <summary>
        /// The Title Key the WADs content is encrypted with.
        /// </summary>
        public byte[] TitleKey { get { return decryptedTitleKey; } set { decryptedTitleKey = value; titleKeyChanged = true; reDecrypt = false; } }
        /// <summary>
        /// Defines which Common Key is used (Standard / Korean).
        /// </summary>
        public CommonKeyType CommonKeyIndex { get { return (CommonKeyType)newKeyIndex; } set { newKeyIndex = (byte)value; } }
        /// <summary>
        /// The Ticket ID.
        /// </summary>
        public ulong TicketID { get { return ticketId; } set { ticketId = value; } }
        /// <summary>
        /// The Console ID.
        /// </summary>
        public uint ConsoleID { get { return consoleId; } set { consoleId = value; } }
        /// <summary>
        /// The Title ID.
        /// </summary>
        public ulong TitleID { get { return titleId; } set { titleId = value; if (reDecrypt) reDecryptTitleKey(); } }
        /// <summary>
        /// Number of DLC.
        /// </summary>
        public ushort NumOfDLC { get { return numOfDlc; } set { numOfDlc = value; } }
        /// <summary>
        /// If true, the Ticket will be fakesigned while saving.
        /// </summary>
        public bool FakeSign { get { return fakeSign; } set { fakeSign = value; } }
        /// <summary>
        /// True if the Title Key was changed.
        /// </summary>
        public bool TitleKeyChanged { get { return titleKeyChanged; } }

        /// <summary>
        /// If true, the Ticket will utilize the DSi CommonKey.
        /// </summary>
        public bool DSiTicket { get { return dsitik; } set { dsitik = value; decryptTitleKey(); } }

		#region IDisposable Members
        private bool isDisposed = false;

        ~Ticket()
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
                decryptedTitleKey = null;
                newEncryptedTitleKey = null;
                signature = null;
                padding = null;
                issuer = null;
                unknown = null;
                encryptedTitleKey = null;
                unknown5 = null;
                unknown6 = null;
                padding4 = null;
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="pathToTicket"></param>
        /// <returns></returns>
        public static Ticket Load(string pathToTicket)
        {
            return Load(File.ReadAllBytes(pathToTicket));
        }

        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static Ticket Load(byte[] ticket)
        {
            Ticket tik = new Ticket();
            MemoryStream ms = new MemoryStream(ticket);

            try { tik.parseTicket(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
            return tik;
        }

        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static Ticket Load(Stream ticket)
        {
            Ticket tik = new Ticket();
            tik.parseTicket(ticket);
            return tik;
        }



        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="pathToTicket"></param>
        public void LoadFile(string pathToTicket)
        {
            LoadFile(File.ReadAllBytes(pathToTicket));
        }

        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="ticket"></param>
        public void LoadFile(byte[] ticket)
        {
            MemoryStream ms = new MemoryStream(ticket);

            try { parseTicket(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
        }
        
        /// <summary>
        /// Loads a tik file.
        /// </summary>
        /// <param name="ticket"></param>
        public void LoadFile(Stream ticket)
        {
            parseTicket(ticket);
        }



        /// <summary>
        /// Saves the Ticket.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            Save(savePath, false);
        }

        /// <summary>
        /// Saves the Ticket. If fakeSign is true, the Ticket will be fakesigned.
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
        /// Returns the Ticket as a memory stream.
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToMemoryStream()
        {
            return ToMemoryStream(false);
        }

        /// <summary>
        /// Returns the Ticket as a memory stream. If fakeSign is true, the Ticket will be fakesigned.
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
        /// Returns the Ticket as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ToByteArray(false);
        }

        /// <summary>
        /// Returns the Ticket as a byte array. If fakeSign is true, the Ticket will be fakesigned.
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
        /// This will set a new encrypted Title Key (i.e. the one that you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void SetTitleKey(string newTitleKey)
        {
            SetTitleKey(newTitleKey.ToCharArray());
        }

        /// <summary>
        /// This will set a new encrypted Title Key (i.e. the one that you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void SetTitleKey(char[] newTitleKey)
        {
            if (newTitleKey.Length != 16)
                throw new Exception("The title key must be 16 characters long!");

            for (int i = 0; i < 16; i++)
                encryptedTitleKey[i] = (byte)newTitleKey[i];

            decryptTitleKey();
            titleKeyChanged = true;

            reDecrypt = true;
            newEncryptedTitleKey = encryptedTitleKey;
        }

        /// <summary>
        /// This will set a new encrypted Title Key (i.e. the one that you can "read" in the Ticket).
        /// </summary>
        /// <param name="newTitleKey"></param>
        public void SetTitleKey(byte[] newTitleKey)
        {
            if (newTitleKey.Length != 16)
                throw new Exception("The title key must be 16 characters long!");

            encryptedTitleKey = newTitleKey;
            decryptTitleKey();
            titleKeyChanged = true;

            reDecrypt = true;
            newEncryptedTitleKey = newTitleKey;
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
        #endregion

        #region Private Functions
        private void writeToStream(Stream writeStream)
        {
            fireDebug("Writing Ticket...");

            fireDebug("   Encrypting Title Key...");
            encryptTitleKey();
            fireDebug("    -> Decrypted Title Key: {0}", Shared.ByteArrayToString(decryptedTitleKey));
            fireDebug("    -> Encrypted Title Key: {0}", Shared.ByteArrayToString(encryptedTitleKey));

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

            fireDebug("   Writing Unknown... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(unknown, 0, unknown.Length);

            fireDebug("   Writing Title Key... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(encryptedTitleKey, 0, encryptedTitleKey.Length);

            fireDebug("   Writing Unknown2... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(unknown2);

            fireDebug("   Writing Ticket ID... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(ticketId)), 0, 8);

            fireDebug("   Writing Console ID... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(consoleId)), 0, 4);

            fireDebug("   Writing Title ID... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(titleId)), 0, 8);

            fireDebug("   Writing Unknwon3... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(unknown3)), 0, 2);

            fireDebug("   Writing NumOfDLC... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(numOfDlc)), 0, 2);

            fireDebug("   Writing Unknwon4... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(unknown4)), 0, 8);

            fireDebug("   Writing Padding2... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(padding2);

            fireDebug("   Writing Common Key Index... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.WriteByte(commonKeyIndex);

            fireDebug("   Writing Unknown5... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(unknown5, 0, unknown5.Length);

            fireDebug("   Writing Unknown6... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(unknown6, 0, unknown6.Length);

            fireDebug("   Writing Padding3... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(padding3)), 0, 2);

            fireDebug("   Writing Enable Time Limit... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(enableTimeLimit)), 0, 4);

            fireDebug("   Writing Time Limit... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(BitConverter.GetBytes(Shared.Swap(timeLimit)), 0, 4);

            fireDebug("   Writing Padding4... (Offset: 0x{0})", ms.Position.ToString("x8").ToUpper());
            ms.Write(padding4, 0, padding4.Length);

            byte[] tik = ms.ToArray();
            ms.Dispose();

            //fake Sign
            if (fakeSign)
            {
                fireDebug("   Fakesigning Ticket...");

                byte[] hash = new byte[20];
                SHA1 s = SHA1.Create();

                for (ushort i = 0; i < 0xFFFF; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(i);
                    tik[498] = bytes[1]; tik[499] = bytes[0];

                    hash = s.ComputeHash(tik);
                    if (hash[0] == 0x00)
                    { fireDebug("   -> Signed ({0})", i); break; } //Win! It's signed...

                    if (i == 0xFFFF - 1)
                    { fireDebug("    -> Signing Failed..."); throw new Exception("Fakesigning failed..."); }
                }

                s.Clear();
            }

            writeStream.Seek(0, SeekOrigin.Begin);
            writeStream.Write(tik, 0, tik.Length);

            fireDebug("Writing Ticket Finished...");
        }

        private void parseTicket(Stream ticketFile)
        {
            fireDebug("Parsing Ticket...");

            ticketFile.Seek(0, SeekOrigin.Begin);
            byte[] temp = new byte[8];

            fireDebug("   Reading Signature Exponent... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 4);
            signatureExponent = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading Signature... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(signature, 0, signature.Length);

            fireDebug("   Reading Padding... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(padding, 0, padding.Length);

            fireDebug("   Reading Issuer... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(issuer, 0, issuer.Length);

            fireDebug("   Reading Unknown... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown, 0, unknown.Length);

            fireDebug("   Reading Title Key... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(encryptedTitleKey, 0, encryptedTitleKey.Length);

            fireDebug("   Reading Unknown2... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            unknown2 = (byte)ticketFile.ReadByte();

            fireDebug("   Reading Ticket ID.. (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 8);
            ticketId = Shared.Swap(BitConverter.ToUInt64(temp, 0));

            fireDebug("   Reading Console ID... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 4);
            consoleId = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading Title ID... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 8);
            titleId = Shared.Swap(BitConverter.ToUInt64(temp, 0));

            fireDebug("   Reading Unknown3... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading NumOfDLC... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 4);
            unknown3 = Shared.Swap(BitConverter.ToUInt16(temp, 0));
            numOfDlc = Shared.Swap(BitConverter.ToUInt16(temp, 2));

            fireDebug("   Reading Unknown4... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 8);
            unknown4 = Shared.Swap(BitConverter.ToUInt64(temp, 0));

            fireDebug("   Reading Padding2... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            padding2 = (byte)ticketFile.ReadByte();

            fireDebug("   Reading Common Key Index... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            commonKeyIndex = (byte)ticketFile.ReadByte();

            newKeyIndex = commonKeyIndex;

            fireDebug("   Reading Unknown5... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown5, 0, unknown5.Length);

            fireDebug("   Reading Unknown6... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(unknown6, 0, unknown6.Length);

            fireDebug("   Reading Padding3... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 2);
            padding3 = Shared.Swap(BitConverter.ToUInt16(temp, 0));

            fireDebug("   Reading Enable Time Limit... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            fireDebug("   Reading Time Limit... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(temp, 0, 8);
            enableTimeLimit = Shared.Swap(BitConverter.ToUInt32(temp, 0));
            timeLimit = Shared.Swap(BitConverter.ToUInt32(temp, 4));

            fireDebug("   Reading Padding4... (Offset: 0x{0})", ticketFile.Position.ToString("x8").ToUpper());
            ticketFile.Read(padding4, 0, padding4.Length);

            fireDebug("   Decrypting Title Key...");
            decryptTitleKey();
            fireDebug("    -> Encrypted Title Key: {0}", Shared.ByteArrayToString(encryptedTitleKey));
            fireDebug("    -> Decrypted Title Key: {0}", Shared.ByteArrayToString(decryptedTitleKey));

            fireDebug("Parsing Ticket Finished...");
        }

        private void decryptTitleKey()
        {
            byte[] ckey = CommonKey.GetStandardKey();
            byte[] iv = BitConverter.GetBytes(Shared.Swap(titleId));
            Array.Resize(ref iv, 16);

            RijndaelManaged rm = new RijndaelManaged();
            rm.Mode = CipherMode.CBC;
            rm.Padding = PaddingMode.None;
            rm.KeySize = 128;
            rm.BlockSize = 128;
            rm.Key = ckey;
            rm.IV = iv;

            ICryptoTransform decryptor = rm.CreateDecryptor();

            MemoryStream ms = new MemoryStream(encryptedTitleKey);
            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            cs.Read(decryptedTitleKey, 0, decryptedTitleKey.Length);

            cs.Dispose();
            ms.Dispose();
            decryptor.Dispose();
            rm.Clear();
        }

        private void encryptTitleKey()
        {
            commonKeyIndex = newKeyIndex;
            byte[] ckey = CommonKey.GetStandardKey();
            byte[] iv = BitConverter.GetBytes(Shared.Swap(titleId));
            Array.Resize(ref iv, 16);

            RijndaelManaged rm = new RijndaelManaged();
            rm.Mode = CipherMode.CBC;
            rm.Padding = PaddingMode.None;
            rm.KeySize = 128;
            rm.BlockSize = 128;
            rm.Key = ckey;
            rm.IV = iv;

            ICryptoTransform encryptor = rm.CreateEncryptor();

            MemoryStream ms = new MemoryStream(decryptedTitleKey);
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Read);

            cs.Read(encryptedTitleKey, 0, encryptedTitleKey.Length);

            cs.Dispose();
            ms.Dispose();
            encryptor.Dispose();
            rm.Clear();
        }

        private void reDecryptTitleKey()
        {
            encryptedTitleKey = newEncryptedTitleKey;
            decryptTitleKey();
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
}
