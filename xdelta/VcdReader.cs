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
		#if USE_32_BITS_INTEGERS
		private const int BytesInInteger = 4;
		#else
		private const int BytesInInteger = 8;
		#endif
		private const ulong Mask = ~((1ul << (BytesInInteger * 8)) - 1u);

        public VcdReader(Stream stream)
        {
            BaseStream = stream;
        }

        public Stream BaseStream {
            get;
            private set;
        }

        public bool Eof {
            get { return BaseStream.Position >= BaseStream.Length; }
        }

        public byte ReadByte()
        {
			int data = BaseStream.ReadByte();
			if (data == -1)
				throw new EndOfStreamException();
			
			return (byte)data;
        }

        public byte[] ReadBytes(int count)
        {
			byte[] data = new byte[count];
			BaseStream.Read(data, 0, data.Length);
			return data;
        }

        public byte[] ReadBytes(uint count)
        {
            if (count > Int32.MaxValue)
                throw new FormatException("Trying to read more than UInt32.MaxValue bytes");

            return ReadBytes((int)count);
        }

        #if USE_32_BITS_INTEGERS
        public uint ReadInteger()
        {
            return (uint)DecodeInteger();
        }
        #else
        public ulong ReadInteger()
        {
            return (ulong)DecodeInteger();
        }
        #endif

        /// <summary>
        /// Decodes an integer of variable length.
        /// Defined in RCF-3284, the VCDIFF format uses a variable-length integer.
        /// Each byte contains 7-bits of data and the last bit indicates if there
        /// more bytes to decode.
        /// </summary>
        /// <returns>The integer.</returns>
        private ulong DecodeInteger()
        {
            // There is no way to constraint the generic to a number so I am
            // using the biggest value possible and a casting at the end.
            // http://stackoverflow.com/questions/32664/is-there-a-constraint-that-restricts-my-generic-method-to-numeric-types
            ulong value = 0;
            int bitsRead = 0;

            int data;
            do {
                // If it had already the opportinuty to read the maximum value
                // This prevent read past the end of the stream
				if (bitsRead > BytesInInteger * 8)
                    throw new FormatException("overflow in decode_integer");

				data = BaseStream.ReadByte();
				if (data == -1)
					throw new EndOfStreamException();

                bitsRead += 7;  // Bits of data read
				value = (value << 7) | ((byte)data & 0x7Fu);  // Data only in the first 7 bits

                // Check the maximum value expected (expect for long)
				if (BytesInInteger != 8 && (value & Mask) != 0)
                    throw new FormatException("overflow in decode_integer");
            } while (data >> 7 == 1);   // Continue bit in the last bit

            return value;
        }
    }
}

