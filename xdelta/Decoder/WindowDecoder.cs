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
using Xdelta.Instructions;

namespace Xdelta
{
    public class WindowDecoder
    {
        private Stream input;
        private Stream output;
        private CodeTable codeTable;

        public WindowDecoder(Stream input, Stream output)
        {
            this.input  = input;
            this.output = output;
            this.codeTable = CodeTable.Default;
        }

        public void Decode(Window window)
        {
            // Clear cache
            codeTable.Cache.Initialize();

            // Set the target window offset
            window.TargetWindowOffset = (uint)output.Position;

            // For each instruction
            Instruction firstInstruction, secondInstruction;
            while (!window.Instructions.Eof) {
                byte codeIndex = window.Instructions.ReadByte();
                codeTable.GetInstructions(codeIndex, out firstInstruction, out secondInstruction);

                firstInstruction.Decode(window, input, output);
                secondInstruction.Decode(window, input, output);
            }

            // Check all data has been decoded
            if (output.Position - window.TargetWindowOffset != window.TargetWindowLength)
                throw new Exception("Target window not fully decoded");

            // Check checksum
			if (window.Source.Contains(WindowFields.Adler32)) {
				output.Position = window.TargetWindowOffset;
				uint newAdler = Adler32.Run(1, output, window.TargetWindowLength);
				if (newAdler != window.Checksum)
					throw new Exception("Invalid checksum");
			}
        }
    }
}

