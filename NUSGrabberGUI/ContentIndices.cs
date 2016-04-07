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
using System.Text;

namespace libWiiSharp
{
    internal struct ContentIndices : IComparable
    {
        private int index;
        private int contentIndex;

        public int Index { get { return index; } }
        public int ContentIndex { get { return contentIndex; } }
        
        public ContentIndices(int index, int contentIndex)
        {
            this.index = index;
            this.contentIndex = contentIndex;
        }

        public int CompareTo(object obj)
        {
            if (obj is ContentIndices) return contentIndex.CompareTo(((ContentIndices)obj).contentIndex);
            else throw new ArgumentException();
        }
    }
}
