//
//  Run.cs
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
    public class Run : Instruction
    {
        public Run(byte sizeInTable)
            : base(sizeInTable, InstructionType.Run)
        {
        }

        public byte Data {
            get;
            private set;
        }

        public override void DecodeInstruction(Window window, Stream input, Stream output)
        {
            Data = window.Data.ReadByte();

            byte[] dataArray = new byte[Size];
            for (int i = 0; i < Size; i++)
                dataArray[i] = Data;

            output.Write(dataArray, 0, dataArray.Length);
        }

        public override string ToString()
        {
            return string.Format("RUN {0:X8}, 0x{1:X2}", Size, Data);
        }
    }
}

