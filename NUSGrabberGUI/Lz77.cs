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
 
//Lz77 (de-)compression algorithm based on gbalzss by Andre Perrot (Thanks!)

using System;
using System.IO;

namespace libWiiSharp
{
    public class Lz77
    {
        private static uint lz77Magic = 0x4c5a3737;
        private const int N = 4096;
        private const int F = 18;
        private const int threshold = 2;
        private int[] leftSon = new int[N + 1];
        private int[] rightSon = new int[N + 257];
        private int[] dad = new int[N + 1];
        private ushort[] textBuffer = new ushort[N + 17];
        private int matchPosition = 0, matchLength = 0;

        /// <summary>
        /// Lz77 Magic.
        /// </summary>
        public static uint Lz77Magic { get { return lz77Magic; } }

        #region Public Functions
        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(string file)
        {
            return IsLz77Compressed(File.ReadAllBytes(file));
        }

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(byte[] file)
        {
            Headers.HeaderType h = Headers.DetectHeader(file);
            return (Shared.Swap(BitConverter.ToUInt32(file, (int)h)) == lz77Magic) ;
        }

        /// <summary>
        /// Checks whether a file is Lz77 compressed or not.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsLz77Compressed(Stream file)
        {
            Headers.HeaderType h = Headers.DetectHeader(file);
            byte[] temp = new byte[4];
            file.Seek((long)h, SeekOrigin.Begin);
            file.Read(temp, 0, temp.Length);
            return (Shared.Swap(BitConverter.ToUInt32(temp, 0)) == lz77Magic);
        }



        /// <summary>
        /// Compresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Compress(string inFile, string outFile)
        {
            Stream compressedFile;
            
            using (FileStream fsIn = new FileStream(inFile, FileMode.Open))
                compressedFile = compress(fsIn);

            byte[] output = new byte[compressedFile.Length];
            compressedFile.Read(output, 0, output.Length);

            if (File.Exists(outFile)) File.Delete(outFile);

            using (FileStream fs = new FileStream(outFile, FileMode.Create))
                fs.Write(output, 0, output.Length);
        }

        /// <summary>
        /// Compresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Compress(byte[] file)
        {
            return ((MemoryStream)compress(new MemoryStream(file))).ToArray();
        }

        /// <summary>
        /// Compresses the stream using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public Stream Compress(Stream file)
        {
            return compress(file);
        }

        /// <summary>
        /// Decompresses a file using the Lz77 algorithm.
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public void Decompress(string inFile, string outFile)
        {
            Stream compressedFile;

            using (FileStream fsIn = new FileStream(inFile, FileMode.Open))
                compressedFile = decompress(fsIn);

            byte[] output = new byte[compressedFile.Length];
            compressedFile.Read(output, 0, output.Length);

            if (File.Exists(outFile)) File.Delete(outFile);

            using (FileStream fs = new FileStream(outFile, FileMode.Create))
                fs.Write(output, 0, output.Length);
        }

        /// <summary>
        /// Decompresses the byte array using the Lz77 algorithm.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] Decompress(byte[] file)
        {
            return ((MemoryStream)decompress(new MemoryStream(file))).ToArray();
        }

        public Stream Decompress(Stream file)
        {
            return decompress(file);
        }
        #endregion

        #region Private Functions
        private Stream decompress(Stream inFile)
        {
            if (!Lz77.IsLz77Compressed(inFile)) return inFile;
            inFile.Seek(0, SeekOrigin.Begin);

            int i, j, k, r, c, z;
            uint flags, decompressedSize, currentSize = 0;

            Headers.HeaderType h = Headers.DetectHeader(inFile);

            byte[] temp = new byte[8];
            inFile.Seek((int)h, SeekOrigin.Begin);
            inFile.Read(temp, 0, 8);

            if (Shared.Swap(BitConverter.ToUInt32(temp, 0)) != lz77Magic)
            { inFile.Dispose(); throw new Exception("Invaild Magic!"); }
            if (temp[4] != 0x10)
            { inFile.Dispose(); throw new Exception("Unsupported Compression Type!"); }

            decompressedSize = (BitConverter.ToUInt32(temp, 4)) >> 8;

            for (i = 0; i < N - F; i++) textBuffer[i] = 0xdf;
            r = N - F; flags = 7; z = 7;

            MemoryStream outFile = new MemoryStream();
            while (true)
            {
                flags <<= 1;
                z++;

                if (z == 8)
                {
                    if ((c = inFile.ReadByte()) == -1) break;

                    flags = (uint)c;
                    z = 0;
                }

                if ((flags & 0x80) == 0)
                {
                    if ((c = inFile.ReadByte()) == inFile.Length - 1) break;
                    if (currentSize < decompressedSize) outFile.WriteByte((byte)c);

                    textBuffer[r++] = (byte)c;
                    r &= (N - 1);
                    currentSize++;
                }
                else
                {
                    if ((i = inFile.ReadByte()) == -1) break;
                    if ((j = inFile.ReadByte()) == -1) break;

                    j = j | ((i << 8) & 0xf00);
                    i = ((i >> 4) & 0x0f) + threshold;
                    for (k = 0; k <= i; k++)
                    {
                        c = textBuffer[(r - j - 1) & (N - 1)];
                        if (currentSize < decompressedSize) outFile.WriteByte((byte)c); textBuffer[r++] = (byte)c; r &= (N - 1); currentSize++;
                    }
                }
            }

            return outFile;
        }

        private Stream compress(Stream inFile)
        {
            if (Lz77.IsLz77Compressed(inFile)) return inFile;
            inFile.Seek(0, SeekOrigin.Begin);

            int textSize = 0;
            int codeSize = 0;

            int i, c, r, s, length, lastMatchLength, codeBufferPointer, mask;
            int[] codeBuffer = new int[17];

            uint fileSize = ((Convert.ToUInt32(inFile.Length)) << 8) + 0x10;
            MemoryStream outFile = new MemoryStream();

            outFile.Write(BitConverter.GetBytes(Shared.Swap(lz77Magic)), 0, 4);
            outFile.Write(BitConverter.GetBytes(fileSize), 0, 4);

            InitTree();
            codeBuffer[0] = 0;
            codeBufferPointer = 1;
            mask = 0x80;
            s = 0;
            r = N - F;

            for (i = s; i < r; i++) textBuffer[i] = 0xffff;

            for (length = 0; length < F && (c = (int)inFile.ReadByte()) != -1; length++)
                textBuffer[r + length] = (ushort)c;

            if ((textSize = length) == 0) return inFile;

            for (i = 1; i <= F; i++) InsertNode(r - i);
            InsertNode(r);

            do
            {
                if (matchLength > length) matchLength = length;

                if (matchLength <= threshold)
                {
                    matchLength = 1;
                    codeBuffer[codeBufferPointer++] = textBuffer[r];
                }
                else
                {
                    codeBuffer[0] |= mask;

                    codeBuffer[codeBufferPointer++] = (char)
                        (((r - matchPosition - 1) >> 8) & 0x0f) |
                        ((matchLength - (threshold + 1)) << 4);

                    codeBuffer[codeBufferPointer++] = (char)((r - matchPosition - 1) & 0xff);
                }

                if ((mask >>= 1) == 0)
                {
                    for (i = 0; i < codeBufferPointer; i++)
                        outFile.WriteByte((byte)codeBuffer[i]);

                    codeSize += codeBufferPointer;
                    codeBuffer[0] = 0; codeBufferPointer = 1;
                    mask = 0x80;
                }

                lastMatchLength = matchLength;
                for (i = 0; i < lastMatchLength && (c = (int)inFile.ReadByte()) != -1; i++)
                {
                    DeleteNode(s);

                    textBuffer[s] = (ushort)c;
                    if (s < F - 1) textBuffer[s + N] = (ushort)c;
                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);

                    InsertNode(r);
                }

                while (i++ < lastMatchLength)
                {
                    DeleteNode(s);

                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
                    if (--length != 0) InsertNode(r);
                }
            } while (length > 0);


            if (codeBufferPointer > 1)
            {
                for (i = 0; i < codeBufferPointer; i++) outFile.WriteByte((byte)codeBuffer[i]);
                codeSize += codeBufferPointer;
            }

            if (codeSize % 4 != 0)
                for (i = 0; i < 4 - (codeSize % 4); i++)
                    outFile.WriteByte(0x00);

            return outFile;
        }

        private void InitTree()
        {
            int i;
            for (i = N + 1; i <= N + 256; i++) rightSon[i] = N;
            for (i = 0; i < N; i++) dad[i] = N;
        }

        private void InsertNode(int r)
        {
            int i, p, cmp;
            cmp = 1;
            p = N + 1 + (textBuffer[r] == 0xffff ? 0 : (int)textBuffer[r]);
            rightSon[r] = leftSon[r] = N; matchLength = 0;

            for (; ; )
            {
                if (cmp >= 0)
                {
                    if (rightSon[p] != N) p = rightSon[p];
                    else { rightSon[p] = r; dad[r] = p; return; }
                }
                else
                {
                    if (leftSon[p] != N) p = leftSon[p];
                    else { leftSon[p] = r; dad[r] = p; return; }
                }

                for (i = 1; i < F; i++)
                    if ((cmp = textBuffer[r + i] - textBuffer[p + i]) != 0) break;

                if (i > matchLength)
                {
                    matchPosition = p;
                    if ((matchLength = i) >= F) break;
                }
            }

            dad[r] = dad[p]; leftSon[r] = leftSon[p]; rightSon[r] = rightSon[p];
            dad[leftSon[p]] = r; dad[rightSon[p]] = r;

            if (rightSon[dad[p]] == p) rightSon[dad[p]] = r;
            else leftSon[dad[p]] = r;

            dad[p] = N;
        }

        private void DeleteNode(int p)
        {
            int q;

            if (dad[p] == N) return;

            if (rightSon[p] == N) q = leftSon[p];
            else if (leftSon[p] == N) q = rightSon[p];
            else
            {
                q = leftSon[p];

                if (rightSon[q] != N)
                {
                    do { q = rightSon[q]; } while (rightSon[q] != N);
                    rightSon[dad[q]] = leftSon[q]; dad[leftSon[q]] = dad[q];
                    leftSon[q] = leftSon[p]; dad[leftSon[p]] = q;
                }

                rightSon[q] = rightSon[p]; dad[rightSon[p]] = q;
            }

            dad[q] = dad[p];

            if (rightSon[dad[p]] == p) rightSon[dad[p]] = q;
            else leftSon[dad[p]] = q;

            dad[p] = N;
        }
        #endregion
    }
}
