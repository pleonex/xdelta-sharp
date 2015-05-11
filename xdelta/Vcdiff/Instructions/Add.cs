//
//  Add.cs
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
    public class Add : Instruction
    {
        public Add(byte sizeInTable)
            : base(sizeInTable, InstructionType.Add)
        {
			Data = new byte[0];
        }

        public byte[] Data {
            get;
            private set;
        }

        public override void DecodeInstruction(Window window, Stream input, Stream output)
        {
            Data = window.Data.ReadBytes(Size);
            output.Write(Data, 0, Data.Length);
        }

        public override string ToString()
        {
            return string.Format("ADD {0:X4}, {1}", Size, BitConverter.ToString(Data));
        }
    }
}

