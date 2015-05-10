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
		private const int MaxBytesInInteger = 5;
		#else
		private const int BytesInInteger = 8;
		private const int MaxBytesInInteger = 10;
		#endif
		private const int MaxBitsInInteger = BytesInInteger * 8;

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
            return DecodeInteger();
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
        private uint DecodeInteger()
        {
            #if !USE_32_BITS_INTEGERS
			throw new NotImplementedException("Not implemented 64-bits integer reading");
			#endif
			uint value = 0;
			byte bytesRead = 0;
			int data;

			do {
				data = BaseStream.ReadByte();
				if (data == -1)
					throw new EndOfStreamException();

				// Before set the last bits, check that there is no invalid bits set in the first iteration
				// or trying to read more bytes than expected
				if (++bytesRead == MaxBytesInInteger &&
					((value >> (MaxBitsInInteger - 7) != 0) || ((data & 0x80) != 0)))
					throw new FormatException("overflow in decode_integer");

				value = (value << 7) | ((byte)data & 0x7Fu);
			} while ((data & 0x80) != 0 && bytesRead < MaxBytesInInteger);

			return value;
        }
    }
}

