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
namespace Pleosoft.XdeltaSharp.Vcdiff.Instructions
{
    using System;
    using System.IO;

    public class Copy : Instruction
    {
        private readonly Cache cache;
        private readonly byte binaryMode;

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
            uint hereAddress = window.SourceSegmentLength + ((uint)output.Position - window.TargetWindowOffset);
            Address = cache.GetAddress(hereAddress, binaryMode, window.Addresses);

            if (!window.Source.HasFlag(WindowFields.Source) && !window.Source.HasFlag(WindowFields.Target))
                throw new InvalidOperationException("Trying to copy from unknown source");

            CopyFromSourceWindow(window, input, output);
            CopyFromTargetWindow(window, output); // Not always
        }

        public override string ToString()
        {
            return string.Format("COPY {0:X4}, {1:X4}", Size, Address);
        }

        private void CopyFromSourceWindow(Window window, Stream input, Stream output)
        {
            // Check if there are some byte to copy from here
            if (Address >= window.SourceSegmentLength)
                return;

            // Decide the source
            Stream stream = window.Source.HasFlag(WindowFields.Target) ? output : input;

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
    }
}
