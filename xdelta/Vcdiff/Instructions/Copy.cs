//
//  Copy.cs
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

namespace Xdelta.Instructions
{
    public class Copy : Instruction
    {
        private Cache cache;
        private byte binaryMode;

        private uint bytesRead;
        private uint hereAddress;

        public Copy(byte sizeInTable, byte mode, Cache cache)
            : base(sizeInTable, InstructionType.Copy)
        {
            this.cache = cache;
            this.binaryMode = mode;
        }

        public uint Address {
            get;
            private set;
        }

        public override void DecodeInstruction(Window window, Stream input, Stream output)
        {
            hereAddress = window.SourceSegmentLength + ((uint)output.Position - window.TargetWindowOffset);
            Address = cache.GetAddress(hereAddress, binaryMode, window.Addresses);
            bytesRead = 0;

            if (!window.Source.Contains(WindowFields.Source | WindowFields.Target))
                throw new Exception("Trying to copy from unknown source");

            for (int i = 0; i < Size; i++) {
                byte data = ReadFromSource(window, input, output);
                output.WriteByte(data);
            }
        }

        private byte ReadFromSource(Window window, Stream input, Stream output)
        {
            bool copyingFromTargetWindow = Address >= window.SourceSegmentLength;
            bool isTarget = window.Source.Contains(WindowFields.Target);
            Stream stream = (isTarget || copyingFromTargetWindow) ? output : input;

            uint address = GetStreamAddress(window);
            return PeekByte(stream, address);
        }

        private uint GetStreamAddress(Window window)
        {
            uint currentAddress = Address + bytesRead;

            // If we are copy from source window
            if (currentAddress < window.SourceSegmentLength)
                currentAddress += window.SourceSegmentOffset;
			else if (currentAddress < hereAddress + bytesRead) {
                // We are copying from current target window
                currentAddress -= window.SourceSegmentLength; // Relative
                currentAddress += window.TargetWindowOffset;
            } else
                throw new FormatException("Invalid copy address");

            return currentAddress;
        }

        private byte PeekByte(Stream stream, uint address)
        {
            long oldPosition = stream.Position;
            stream.Seek(address, SeekOrigin.Begin);

            int result = stream.ReadByte();
            stream.Seek(oldPosition, SeekOrigin.Begin);

            if (result == -1)
                throw new EndOfStreamException("In copy");

            bytesRead++;
            return (byte)result;
        }

        public override string ToString()
        {
            return string.Format("COPY {0:X4}, {1:X4}", Size, Address);
        }
    }
}

