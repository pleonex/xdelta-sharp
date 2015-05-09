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
    internal class DecoderWindow
    {
        private const uint HardMaxWindowSize = 1u << 24;

        private BinaryReader inputReader;
        private VcdReader patchReader;
        private BinaryWriter outputWriter;

        uint currentWindow = 0;
        uint windowCount   = 0;

        uint windowOffset  = 0;
        uint windowLength  = 0;
        uint windowMaxPosition;

        VcdWindowFields indicator;
        uint copyLength = 0;
        uint copyOffset = 0;
        uint checksumOffset = 0;

        uint encodedPosition;
        uint encodedLength;

        public DecoderWindow(Stream input, Stream patch, Stream output)
        {
            inputReader  = new BinaryReader(input);
            patchReader  = new VcdReader(patch);
            outputWriter = new BinaryWriter(output);
        }

        public void NextWindow()
        {
            InitiazeWindow();
            ReadWindowData();
        }

        private void InitiazeWindow()
        {
            currentWindow = windowCount;
            if (windowOffset.CheckOverflow(windowLength))
                throw new FormatException("decoder file offset overflow");

            // Updated at the initialization to avoid throwing an overflow error
            // decoding exactly 4 GB files.
            windowOffset += windowLength;
                   
            // Reset window variables
            copyLength = 0;
            copyOffset = 0;
            checksumOffset = 0;
        }

        private void ReadWindowData()
        {
            // Get window indicator
            indicator = (VcdWindowFields)patchReader.ReadByte();
            if ((indicator & VcdWindowFields.NotSupported) != 0)
                throw new FormatException("unrecognized window indicator bits set");

            if ((indicator & (VcdWindowFields.Source | VcdWindowFields.Target)) != 0) {
                copyLength = patchReader.ReadUInt32();  // Copy window length
                copyOffset = patchReader.ReadUInt32();  // Copy window offset
            }

            // Copy offset and copy length may not overflow
            if (copyOffset.CheckOverflow(copyLength))
                throw new FormatException("decoder copy window overflows a file offset");
       
            // Check copy window bounds
            if ((indicator & VcdWindowFields.Target) != 0 &&
               copyOffset + copyLength > windowOffset)
                throw new FormatException("VCD_TARGET window out of bounds");

            // Set encoded data address and length
            encodedPosition = copyLength;
            encodedLength   = patchReader.ReadUInt32();  // Length of the delta encoding

            // Get the length of target window
            windowLength = patchReader.ReadUInt32();
            windowMaxPosition = copyLength + windowLength;

            // Set the maximum decoder position, beyond which we should not
            // decode any data.  This is the maximum value for dec_position.
            //  This may not exceed the size of a UInt32
            if (copyLength.CheckOverflow(windowLength))
                throw new FormatException("decoder target window overflows a UInt32");

            // Check for malicious files
            if (windowLength > HardMaxWindowSize)
                throw new FormatException("Hard window size exceeded");
        }
    }
}

