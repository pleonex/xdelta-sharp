//
//  DecoderWindow.cs
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
    internal class WindowReader
    {
        private const uint HardMaxWindowSize = 1u << 24;

        private Header header;
        private VcdReader vcdReader;
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
            if (window.Source.Contains(WindowFields.NotSupported))
                throw new FormatException("unrecognized window indicator bits set");

            if (window.Source.Contains(WindowFields.Source | WindowFields.Target)) {
                window.SourceSegmentLength = vcdReader.ReadUInteger();
                window.SourceSegmentOffset = vcdReader.ReadUInteger();
            }

            // Copy offset and copy length may not overflow
            if (window.SourceSegmentOffset.CheckOverflow(window.SourceSegmentLength))
                throw new FormatException("decoder copy window overflows a file offset");
       
            // Check copy window bounds
            if (window.Source.Contains(WindowFields.Target) &&
               window.SourceSegmentOffset + window.SourceSegmentLength > lastWindowOffset)
                throw new FormatException("VCD_TARGET window out of bounds");

            // Start of Delta Encoding Data
            vcdReader.ReadUInteger();  // Length of the delta encoding (following data)

            // Get the length of target window
            window.TargetWindowLength = vcdReader.ReadUInteger();
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
            if (window.CompressedFields.Contains(WindowCompressedFields.Invalid))
                throw new FormatException("unrecognized delta indicator bits set");

            // Compressed fields is only used with secondary compression
            if (window.CompressedFields != WindowCompressedFields.None &&
                header.SecondaryCompressor == SecondaryCompressor.None)
                throw new FormatException("invalid delta indicator bits set");

            // Read section lengths
            int dataLength         = vcdReader.ReadInteger();
            int instructionsLength = vcdReader.ReadInteger();
            int addressesLength    = vcdReader.ReadInteger();

            // Read checksum if so (it's in big-endian-non-integer)
            if (window.Source.Contains(WindowFields.Adler32)) {
                byte[] data = vcdReader.ReadBytes(4);
                window.Checksum = (uint)((data[0] << 24) | (data[1] << 16) |
                    (data[2] << 8) | data[3]);
            }

            // Read sections
            window.DataSection = new MemoryStream(vcdReader.ReadBytes(dataLength));
            window.InstructionsSection = new MemoryStream(vcdReader.ReadBytes(instructionsLength));
            window.AddressesSection = new MemoryStream(vcdReader.ReadBytes(addressesLength));

            return window;
        }
    }
}

