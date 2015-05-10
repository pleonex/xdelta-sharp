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
        private uint currentAddress;

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
            Address = cache.GetAddress((uint)output.Position, binaryMode, window.Addresses);
            currentAddress = window.SourceSegmentOffset + Address;
            if (Address + Size > window.SourceSegmentLength)
                throw new FormatException("Trying to read outsie the window");

            if (!window.Source.Contains(WindowFields.Source | WindowFields.Target))
                throw new FormatException("Trying to copy without source");

            for (int i = 0; i < Size; i++) {
                byte data = ReadFromSource(window.Source, input, output);
                output.WriteByte(data);
            }
        }

        private byte ReadFromSource(WindowFields source, Stream input, Stream output)
        {
            byte data;
            Stream stream = source.Contains(WindowFields.Source) ? input : output;

            long oldPosition = stream.Position;
            stream.Seek(currentAddress, SeekOrigin.Begin);
            data = (byte)stream.ReadByte();
            stream.Seek(oldPosition, SeekOrigin.Begin);

            currentAddress++;
            return data;
        }

        public override string ToString()
        {
            return string.Format("COPY {0:X4}, {1:X4}", Size, Address);
        }
    }
}

