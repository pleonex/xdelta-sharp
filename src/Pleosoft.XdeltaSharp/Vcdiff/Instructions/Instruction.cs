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
using System.IO;

namespace Xdelta.Instructions
{
    public abstract class Instruction
    {
        public Instruction(byte sizeInTable, InstructionType type)
        {
            Type = type;
            SizeInTable = sizeInTable;
            Size = sizeInTable;
        }

        public InstructionType Type {
            get;
            private set;
        }

        protected byte SizeInTable {
            get;
            private set;
        }

        public uint Size {
            get;
            private set;
        }

        public abstract void DecodeInstruction(Window window, Stream input, Stream output);

        public void Decode(Window window, Stream input, Stream output)
        {
            if (Type != InstructionType.Noop && SizeInTable == 0)
                Size = window.Instructions.ReadInteger();

            DecodeInstruction(window, input, output);
        }
    }
}

