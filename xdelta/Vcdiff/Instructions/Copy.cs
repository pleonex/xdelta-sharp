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

            if (!window.Source.Contains(WindowFields.Source | WindowFields.Target))
                throw new Exception("Trying to copy from unknown source");

            CopyFromSourceWindow(window, input, output);
            CopyFromTargetWindow(window, output); // Not always
        }

        private void CopyFromSourceWindow(Window window, Stream input, Stream output)
        {
            // Check if there are some byte to copy from here
            if (Address >= window.SourceSegmentLength)
                return;
                
            // Decide the source
            Stream stream = window.Source.Contains(WindowFields.Target) ? output : input;

            // Get the length
            uint length = Size;
            if (Address + Size > window.SourceSegmentLength)
                length = window.SourceSegmentLength - Address;

            // Get the address
            uint address = Address + window.SourceSegmentOffset;

            // And copy
            DirectCopy(stream, output, address, (int)length);
        }

        private void CopyFromTargetWindow(Window window, Stream output)
        {
            // If there is no data from target window, just return
            if (Address + Size < window.SourceSegmentLength)
                return;

            // Get length, that is Size except if we have read something from SourceWindow
            uint length = Size;
            if (Address < window.SourceSegmentLength)
                length -= window.SourceSegmentLength - Address;

            // Get address
            uint address = window.TargetWindowOffset;        // Absolute to target window
            address += Address - window.SourceSegmentLength; // Relative to TargetWindow

            // Determine if some bytes can't be read still
            bool overlap = address + length >= output.Position;

            // If there is no overlap, the typical read and write, else copy one by one
            if (!overlap)
                DirectCopy(output, output, address, (int)length);
            else
                SlowCopy(output, address, (int)length);
        }

        private void DirectCopy(Stream input, Stream output, uint address, int length)
        {
            byte[] data = new byte[length];

            // Seek and read. Need to keep the position if we are reading from output
            long oldAddress = input.Position;
            input.Position = address;
            input.Read(data, 0, length);
            input.Position = oldAddress;

            // Write
            output.Write(data, 0, length);
        }

        private void SlowCopy(Stream stream, uint address, int length)
        {
            long startOutputPosition = stream.Position;
            int availableData = (int)(startOutputPosition - address);
            byte[] buffer = new byte[availableData];

            for (int i = 0; i < length; i += availableData) {
                int toCopy = (length - i < availableData) ? length - i : availableData;

                stream.Position = address + i;
                stream.Read(buffer, 0, toCopy);

                stream.Position = startOutputPosition + i;
                stream.Write(buffer, 0, toCopy);
            }
        }

        public override string ToString()
        {
            return string.Format("COPY {0:X4}, {1:X4}", Size, Address);
        }
    }
}

