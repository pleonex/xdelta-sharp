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
namespace Pleosoft.XdeltaSharp
{
    using System;
    using System.IO;

    internal class HeaderReader
    {
        private const uint MagicStamp = 0xC4C3D6;
        private const byte SupportedVersion = 0x00;
        private static readonly System.Text.Encoding Encoding = System.Text.Encoding.ASCII;

        private VcdReader vcdReader;

        public Header Read(Stream patch)
        {
            vcdReader = new VcdReader(patch);

            // Checks the first four bytes of the patch
            CheckStamp();

            // Now let's go to the header
            return ReadHeader();
        }

        private void CheckStamp()
        {
            // Can't read as uint32 directly since a integer format is non-encoded
            // in a standard format.
            uint stamp = BitConverter.ToUInt32(vcdReader.ReadBytes(4), 0);
            if ((stamp & 0xFFFFFF) != MagicStamp)
                throw new FormatException("not a VCDIFF input");

            if ((stamp >> 24) > SupportedVersion)
                throw new FormatException("VCDIFF input version > 0 is not supported");
        }

        private Header ReadHeader()
        {
            Header header = new Header();

            HeaderFields fields = (HeaderFields)vcdReader.ReadByte();
            if ((fields & HeaderFields.All) != fields)
                throw new FormatException("unrecognized header indicator bits set");

            header.SecondaryCompressor = SecondaryCompressor.None;
            if (fields.HasFlag(HeaderFields.SecondaryCompression))
                throw new NotSupportedException("unavailable secondary compressor");

            if (fields.HasFlag(HeaderFields.CodeTable))
                throw new NotSupportedException("compressed code table not implemented");

            if (fields.HasFlag(HeaderFields.ApplicationData))
                header.ApplicationData = ReadApplicationData();

            return header;
        }

        private string ReadApplicationData()
        {
            uint length = vcdReader.ReadInteger();
            return Encoding.GetString(vcdReader.ReadBytes(length));
        }
    }
}