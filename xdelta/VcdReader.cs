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
using System;
using System.IO;

namespace Xdelta
{
    public class VcdReader : BinaryReader
    {
        public VcdReader(Stream stream)
            : base(stream)
        {
        }

		public override string ReadString()
		{
			// Explicity non-supported since internally it is being used ReadByte that has been overwritten
			throw new NotSupportedException("VCDIFF Standard does not support strings");
		}

		public override sbyte ReadSByte()
        {
            return (sbyte)DecodeInteger(1); 
        }

        public override byte ReadByte()
        {
			// I have checked that internally in BinaryReader it is not used this method.
			// It's used only for Internal7BitEncodedInt, but this other one is only used
			// for strings that are not allowed.
            return (byte)DecodeInteger(1);
        }

        public override short ReadInt16()
        {
            return (short)DecodeInteger(2);
        }

        public override ushort ReadUInt16()
        {
            return (ushort)DecodeInteger(2);
        }

        public override int ReadInt32()
        {
            return (int)DecodeInteger(4);
        }

        public override long ReadInt64()
        {
            return (long)DecodeInteger(8);
        }

        public override uint ReadUInt32()
        {
            return (uint)DecodeInteger(4);
        }

        public override ulong ReadUInt64()
        {
            return DecodeInteger(8);
        }

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

                data = base.ReadByte();

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

