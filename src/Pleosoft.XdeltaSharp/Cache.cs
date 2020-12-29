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
using System;

namespace Xdelta
{
    public class Cache
    {
        private uint[] near;
        private uint[] same;
        private int nextSlot;

        public Cache(byte nearSlots, byte sameSlots)
        {
            NearSlots = nearSlots;
            SameSlots = sameSlots;
            near = new uint[NearSlots];
            same = new uint[SameSlots * 256];
        }

        public byte NearSlots {
            get;
            private set;
        }

        public byte SameSlots {
            get;
            private set;
        }

        public void Initialize()
        {
            nextSlot = 0;
            Array.Clear(near, 0, NearSlots);
            Array.Clear(same, 0, SameSlots * 256);
        }

        public uint GetAddress(uint hereAddress, byte mode, VcdReader addressSection)
        {
            uint address = 0;
            switch (GetMode(mode)) {
            case AddressMode.Self:
                address = addressSection.ReadInteger();
                break;

            case AddressMode.Here:
                address = hereAddress - addressSection.ReadInteger();
                break;

            case AddressMode.Near:
                address = near[mode - 2] + addressSection.ReadInteger();
                break;

            case AddressMode.Same:
                int index = mode - (2 + NearSlots);
                address = same[index * 256 + addressSection.ReadByte()];
                break;
            }

            Update(address);
            return address;
        }

        private AddressMode GetMode(byte mode)
        {
            AddressMode addressMode = AddressMode.Invalid;
            if (mode == 0)
                addressMode = AddressMode.Self;
            else if (mode == 1)
                addressMode = AddressMode.Here;
            else if (mode >= 2 && mode <= NearSlots + 1)
                addressMode = AddressMode.Near;
            else if (mode >= NearSlots + 2 && mode <= NearSlots + SameSlots + 1)
                addressMode = AddressMode.Same;

            return addressMode;
        }

        private void Update(uint address)
        {
            if (NearSlots > 0) {
                near[nextSlot] = address;
                nextSlot = (nextSlot + 1) % NearSlots;
            }

            if (SameSlots > 0)
                same[address % (SameSlots * 256)] = address;
        }
    }
}

