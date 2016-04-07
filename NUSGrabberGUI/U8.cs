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
    public enum U8_NodeType : ushort
    {
        File = 0x0000,
        Directory = 0x0100,
    }

    public class U8 : IDisposable
    {
        private const int dataPadding = 32;
        private Headers.HeaderType headerType = Headers.HeaderType.None;
        private object header = null;
        private U8_Header u8Header = new U8_Header();
        private U8_Node rootNode = new U8_Node();
        private List<U8_Node> u8Nodes = new List<U8_Node>();
        private List<string> stringTable = new List<string>();
        private List<byte[]> data = new List<byte[]>();
        private int iconSize = -1;
        private int bannerSize = -1;
        private int soundSize = -1;
        private bool lz77 = false;

        /// <summary>
        /// The type of the Header of the U8 file.
        /// </summary>
        public Headers.HeaderType HeaderType { get { return headerType; } }
        /// <summary>
        /// The Header of the U8 file as an object. Will be null if the file has no Header.
        /// </summary>
        public object Header { get { return header; } }
        /// <summary>
        /// The Rootnode of the U8 file.
        /// </summary>
        public U8_Node RootNode { get { return rootNode; } }
        /// <summary>
        /// The Nodes of the U8 file.
        /// </summary>
        public List<U8_Node> Nodes { get { return u8Nodes; } }
        /// <summary>
        /// The string table of the U8 file.
        /// </summary>
        public string[] StringTable { get { return stringTable.ToArray(); } }
        /// <summary>
        /// The actual data (files) in the U8 file. Will be an empty byte array for directory entries.
        /// </summary>
        public byte[][] Data { get { return data.ToArray(); } }

        /// <summary>
        /// The Number of Nodes WITHOUT the Rootnode.
        /// </summary>
        public int NumOfNodes { get { return (int)rootNode.SizeOfData - 1; } }
        /// <summary>
        /// The size of the icon.bin (if the U8 files contains an icon.bin).
        /// </summary>
        public int IconSize { get { return iconSize; } }
        /// <summary>
        /// The size of the banner.bin (if the U8 files contains an banner.bin).
        /// </summary>
        public int BannerSize { get { return bannerSize; } }
        /// <summary>
        /// The size of the sound.bin (if the U8 files contains an sound.bin).
        /// </summary>
        public int SoundSize { get { return soundSize; } }
        /// <summary>
        /// If true, the U8 file will be Lz77 compressed while saving.
        /// </summary>
        public bool Lz77Compress { get { return lz77; } set { lz77 = value; } }

        public U8()
        {
            rootNode.Type = U8_NodeType.Directory;
        }

		#region IDisposable Members
        private bool isDisposed = false;

        ~U8()
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
                header = null;
                u8Header = null;
                rootNode = null;

                u8Nodes.Clear();
                u8Nodes = null;

                stringTable.Clear();
                stringTable = null;

                data.Clear();
                data = null;
            }

            isDisposed = true;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Checks whether a file is a U8 file or not.
        /// </summary>
        /// <param name="pathToFile"></param>
        /// <returns></returns>
        public static bool IsU8(string pathToFile)
        {
            return IsU8(File.ReadAllBytes(pathToFile));
        }

        /// <summary>
        /// Checks whether a file is a U8 file or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsU8(byte[] file)
        {
            if (Lz77.IsLz77Compressed(file))
            {
                byte[] partOfFile = new byte[(file.Length > 2000) ? 2000 : file.Length];

                for (int i = 0; i < partOfFile.Length; i++)
                    partOfFile[i] = file[i];

                Lz77 l = new Lz77();
                partOfFile = l.Decompress(partOfFile);

                return IsU8(partOfFile);
            }
            else
            {
                Headers.HeaderType h = Headers.DetectHeader(file);
                return (Shared.Swap(BitConverter.ToUInt32(file, (int)h)) == 0x55AA382D);
            }
        }



        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="pathToU8"></param>
        /// <returns></returns>
        public static U8 Load(string pathToU8)
        {
            return Load(File.ReadAllBytes(pathToU8));
        }

        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="u8File"></param>
        /// <returns></returns>
        public static U8 Load(byte[] u8File)
        {
            U8 u = new U8();
            MemoryStream ms = new MemoryStream(u8File);

            try { u.parseU8(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
            return u;
        }

        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="u8File"></param>
        /// <returns></returns>
        public static U8 Load(Stream u8File)
        {
            U8 u = new U8();
            u.parseU8(u8File);
            return u;
        }

        /// <summary>
        /// Creates a U8 file.
        /// </summary>
        /// <param name="pathToDirectory"></param>
        /// <returns></returns>
        public static U8 FromDirectory(string pathToDirectory)
        {
            U8 u = new U8();
            u.createFromDir(pathToDirectory);
            return u;
        }



        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="pathToU8"></param>
        public void LoadFile(string pathToU8)
        {
            LoadFile(File.ReadAllBytes(pathToU8));
        }

        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="u8File"></param>
        public void LoadFile(byte[] u8File)
        {
            MemoryStream ms = new MemoryStream(u8File);

            try { parseU8(ms); }
            catch { ms.Dispose(); throw; }

            ms.Dispose();
        }

        /// <summary>
        /// Loads a U8 file.
        /// </summary>
        /// <param name="u8File"></param>
        public void LoadFile(Stream u8File)
        {
            parseU8(u8File);
        }

        /// <summary>
        /// Creates a U8 file.
        /// </summary>
        /// <param name="pathToDirectory"></param>
        public void CreateFromDirectory(string pathToDirectory)
        {
            createFromDir(pathToDirectory);
        }



        /// <summary>
        /// Saves the U8 file.
        /// </summary>
        /// <param name="savePath"></param>
        public void Save(string savePath)
        {
            if (File.Exists(savePath)) File.Delete(savePath);

            using (FileStream fs = new FileStream(savePath, FileMode.Create))
                writeToStream(fs);
        }

        /// <summary>
        /// Returns the U8 file as a memory stream.
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
        /// Returns the U8 file as a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return ToMemoryStream().ToArray();
        }

        /// <summary>
        /// Unpacks the U8 file to given directory.
        /// </summary>
        /// <param name="saveDir"></param>
        public void Unpack(string saveDir)
        {
            unpackToDir(saveDir);
        }

        /// <summary>
        /// Unpacks the U8 file to given directory.
        /// </summary>
        /// <param name="saveDir"></param>
        public void Extract(string saveDir)
        {
            unpackToDir(saveDir);
        }

        /// <summary>
        /// Adds an IMET Header to the U8 file.
        /// </summary>
        /// <param name="shortImet"></param>
        /// <param name="titles"></param>
        public void AddHeaderImet(bool shortImet, params string[] titles)
        {
            if (iconSize == -1)
                throw new Exception("icon.bin wasn't found!");
            else if (bannerSize == -1)
                throw new Exception("banner.bin wasn't found!");
            else if (soundSize == -1)
                throw new Exception("sound.bin wasn't found!");

            header = Headers.IMET.Create(shortImet, iconSize, bannerSize, soundSize, titles);
            headerType = (shortImet) ? Headers.HeaderType.ShortIMET : Headers.HeaderType.IMET;
        }

        /// <summary>
        /// Adds an IMD5 Header to the U8 file.
        /// </summary>
        public void AddHeaderImd5()
        {
            headerType = Headers.HeaderType.IMD5;
        }

        /// <summary>
        /// Replaces the file with the given index.
        /// </summary>
        /// <param name="fileIndex"></param>
        /// <param name="pathToNewFile"></param>
        public void ReplaceFile(int fileIndex, string pathToNewFile, bool changeFileName = false)
        {
            if (u8Nodes[fileIndex].Type == U8_NodeType.Directory)
                throw new Exception("You can't replace a directory with a file!");

            data[fileIndex] = File.ReadAllBytes(pathToNewFile);
            if (changeFileName) stringTable[fileIndex] = Path.GetFileName(pathToNewFile);

            if (stringTable[fileIndex].ToLower() == "icon.bin")
                iconSize = getRealSize(File.ReadAllBytes(pathToNewFile));
            else if (stringTable[fileIndex].ToLower() == "banner.bin")
                bannerSize = getRealSize(File.ReadAllBytes(pathToNewFile));
            else if (stringTable[fileIndex].ToLower() == "sound.bin")
                soundSize = getRealSize(File.ReadAllBytes(pathToNewFile));
        }

        /// <summary>
        /// Replaces the file with the given index.
        /// </summary>
        /// <param name="fileIndex"></param>
        /// <param name="newData"></param>
        public void ReplaceFile(int fileIndex, byte[] newData)
        {
            if (u8Nodes[fileIndex].Type == U8_NodeType.Directory)
                throw new Exception("You can't replace a directory with a file!");

            data[fileIndex] = newData;

            if (stringTable[fileIndex].ToLower() == "icon.bin")
                iconSize = getRealSize(newData);
            else if (stringTable[fileIndex].ToLower() == "banner.bin")
                bannerSize = getRealSize(newData);
            else if (stringTable[fileIndex].ToLower() == "sound.bin")
                soundSize = getRealSize(newData);
        }

        /// <summary>
        /// Returns the index of the directory or file with the given name.
        /// If no matching Node is found, -1 will be returned.
        /// </summary>
        /// <param name="fileOrDirName"></param>
        /// <returns></returns>
        public int GetNodeIndex(string fileOrDirName)
        {
            for (int i = 0; i < u8Nodes.Count; i++)
                if (stringTable[i].ToLower() == fileOrDirName.ToLower()) return i;

            return -1;
        }

        /// <summary>
        /// Changes the name of a node.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newName"></param>
        public void RenameNode(int index, string newName)
        {
            stringTable[index] = newName;
        }

        /// <summary>
        /// Changes the name of a node.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void RenameNode(string oldName, string newName)
        {
            stringTable[GetNodeIndex(oldName)] = newName;
        }

        /// <summary>
        /// Adds a directory to the U8 file.
        /// Path must be like this: "/arc/timg/newFolder".
        /// </summary>
        /// <param name="path"></param>
        public void AddDirectory(string path)
        {
            addEntry(path, new byte[0]);
        }

        /// <summary>
        /// Adds a file to the U8 file.
        /// Path must be like this: "/arc/timg/newFile.tpl".
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        public void AddFile(string path, byte[] data)
        {
            addEntry(path, data);
        }

        /// <summary>
        /// Removes a directory from the U8 file.
        /// If the directory contains files/dirs, they will also be deleted.
        /// Path must be like this: "/arc/timg/folderToDelete".
        /// </summary>
        /// <param name="path"></param>
        public void RemoveDirectory(string path)
        {
            removeEntry(path);
        }

        /// <summary>
        /// Removes a file from the U8 file.
        /// Path must be like this: "/arc/timg/fileToDelete.tpl".
        /// </summary>
        /// <param name="path"></param>
        public void RemoveFile(string path)
        {
            removeEntry(path);
        }
        #endregion

        #region Private Functions
        private void writeToStream(Stream writeStream)
        {
            fireDebug("Writing U8 File...");

            //Update Rootnode
            fireDebug("   Updating Rootnode...");
            rootNode.SizeOfData = (uint)u8Nodes.Count + 1;

            MemoryStream u8Stream = new MemoryStream();

            //Write Stringtable
            u8Stream.Seek(u8Header.OffsetToRootNode + ((u8Nodes.Count + 1) * 12), SeekOrigin.Begin);

            fireDebug("   Writing String Table... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper());
            u8Stream.WriteByte(0x00);

            int stringTablePosition = (int)u8Stream.Position - 1;
            for (int i = 0; i < u8Nodes.Count; i++)
            {
                fireDebug("    -> Entry #{1} of {2}: \"{3}\"... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper(), i + 1, u8Nodes.Count, stringTable[i]);

                u8Nodes[i].OffsetToName = (ushort)(u8Stream.Position - stringTablePosition);
                byte[] stringBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(stringTable[i]);

                u8Stream.Write(stringBytes, 0, stringBytes.Length);
                u8Stream.WriteByte(0x00);
            }

            u8Header.HeaderSize = (uint)(u8Stream.Position - u8Header.OffsetToRootNode);
            u8Header.OffsetToData = 0;

            //Write Data
            for (int i = 0; i < u8Nodes.Count; i++)
            {
                fireProgress((i + 1) * 100 / u8Nodes.Count);

                if (u8Nodes[i].Type == U8_NodeType.File)
                {
                    u8Stream.Seek(Shared.AddPadding((int)u8Stream.Position, dataPadding), SeekOrigin.Begin);

                    fireDebug("   Writing Data #{1} of {2}... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper(), i + 1, u8Nodes.Count);

                    if (u8Header.OffsetToData == 0) u8Header.OffsetToData = (uint)u8Stream.Position;
                    u8Nodes[i].OffsetToData = (uint)u8Stream.Position;
                    u8Nodes[i].SizeOfData = (uint)data[i].Length;

                    u8Stream.Write(data[i], 0, data[i].Length);
                }
                else fireDebug("   Node #{0} of {1} is a Directory...", i + 1, u8Nodes.Count);
            }

            //Pad End to 16 bytes
            while (u8Stream.Position % 16 != 0)
                u8Stream.WriteByte(0x00);

            //Write Header + Nodes
            u8Stream.Seek(0, SeekOrigin.Begin);

            fireDebug("   Writing Header... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper());
            u8Header.Write(u8Stream);

            fireDebug("   Writing Rootnode... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper());
            rootNode.Write(u8Stream);

            for (int i = 0; i < u8Nodes.Count; i++)
            {
                fireDebug("   Writing Node Entry #{1} of {2}... (Offset: 0x{0})", u8Stream.Position.ToString("x8").ToUpper(), i + 1, u8Nodes.Count);
                u8Nodes[i].Write(u8Stream);
            }

            byte[] u8Array = u8Stream.ToArray();
            u8Stream.Dispose();

            if (lz77)
            {
                fireDebug("   Lz77 Compressing U8 File...");

                Lz77 l = new Lz77();
                u8Array = l.Compress(u8Array);
            }

            //Write File Header
            if (headerType == Headers.HeaderType.IMD5)
            {
                fireDebug("   Adding IMD5 Header...");

                writeStream.Seek(0, SeekOrigin.Begin);
                Headers.IMD5 h = Headers.IMD5.Create(u8Array);
                h.Write(writeStream);
            }
            else if (headerType == Headers.HeaderType.IMET || headerType == Headers.HeaderType.ShortIMET)
            {
                fireDebug("   Adding IMET Header...");

                ((Headers.IMET)header).IconSize = (uint)iconSize;
                ((Headers.IMET)header).BannerSize = (uint)bannerSize;
                ((Headers.IMET)header).SoundSize = (uint)soundSize;

                writeStream.Seek(0, SeekOrigin.Begin);
                ((Headers.IMET)header).Write(writeStream);
            }

            writeStream.Write(u8Array, 0, u8Array.Length);

            fireDebug("Writing U8 File Finished...");
        }

        private void unpackToDir(string saveDir)
        {
            fireDebug("Unpacking U8 File to: {0}", saveDir);

            if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);

            string[] dirs = new string[u8Nodes.Count];
            dirs[0] = saveDir;
            int[] dirCount = new int[u8Nodes.Count];
            int dirIndex = 0;

            for (int i = 0; i < u8Nodes.Count; i++)
            {
                fireDebug("   Unpacking Entry #{0} of {1}", i + 1, u8Nodes.Count);
                fireProgress((i + 1) * 100 / u8Nodes.Count);

                switch (u8Nodes[i].Type)
                {
                    case U8_NodeType.Directory:
                        fireDebug("    -> Directory: \"{0}\"", stringTable[i]);

                        if (dirs[dirIndex][dirs[dirIndex].Length - 1] != Path.DirectorySeparatorChar) { dirs[dirIndex] = dirs[dirIndex] + Path.DirectorySeparatorChar; }
                        Directory.CreateDirectory(dirs[dirIndex] + stringTable[i]);
                        dirs[dirIndex + 1] = dirs[dirIndex] + stringTable[i];
                        dirIndex++;
                        dirCount[dirIndex] = (int)u8Nodes[i].SizeOfData;
                        break;
                    default:
                        fireDebug("    -> File: \"{0}\"", stringTable[i]);
                        fireDebug("    -> Size: {0} bytes", data[i].Length);

                        using (FileStream fs = new FileStream(dirs[dirIndex] + Path.DirectorySeparatorChar + stringTable[i], FileMode.Create))
                            fs.Write(data[i], 0, data[i].Length);
                        break;
                }

                while (dirIndex > 0 && dirCount[dirIndex] == i + 2)
                { dirIndex--; }
            }

            fireDebug("Unpacking U8 File Finished");
        }

        private void parseU8(Stream u8File)
        {
            fireDebug("Pasing U8 File...");

            u8Header = new U8_Header();
            rootNode = new U8_Node();
            u8Nodes = new List<U8_Node>();
            stringTable = new List<string>();
            data = new List<byte[]>();

            fireDebug("   Detecting Header...");
            headerType = Headers.DetectHeader(u8File);
            Headers.HeaderType tempType = headerType;

            fireDebug("    -> {0}", headerType.ToString());

            if (headerType == Headers.HeaderType.IMD5)
            {
                fireDebug("   Reading IMD5 Header...");
                header = Headers.IMD5.Load(u8File);

                byte[] file = new byte[u8File.Length];
                u8File.Read(file, 0, file.Length);

                MD5 m = MD5.Create();
                byte[] newHash = m.ComputeHash(file, (int)headerType, (int)u8File.Length - (int)headerType);
                m.Clear();

                if (!Shared.CompareByteArrays(newHash, ((Headers.IMD5)header).Hash))
                {
                    fireDebug(@"/!\ /!\ /!\ Hashes do not match /!\ /!\ /!\");
                    fireWarning(string.Format("Hashes of IMD5 header and file do not match! The content might be corrupted!"));
                }
            }
            else if (headerType == Headers.HeaderType.IMET || headerType == Headers.HeaderType.ShortIMET)
            {
                fireDebug("   Reading IMET Header...");
                header = Headers.IMET.Load(u8File);

                if (!((Headers.IMET)header).HashesMatch)
                {
                    fireDebug(@"/!\ /!\ /!\ Hashes do not match /!\ /!\ /!\");
                    fireWarning(string.Format("The hash stored in the IMET header doesn't match the headers hash! The header and/or file might be corrupted!"));
                }
            }

            fireDebug("   Checking for Lz77 Compression...");
            if (Lz77.IsLz77Compressed(u8File))
            {
                fireDebug("    -> Lz77 Compression Found...");
                fireDebug("   Decompressing U8 Data...");

                Lz77 l = new Lz77();
                Stream decompressedFile = l.Decompress(u8File);

                tempType = Headers.DetectHeader(decompressedFile);
                u8File = decompressedFile;

                lz77 = true;
            }

            u8File.Seek((int)tempType, SeekOrigin.Begin);
            byte[] temp = new byte[4];

            //Read U8 Header
            fireDebug("   Reading U8 Header: Magic... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
            u8File.Read(temp, 0, 4);
            if (Shared.Swap(BitConverter.ToUInt32(temp, 0)) != u8Header.U8Magic)
            { fireDebug("    -> Invalid Magic!"); throw new Exception("U8 Header: Invalid Magic!"); }

            fireDebug("   Reading U8 Header: Offset to Rootnode... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
            u8File.Read(temp, 0, 4);
            if (Shared.Swap(BitConverter.ToUInt32(temp, 0)) != u8Header.OffsetToRootNode)
            { fireDebug("    -> Invalid Offset to Rootnode"); throw new Exception("U8 Header: Invalid Offset to Rootnode!"); }

            fireDebug("   Reading U8 Header: Header Size... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
            u8File.Read(temp, 0, 4);
            u8Header.HeaderSize = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            fireDebug("   Reading U8 Header: Offset to Data... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
            u8File.Read(temp, 0, 4);
            u8Header.OffsetToData = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            u8File.Seek(16, SeekOrigin.Current);

            //Read Rootnode
            fireDebug("   Reading Rootnode... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
            u8File.Read(temp, 0, 4);
            rootNode.Type = (U8_NodeType)Shared.Swap(BitConverter.ToUInt16(temp, 0));
            rootNode.OffsetToName = Shared.Swap(BitConverter.ToUInt16(temp, 2));

            u8File.Read(temp, 0, 4);
            rootNode.OffsetToData = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            u8File.Read(temp, 0, 4);
            rootNode.SizeOfData = Shared.Swap(BitConverter.ToUInt32(temp, 0));

            int stringTablePosition = (int)((int)tempType + u8Header.OffsetToRootNode + rootNode.SizeOfData * 12);
            int nodePosition = (int)u8File.Position;

            //Read Nodes
            for (int i = 0; i < rootNode.SizeOfData - 1; i++)
            {
                fireDebug("   Reading Node #{1} of {2}... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper(), i + 1, rootNode.SizeOfData - 1);
                fireProgress((int)((i + 1) * 100 / (rootNode.SizeOfData - 1)));

                U8_Node tempNode = new U8_Node();
                string tempName = string.Empty;
                byte[] tempData = new byte[0];

                //Read Node Entry
                u8File.Seek(nodePosition, SeekOrigin.Begin);

                fireDebug("    -> Reading Node Entry... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
                u8File.Read(temp, 0, 4);
                tempNode.Type = (U8_NodeType)Shared.Swap(BitConverter.ToUInt16(temp, 0));
                tempNode.OffsetToName = Shared.Swap(BitConverter.ToUInt16(temp, 2));

                u8File.Read(temp, 0, 4);
                tempNode.OffsetToData = Shared.Swap(BitConverter.ToUInt32(temp, 0));

                u8File.Read(temp, 0, 4);
                tempNode.SizeOfData = Shared.Swap(BitConverter.ToUInt32(temp, 0));

                nodePosition = (int)u8File.Position;

                fireDebug("        -> {0}", tempNode.Type.ToString());

                //Read Node Name
                u8File.Seek(stringTablePosition + tempNode.OffsetToName, SeekOrigin.Begin);

                fireDebug("    -> Reading Node Name... (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());
                for (;;)
                {
                    char tempChar = (char)u8File.ReadByte();
                    if (tempChar == 0x00) break;

                    tempName += tempChar;

                    if (tempName.Length > 255) break;
                }

                fireDebug("        -> {0}", tempName);

                //Read Node Data
                if (tempNode.Type == U8_NodeType.File)
                {
                    u8File.Seek((int)tempType + tempNode.OffsetToData, SeekOrigin.Begin);

                    fireDebug("    -> Reading Node Data (Offset: 0x{0})", u8File.Position.ToString("x8").ToUpper());

                    tempData = new byte[tempNode.SizeOfData];
                    u8File.Read(tempData, 0, tempData.Length);
                }

                if (tempName.ToLower() == "icon.bin") iconSize = getRealSize(tempData);
                else if (tempName.ToLower() == "banner.bin") bannerSize = getRealSize(tempData);
                else if (tempName.ToLower() == "sound.bin") soundSize = getRealSize(tempData);

                u8Nodes.Add(tempNode);
                stringTable.Add(tempName);
                data.Add(tempData);
            }

            fireDebug("Pasing U8 File Finished...");
        }

        private void createFromDir(string path)
        {
            fireDebug("Creating U8 File from: {0}", path);

            if (path[path.Length - 1] != Path.DirectorySeparatorChar) path += Path.DirectorySeparatorChar;

            fireDebug("   Collecting Content...");
            string[] dirEntries = getDirContent(path, true);
            int offsetToName = 1;
            int offsetToData = 0;

            fireDebug("   Creating U8 Header...");
            u8Header = new U8_Header();
            rootNode = new U8_Node();
            u8Nodes = new List<U8_Node>();
            stringTable = new List<string>();
            data = new List<byte[]>();

            //Create Rootnode
            fireDebug("   Creating Rootnode...");
            rootNode.Type = U8_NodeType.Directory;
            rootNode.OffsetToName = 0;
            rootNode.OffsetToData = 0;
            rootNode.SizeOfData = (uint)dirEntries.Length + 1;

            //Create Nodes
            for (int i = 0; i < dirEntries.Length; i++)
            {
                fireDebug("   Creating Node #{0} of {1}", i + 1, dirEntries.Length);
                fireProgress((i + 1) * 100 / dirEntries.Length);

                U8_Node tempNode = new U8_Node();
                byte[] tempData = new byte[0];

                string tempDirEntry = dirEntries[i].Remove(0, path.Length - 1);

                if (Directory.Exists(dirEntries[i])) //It's a dir
                {
                    fireDebug("    -> Directory");

                    tempNode.Type = U8_NodeType.Directory;
                    tempNode.OffsetToData = (uint)Shared.CountCharsInString(tempDirEntry, Path.DirectorySeparatorChar); //Recursion

                    int size = u8Nodes.Count + 2;
                    for (int j = 0; j < dirEntries.Length; j++)
                        if (dirEntries[j].Contains(dirEntries[i] + System.IO.Path.DirectorySeparatorChar.ToString())) size++;

                    tempNode.SizeOfData = (uint)size;
                }
                else //It's a file
                {
                    fireDebug("    -> File");
                    fireDebug("    -> Reading File Data...");

                    tempData = File.ReadAllBytes(dirEntries[i]);
                    tempNode.Type = U8_NodeType.File;
                    tempNode.OffsetToData = (uint)offsetToData;
                    tempNode.SizeOfData = (uint)tempData.Length;

                    offsetToData += Shared.AddPadding(offsetToData + tempData.Length, dataPadding);
                }

                tempNode.OffsetToName = (ushort)offsetToName;
                offsetToName += (Path.GetFileName(dirEntries[i])).Length + 1;

                fireDebug("    -> Reading Name...");
                string tempName = Path.GetFileName(dirEntries[i]);

                if (tempName.ToLower() == "icon.bin") iconSize = getRealSize(tempData);
                else if (tempName.ToLower() == "banner.bin") bannerSize = getRealSize(tempData);
                else if (tempName.ToLower() == "sound.bin") soundSize = getRealSize(tempData);

                u8Nodes.Add(tempNode);
                stringTable.Add(tempName);
                data.Add(tempData);
            }

            //Update U8 Header
            fireDebug("   Updating U8 Header...");
            u8Header.HeaderSize = (uint)(((u8Nodes.Count + 1) * 12) + offsetToName);
            u8Header.OffsetToData = (uint)Shared.AddPadding((int)u8Header.OffsetToRootNode + (int)u8Header.HeaderSize, dataPadding);
            
            //Update dataOffsets
            fireDebug("   Calculating Data Offsets...");

            for (int i = 0; i < u8Nodes.Count; i++)
            {
                fireDebug("    -> Node #{0} of {1}...", i + 1, u8Nodes.Count);

                int tempOffset = (int)u8Nodes[i].OffsetToData;
                u8Nodes[i].OffsetToData = (uint)(u8Header.OffsetToData + tempOffset);
            }

            fireDebug("Creating U8 File Finished...");
        }

        private string[] getDirContent(string dir, bool root)
        {
            string[] files = Directory.GetFiles(dir);
            string[] dirs = Directory.GetDirectories(dir);
            string all = "";

            if (!root)
                all += dir + "\n";

            for (int i = 0; i < files.Length; i++)
                all += files[i] + "\n";

            foreach (string thisDir in dirs)
            {
                string[] temp = getDirContent(thisDir, false);

                foreach (string thisTemp in temp)
                    all += thisTemp + "\n";
            }

            return all.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private int getRealSize(byte[] data)
        {
            if (data[0] == 'I' && data[1] == 'M' && data[2] == 'D' && data[3] == '5')
                if (data[0x20] == 'L' && data[0x21] == 'Z' && data[0x22] == '7' && data[0x23] == '7')
                    return (BitConverter.ToInt32(data, 0x24)) >> 8;
                else 
                    return data.Length - 32;

            return data.Length;
        }

        private void addEntry(string nodePath, byte[] fileData)
        {
            //Parse path
            if (nodePath.StartsWith("/")) nodePath = nodePath.Remove(0, 1);
            string[] path = nodePath.Split('/'); //Last entry is the filename

            int nodeIndex = -1;
            int maxIndex = (u8Nodes.Count > 0) ? u8Nodes.Count - 1 : 0;
            int currentIndex = 0;
            List<int> pathIndices = new List<int>();

            for (int i = 0; i < path.Length - 1; i++)
            {
                for (int j = currentIndex; j <= maxIndex; j++)
                {
                    if (stringTable[j].ToLower() == path[i].ToLower())
                    {
                        if (i == path.Length - 2) nodeIndex = j;

                        maxIndex = (int)u8Nodes[j].SizeOfData - 1;
                        currentIndex = j + 1;
                        pathIndices.Add(j);

                        break;
                    }

                    if (j == maxIndex - 1) throw new Exception("Path wasn't found!");
                }
            }

            //Get last entry in current dir
            int lastEntry;

            if (nodeIndex > -1) lastEntry = (int)u8Nodes[nodeIndex].SizeOfData - 2;
            else lastEntry = (rootNode.SizeOfData > 1) ? (int)rootNode.SizeOfData - 2 : -1;

            //Create and insert node + data
            U8_Node tempNode = new U8_Node();
            tempNode.Type = (fileData.Length == 0) ? U8_NodeType.Directory : U8_NodeType.File;
            tempNode.SizeOfData = (uint)((fileData.Length == 0) ? lastEntry + 2 : fileData.Length);
            tempNode.OffsetToData = (uint)((fileData.Length == 0) ? Shared.CountCharsInString(nodePath, '/') : 0);

            stringTable.Insert(lastEntry + 1, path[path.Length - 1]);
            u8Nodes.Insert(lastEntry + 1, tempNode);
            data.Insert(lastEntry + 1, fileData);

            //Update rootnode and path sizes (+1)
            rootNode.SizeOfData += 1;

            foreach (int index in pathIndices)
                if (u8Nodes[index].Type == U8_NodeType.Directory)
                    u8Nodes[index].SizeOfData += 1;

            for (int i = lastEntry + 1; i < u8Nodes.Count; i++)
                if (u8Nodes[i].Type == U8_NodeType.Directory)
                    u8Nodes[i].SizeOfData += 1;
        }

        private void removeEntry(string nodePath)
        {
            //Parse path
            if (nodePath.StartsWith("/")) nodePath = nodePath.Remove(0, 1);
            string[] path = nodePath.Split('/'); //Last entry is the filename

            int nodeIndex = -1;
            int maxIndex = u8Nodes.Count - 1;
            int currentIndex = 0;
            List<int> pathIndices = new List<int>();

            for (int i = 0; i < path.Length; i++)
            {
                for (int j = currentIndex; j < maxIndex; j++)
                {
                    if (stringTable[j].ToLower() == path[i].ToLower())
                    {
                        if (i == path.Length - 1) nodeIndex = j;
                        else pathIndices.Add(j);

                        maxIndex = (int)u8Nodes[j].SizeOfData - 1;
                        currentIndex = j + 1;

                        break;
                    }

                    if (j == maxIndex - 1) throw new Exception("Path wasn't found!");
                }
            }

            //Remove Node (and subnodes if node is dir)
            int removed = 0;

            if (u8Nodes[nodeIndex].Type == U8_NodeType.Directory)
            {
                for (int i = (int)u8Nodes[nodeIndex].SizeOfData - 2; i >= nodeIndex; i--)
                {
                    stringTable.RemoveAt(i);
                    u8Nodes.RemoveAt(i);
                    data.RemoveAt(i);

                    removed++;
                }
            }
            else
            {
                stringTable.RemoveAt(nodeIndex);
                u8Nodes.RemoveAt(nodeIndex);
                data.RemoveAt(nodeIndex);

                removed++;
            }

            //Update rootnode and path sizes
            rootNode.SizeOfData -= (uint)removed;

            foreach (int index in pathIndices)
                if (u8Nodes[index].Type == U8_NodeType.Directory)
                    u8Nodes[index].SizeOfData -= (uint)removed;

            for (int i = nodeIndex + 1; i < u8Nodes.Count; i++)
                if (u8Nodes[i].Type == U8_NodeType.Directory)
                    u8Nodes[i].SizeOfData -= (uint)removed;
        }
        #endregion

        #region Events
        /// <summary>
        /// Fires the Progress of various operations
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs> Progress;
        /// <summary>
        /// Fires warnings (e.g. when hashes do not match)
        /// </summary>
        public event EventHandler<MessageEventArgs> Warning;
        /// <summary>
        /// Fires debugging messages. You may write them into a log file or log textbox.
        /// </summary>
        public event EventHandler<MessageEventArgs> Debug;


        private void fireWarning(string warningMessage)
        {
            EventHandler<MessageEventArgs> warning = Warning;
            if (warning != null)
                warning(new object(), new MessageEventArgs(warningMessage));
        }

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

    public class U8_Header
    {
        private uint u8Magic = 0x55AA382D;
        private uint offsetToRootNode = 0x20;
        private uint headerSize;
        private uint offsetToData;
        private byte[] padding = new byte[16];

        public uint U8Magic { get { return u8Magic; } }
        public uint OffsetToRootNode { get { return offsetToRootNode; } }
        public uint HeaderSize { get { return headerSize; } set { headerSize = value; } }
        public uint OffsetToData { get { return offsetToData; } set { offsetToData = value; } }
        public byte[] Padding { get { return padding; } }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(u8Magic)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToRootNode)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(headerSize)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToData)), 0, 4);
            writeStream.Write(padding, 0, 16);
        }
    }

    public class U8_Node
    {
        private ushort type;
        private ushort offsetToName;
        private uint offsetToData;
        private uint sizeOfData;

        public U8_NodeType Type { get { return (U8_NodeType)type; } set { type = (ushort)value; } }
        public ushort OffsetToName { get { return offsetToName; } set { offsetToName = value; } }
        public uint OffsetToData { get { return offsetToData; } set { offsetToData = value; } }
        public uint SizeOfData { get { return sizeOfData; } set { sizeOfData = value; } }

        public void Write(Stream writeStream)
        {
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(type)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToName)), 0, 2);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(offsetToData)), 0, 4);
            writeStream.Write(BitConverter.GetBytes(Shared.Swap(sizeOfData)), 0, 4);
        }
    }
}
