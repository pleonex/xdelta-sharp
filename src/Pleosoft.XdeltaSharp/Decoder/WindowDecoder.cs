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
namespace Pleosoft.XdeltaSharp
{
    using System;
    using System.IO;
    using Pleosoft.XdeltaSharp.Instructions;

    public class WindowDecoder
    {
        private readonly Stream input;
        private readonly Stream output;
        private readonly CodeTable codeTable;

        public WindowDecoder(Stream input, Stream output)
        {
            this.input = input;
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
                throw new InvalidOperationException("Target window not fully decoded");

            // Check checksum
            if (window.Source.HasFlag(WindowFields.Adler32)) {
                output.Position = window.TargetWindowOffset;
                uint newAdler = Adler32.Run(output, window.TargetWindowLength);
                if (newAdler != window.Checksum)
                    throw new InvalidOperationException("Invalid checksum");
            }
        }
    }
}
