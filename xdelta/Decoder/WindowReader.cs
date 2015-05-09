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

        private VcdReader patchReader;
        private uint lastWindowOffset;
        private uint lastWindowLength;

        public WindowReader(Stream patch)
        {
            patchReader  = new VcdReader(patch);
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
            window.Fields = (WindowFields)patchReader.ReadByte();
            if ((window.Fields & WindowFields.NotSupported) != 0)
                throw new FormatException("unrecognized window indicator bits set");

            if ((window.Fields & (WindowFields.Source | WindowFields.Target)) != 0) {
                window.CopyLength = patchReader.ReadUInt32();  // Copy window length
                window.CopyOffset = patchReader.ReadUInt32();  // Copy window offset
            }

            // Copy offset and copy length may not overflow
            if (window.CopyOffset.CheckOverflow(window.CopyLength))
                throw new FormatException("decoder copy window overflows a file offset");
       
            // Check copy window bounds
            if ((window.Fields & WindowFields.Target) != 0 &&
               window.CopyOffset + window.CopyLength > lastWindowOffset)
                throw new FormatException("VCD_TARGET window out of bounds");

            // Set encoded data address and length
            window.EncodedPosition = window.CopyLength;
            window.EncodedLength   = patchReader.ReadUInt32();  // Length of the delta encoding

            // Get the length of target window
            window.Length = patchReader.ReadUInt32();
            lastWindowLength = window.Length;

            // Set the maximum decoder position, beyond which we should not
            // decode any data.  This is the maximum value for dec_position.
            //  This may not exceed the size of a UInt32
            if (window.CopyLength.CheckOverflow(window.Length))
                throw new FormatException("decoder target window overflows a UInt32");

            // Check for malicious files
            if (window.Length > HardMaxWindowSize)
                throw new FormatException("Hard window size exceeded");

            return window;
        }
    }
}

