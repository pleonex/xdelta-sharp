// Copyright (c) 2019 Benito Palacios Sánchez

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace PleOps.XdeltaSharp
{
    using System;
    using System.IO;

    public class VcdReader
    {
        private const int BytesInInteger = 4;
        private const int MaxBytesInInteger = 5;
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
            if (count > int.MaxValue)
                throw new FormatException("Trying to read more than UInt32.MaxValue bytes");

            return ReadBytes((int)count);
        }

        public uint ReadInteger()
        {
            return DecodeInteger();
        }

        /// <summary>
        /// Decodes an integer of variable length.
        /// Defined in RCF-3284, the VCDIFF format uses a variable-length integer.
        /// Each byte contains 7-bits of data and the last bit indicates if there
        /// more bytes to decode.
        /// </summary>
        /// <returns>The integer.</returns>
        private uint DecodeInteger()
        {
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
