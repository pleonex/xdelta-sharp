//
//  VcdiffReader.cs
//
//  Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
//  Copyright (c) 2015 Benito Palacios Sánchez
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
#define USE_32_BITS_INTEGERS

using System;
using System.IO;

namespace Xdelta
{
    public class VcdReader
    {
        private BinaryReader binReader;

        public VcdReader(Stream stream)
        {
            binReader = new BinaryReader(stream);
        }

        public byte ReadByte()
        {
            return binReader.ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return binReader.ReadBytes(count);
        }

        #if USE_32_BITS_INTEGERS
        public int ReadInteger()
        {
            return (int)DecodeInteger(4);
        }

        public uint ReadUInteger()
        {
            return (uint)DecodeInteger(4);
        }
        #else
        public long ReadInteger()
        {
            return (long)DecodeInteger(8);
        }

        public ulong ReadUInteger()
        {
            return DecodeInteger(8);
        }
        #endif

        /// <summary>
        /// Decodes an integer of variable length.
        /// Defined in RCF-3284, the VCDIFF format uses a variable-length integer.
        /// Each byte contains 7-bits of data and the last bit indicates if there
        /// more bytes to decode.
        /// </summary>
        /// <returns>The integer.</returns>
        /// <param name="maxBytes">Max bytes of the output format.</param>
        private ulong DecodeInteger(int maxBytes)
        {
            // There is no way to constraint the generic to a number so I am
            // using the biggest value possible and a casting at the end.
            // http://stackoverflow.com/questions/32664/is-there-a-constraint-that-restricts-my-generic-method-to-numeric-types
            ulong value = 0;
            ulong mask = ~((1ul << (maxBytes * 8)) - 1u);
            int bitsRead = 0;

            byte data;
            do {
                // If it had already the opportinuty to read the maximum value
                // This prevent read past the end of the stream
                if (bitsRead > maxBytes * 8)
                    throw new FormatException("overflow in decode_integer");

                data = binReader.ReadByte();

                bitsRead += 7;  // Bits of data read
                value = (value << 7) | (data & 0x7Fu);  // Data only in the first 7 bits

                // Check the maximum value expected (expect for long)
                if (maxBytes != 8 && (value & mask) != 0)
                    throw new FormatException("overflow in decode_integer");
            } while (data >> 7 == 1);   // Continue bit in the last bit

            return value;
        }
    }
}

