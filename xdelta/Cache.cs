//
//  Cache.cs
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

