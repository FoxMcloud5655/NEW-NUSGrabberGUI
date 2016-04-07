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
    public class CertificateChain : IDisposable
    {
        private const string certCaHash = "5B7D3EE28706AD8DA2CBD5A6B75C15D0F9B6F318";
        private const string certCpHash = "6824D6DA4C25184F0D6DAF6EDB9C0FC57522A41C";
        private const string certXsHash = "09787045037121477824BC6A3E5E076156573F8A";
        private SHA1 sha = SHA1.Create();
        private bool[] certsComplete = new bool[3];

        private byte[] certCa = new byte[0x400];
        private byte[] certCp = new byte[0x300];
        private byte[] certXs = new byte[0x300];

        /// <summary>
        /// If false, the Certificate Chain is not complete (i.e. at least one certificate is missing).
        /// </summary>
        public bool CertsComplete { get { return (certsComplete[0] && certsComplete[1] && certsComplete[2]); } }

        #region IDisposable Members
        private bool isDisposed = false;

        ~CertificateChain()
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

                certsComplete = null;
                certCa = null;
                certCp = null;
                certXs = null;
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="pathToCert"></param>
        /// <returns></returns>
        public static CertificateChain Load(string pathToCert)
        {
            return Load(File.ReadAllBytes(pathToCert));
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="certFile"></param>
        /// <returns></returns>
        public static CertificateChain Load(byte[] certFile)
        {
            CertificateChain c = new CertificateChain();
            MemoryStream ms = new MemoryStream(certFile);

            try { c.parseCert(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
            return c;
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static CertificateChain Load(Stream cert)
        {
            CertificateChain c = new CertificateChain();
            c.parseCert(cert);
            return c;
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(string pathToTik, string pathToTmd)
        {
            return FromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(string pathToTik, byte[] tmdFile)
        {
            return FromTikTmd(File.ReadAllBytes(pathToTik), tmdFile);
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tikFile"></param>
        /// <param name="tmdFile"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(byte[] tikFile, byte[] tmdFile)
        {
            CertificateChain c = new CertificateChain();
            MemoryStream ms = new MemoryStream(tikFile);

            try { c.grabFromTik(ms); }
            catch { ms.Dispose(); throw; }

            ms = new MemoryStream(tmdFile);

            try { c.grabFromTmd(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();

            if (!c.CertsComplete) throw new Exception("Couldn't locate all certs!");

            return c;
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        /// <returns></returns>
        public static CertificateChain FromTikTmd(Stream tik, Stream tmd)
        {
            CertificateChain c = new CertificateChain();
            c.grabFromTik(tik);
            c.grabFromTmd(tmd);
            return c;
        }



        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="pathToCert"></param>
        public void LoadFile(string pathToCert)
        {
            LoadFile(File.ReadAllBytes(pathToCert));
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="certFile"></param>
        public void LoadFile(byte[] certFile)
        {
            MemoryStream ms = new MemoryStream(certFile);

            try { parseCert(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
        }

        /// <summary>
        /// Loads a cert file.
        /// </summary>
        /// <param name="cert"></param>
        public void LoadFile(Stream cert)
        {
            parseCert(cert);
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="pathToTmd"></param>
        /// <returns></returns>
        public void LoadFromTikTmd(string pathToTik, string pathToTmd)
        {
            LoadFromTikTmd(File.ReadAllBytes(pathToTik), File.ReadAllBytes(pathToTmd));
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="pathToTik"></param>
        /// <param name="tmdFile"></param>
        /// <returns></returns>
        public void LoadFromTikTmd(string pathToTik, byte[] tmdFile)
        {
            LoadFromTikTmd(File.ReadAllBytes(pathToTik), tmdFile);
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tikFile"></param>
        /// <param name="tmdFile"></param>
        public void LoadFromTikTmd(byte[] tikFile, byte[] tmdFile)
        {
            MemoryStream ms = new MemoryStream(tikFile);

            try { grabFromTik(ms); }
            catch { ms.Dispose(); throw; }

            ms = new MemoryStream(tmdFile);

            try { grabFromTmd(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();

            if (!CertsComplete) throw new Exception("Couldn't locate all certs!");
        }

        /// <summary>
        /// Grabs certificates from Ticket and Tmd.
        /// Ticket and Tmd must contain certs! (They do when they're downloaded from NUS!)
        /// </summary>
        /// <param name="tik"></param>
        /// <param name="tmd"></param>
        public void LoadFromTikTmd(Stream tik, Stream tmd)
        {
            grabFromTik(tik);
            grabFromTmd(tmd);
        }



        /// <summary>
        /// Saves the Certificate Chain.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            if (File.Exists(savePath)) File.Delete(savePath);

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
                writeToStream(fs);
        }

        /// <summary>
        /// Returns the Certificate Chain as a memory stream.
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
        /// Returns the Certificate Chain as a byte array.
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
        #endregion

        #region Private Functions
        private void writeToStream(Stream writeStream)
        {
            fireDebug("Writing Certificate Chain...");

            if (!CertsComplete)
            { fireDebug("   Certificate Chain incomplete..."); throw new Exception("At least one certificate is missing!"); }

            writeStream.Seek(0, SeekOrigin.Begin);

            fireDebug("   Writing Certificate CA... (Offset: 0x{0})", writeStream.Position.ToString("x8"));
            writeStream.Write(certCa, 0, certCa.Length);

            fireDebug("   Writing Certificate CP... (Offset: 0x{0})", writeStream.Position.ToString("x8"));
            writeStream.Write(certCp, 0, certCp.Length);

            fireDebug("   Writing Certificate XS... (Offset: 0x{0})", writeStream.Position.ToString("x8"));
            writeStream.Write(certXs, 0, certXs.Length);

            fireDebug("Writing Certificate Chain Finished...");
        }

        private void parseCert(Stream certFile)
        {
            fireDebug("Parsing Certificate Chain...");
            int off = 0;

            for (int i = 0; i < 3; i++)
            {
                fireDebug("   Scanning at Offset 0x{0}:", off.ToString("x8"));

                try
                {
                    certFile.Seek(off, SeekOrigin.Begin);
                    byte[] temp = new byte[0x400];

                    certFile.Read(temp, 0, temp.Length);

                    fireDebug("   Checking for Certificate CA...");
                    if (isCertCa(temp) && !certsComplete[1])
                    { fireDebug("   Certificate CA detected..."); certCa = temp; certsComplete[1] = true; off += 0x400; continue; }

                    fireDebug("   Checking for Certificate CP...");
                    if (isCertCp(temp) && !certsComplete[2])
                    { fireDebug("   Certificate CP detected..."); Array.Resize(ref temp, 0x300); certCp = temp; certsComplete[2] = true; off += 0x300; continue; }

                    fireDebug("   Checking for Certificate XS...");
                    if (isCertXs(temp) && !certsComplete[0])
                    { fireDebug("   Certificate XS detected..."); Array.Resize(ref temp, 0x300); certXs = temp; certsComplete[0] = true; off += 0x300; continue; }
                }
                catch (Exception ex) { fireDebug("Error: {0}", ex.Message); }

                off += 0x300;
            }

            if (!CertsComplete)
            { fireDebug("   Couldn't locate all Certificates..."); throw new Exception("Couldn't locate all certs!"); }

            fireDebug("Parsing Certificate Chain Finished...");
        }

        private void grabFromTik(Stream tik)
        {
            fireDebug("Scanning Ticket for Certificates...");
            int off = 676;

            for (int i = 0; i < 3; i++)
            {
                fireDebug("   Scanning at Offset 0x{0}:", off.ToString("x8"));

                try
                {
                    tik.Seek(off, SeekOrigin.Begin);
                    byte[] temp = new byte[0x400];

                    tik.Read(temp, 0, temp.Length);

                    fireDebug("   Checking for Certificate CA...");
                    if (isCertCa(temp) && !certsComplete[1])
                    { fireDebug("   Certificate CA detected..."); certCa = temp; certsComplete[1] = true; off += 0x400; continue; }

                    fireDebug("   Checking for Certificate CP...");
                    if (isCertCp(temp) && !certsComplete[2])
                    { fireDebug("   Certificate CP detected..."); Array.Resize(ref temp, 0x300); certCp = temp; certsComplete[2] = true; off += 0x300; continue; }

                    fireDebug("   Checking for Certificate XS...");
                    if (isCertXs(temp) && !certsComplete[0])
                    { fireDebug("   Certificate XS detected..."); Array.Resize(ref temp, 0x300); certXs = temp; certsComplete[0] = true; off += 0x300; continue; }
                }
                catch { }

                off += 0x300;
            }

            fireDebug("Scanning Ticket for Certificates Finished...");
        }

        private void grabFromTmd(Stream tmd)
        {
            fireDebug("Scanning TMD for Certificates...");

            byte[] temp = new byte[2];
            tmd.Seek(478, SeekOrigin.Begin);
            tmd.Read(temp, 0, 2);

            int numContents = Shared.Swap(BitConverter.ToUInt16(temp, 0));
            int off = 484 + numContents * 36;

            for (int i = 0; i < 3; i++)
            {
                fireDebug("   Scanning at Offset 0x{0}:", off.ToString("x8"));

                try
                {
                    tmd.Seek(off, SeekOrigin.Begin);
                    temp = new byte[0x400];

                    tmd.Read(temp, 0, temp.Length);

                    fireDebug("   Checking for Certificate CA...");
                    if (isCertCa(temp) && !certsComplete[1])
                    { fireDebug("   Certificate CA detected..."); certCa = temp; certsComplete[1] = true; off += 0x400; continue; }

                    fireDebug("   Checking for Certificate CP...");
                    if (isCertCp(temp) && !certsComplete[2])
                    { fireDebug("   Certificate CP detected..."); Array.Resize(ref temp, 0x300); certCp = temp; certsComplete[2] = true; off += 0x300; continue; }

                    fireDebug("   Checking for Certificate XS...");
                    if (isCertXs(temp) && !certsComplete[0])
                    { fireDebug("   Certificate XS detected..."); Array.Resize(ref temp, 0x300); certXs = temp; certsComplete[0] = true; off += 0x300; continue; }
                }
                catch { }

                off += 0x300;
            }

            fireDebug("Scanning TMD for Certificates Finished...");
        }

        private bool isCertXs(byte[] part)
        {
            if (part.Length < 0x300) return false;
            else if (part.Length > 0x300) Array.Resize(ref part, 0x300);

            if (part[0x184] == 'X' && part[0x185] == 'S')
            {
                byte[] newHash = sha.ComputeHash(part);
                byte[] oldHash = Shared.HexStringToByteArray(certXsHash);

                if (Shared.CompareByteArrays(newHash, oldHash)) return true;
            }

            return false;
        }

        private bool isCertCa(byte[] part)
        {
            if (part.Length < 0x400) return false;
            else if (part.Length > 0x400) Array.Resize(ref part, 0x400);

            if (part[0x284] == 'C' && part[0x285] == 'A')
            {
                byte[] newHash = sha.ComputeHash(part);
                byte[] oldHash = Shared.HexStringToByteArray(certCaHash);

                if (Shared.CompareByteArrays(newHash, oldHash)) return true;
            }

            return false;
        }

        private bool isCertCp(byte[] part)
        {
            if (part.Length < 0x300) return false;
            else if (part.Length > 0x300) Array.Resize(ref part, 0x300);

            if (part[0x184] == 'C' && part[0x185] == 'P')
            {
                byte[] newHash = sha.ComputeHash(part);
                byte[] oldHash = Shared.HexStringToByteArray(certCpHash);

                if (Shared.CompareByteArrays(newHash, oldHash)) return true;
            }

            return false;
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
