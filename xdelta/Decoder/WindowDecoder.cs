//
//  WindowDecoder.cs
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
    public class WindowDecoder
    {
        private Stream input;
        private BinaryReader inputReader;
        private Stream output;
        private BinaryWriter outputWriter;

        long inputSize;
        int blockShift;
        uint blockMask;

        uint numBlocks;
        uint blockOffset;

        public WindowDecoder(Stream input, Stream output)
        {
            this.input = input;
            this.inputReader = new BinaryReader(input);
            this.output = output;
            this.outputWriter = new BinaryWriter(output);

            Initialize();
        }

        private void Initialize()
        {
            inputSize  = input.Length.IsPowerOfTwo() ? input.Length : input.Length.RoundUpPowerOfTwo();
            blockShift = inputSize.Log2OfPowerOfTwo();
            blockMask  = (1u << blockShift) - 1;
        }

        public void Decode(Window window)
        {
            numBlocks   = window.CopyOffset >> blockShift;
            blockOffset = window.CopyOffset & blockMask;

            // TODO: While there are instructions to read
            // ... TODO: Parse it
            // ... TODO: Process it

            // TODO: Perfom checksum
        }
    }
}

