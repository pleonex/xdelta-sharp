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
        private Stream input;
        private Stream output;
        private Stream patch;
        private BinaryReader inputReader;
        private BinaryReader patchReader;
        private BinaryWriter outputWriter;

        uint currentWindow = 0;
        uint windowCount   = 0;
        uint windowOffset  = 0;
        uint windowLength  = 0;

        uint copyLength = 0;
        uint copyOffset = 0;
        uint checksumOffset = 0;

        public DecoderWindow(Stream input, Stream patch, Stream output)
        {
            this.input  = input;
            this.patch  = patch;
            this.output = output;

            inputReader  = new BinaryReader(input);
            patchReader  = new BinaryReader(patch);
            outputWriter = new BinaryWriter(output);
        }

        public void NextWindow()
        {
            InitiazeWindow();
        }

        private void InitiazeWindow()
        {
            currentWindow = windowCount;
            if (CheckOffsetOverflow(windowOffset, windowLength))
                throw new FormatException("decoder file offset overflow");

            // Updated at the initialization to avoid throwing an overflow error
            // decoding exactly 4 GB files.
            windowOffset += windowLength;

            // Get window indicator
            VcdWindow indicator = (VcdWindow)patchReader.ReadByte();
            if ((indicator & VcdWindow.NotSupported) != 0)
                throw new FormatException("unrecognized window indicator bits set");
        
            // Reset window variables
            copyLength = 0;
            copyOffset = 0;
            checksumOffset = 0;
        }

        private bool CheckOffsetOverflow(uint address, uint length)
        {
            return length > (UInt32.MaxValue - address);
        }
    }
}

