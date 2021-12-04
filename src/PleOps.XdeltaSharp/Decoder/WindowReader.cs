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
namespace PleOps.XdeltaSharp.Decoder
{
    using System;
    using System.IO;
    using PleOps.XdeltaSharp.Vcdiff;

    internal class WindowReader
    {
        private const uint HardMaxWindowSize = 1u << 24;

        private readonly Header header;
        private readonly VcdReader vcdReader;
        private uint lastWindowOffset;
        private uint lastWindowLength;

        public WindowReader(Stream patch, Header header)
        {
            this.header = header;
            vcdReader = new VcdReader(patch);
            lastWindowLength = 0;
            lastWindowOffset = 0;
        }

        public Window Read()
        {
            CheckReadOverflow();
            return ReadWindow();
        }

        private void CheckReadOverflow()
        {
            if (lastWindowOffset.CheckOverflow(lastWindowLength))
                throw new FormatException("decoder file offset overflow");

            // Updated at the initialization to avoid throwing an overflow error
            // decoding exactly 4 GB files.
            lastWindowOffset += lastWindowLength;
        }

        private Window ReadWindow()
        {
            Window window = new Window();

            // Get window indicator
            window.Source = (WindowFields)vcdReader.ReadByte();
            if ((window.Source & WindowFields.All) != window.Source)
                throw new FormatException("unrecognized window indicator bits set");

            if (window.Source.HasFlag(WindowFields.Source) || window.Source.HasFlag(WindowFields.Target)) {
                window.SourceSegmentLength = vcdReader.ReadInteger();
                window.SourceSegmentOffset = vcdReader.ReadInteger();
            }

            // Copy offset and copy length may not overflow
            if (window.SourceSegmentOffset.CheckOverflow(window.SourceSegmentLength))
                throw new FormatException("decoder copy window overflows a file offset");

            // Check copy window bounds
            if (window.Source.HasFlag(WindowFields.Target) &&
               window.SourceSegmentOffset + window.SourceSegmentLength > lastWindowOffset)
                throw new FormatException("VCD_TARGET window out of bounds");

            // Start of Delta Encoding Data
            vcdReader.ReadInteger();  // Length of the delta encoding (following data)

            // Get the length of target window
            window.TargetWindowLength = vcdReader.ReadInteger();
            lastWindowLength = window.TargetWindowLength;

            // Set the maximum decoder position, beyond which we should not
            // decode any data. This may not exceed the size of a UInt32.
            if (window.SourceSegmentLength.CheckOverflow(window.TargetWindowLength))
                throw new FormatException("decoder target window overflows a UInt32");

            // Check for malicious files
            if (window.TargetWindowLength > HardMaxWindowSize)
                throw new FormatException("Hard window size exceeded");

            // Get compressed / delta fields
            window.CompressedFields = (WindowCompressedFields)vcdReader.ReadByte();
            if ((window.CompressedFields & WindowCompressedFields.All) != window.CompressedFields)
                throw new FormatException("unrecognized delta indicator bits set");

            // Compressed fields is only used with secondary compression
            if (window.CompressedFields != WindowCompressedFields.None &&
                header.SecondaryCompressor == SecondaryCompressor.None)
                throw new FormatException("invalid delta indicator bits set");

            // Read section lengths
            uint dataLength = vcdReader.ReadInteger();
            uint instructionsLength = vcdReader.ReadInteger();
            uint addressesLength = vcdReader.ReadInteger();

            // Read checksum if so (it's in big-endian-non-integer)
            if (window.Source.HasFlag(WindowFields.Adler32)) {
                byte[] checksum = vcdReader.ReadBytes(4);
                window.Checksum = (uint)((checksum[0] << 24) | (checksum[1] << 16) |
                    (checksum[2] << 8) | checksum[3]);
            }

            // Read sections
            MemoryStream data = new MemoryStream(vcdReader.ReadBytes(dataLength));
            MemoryStream instructions = new MemoryStream(vcdReader.ReadBytes(instructionsLength));
            MemoryStream addresses = new MemoryStream(vcdReader.ReadBytes(addressesLength));
            window.Data = new VcdReader(data);
            window.Instructions = new VcdReader(instructions);
            window.Addresses = new VcdReader(addresses);

            return window;
        }
    }
}
