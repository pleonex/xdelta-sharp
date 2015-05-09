//
//  DecoderHeader.cs
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

namespace Xdelta
{
    internal class DecoderHeader
    {
        private const uint MagicStamp = 0xC4C3D6;
        private const byte SupportedVersion = 0x00;
        private static readonly System.Text.Encoding Encoding = System.Text.Encoding.ASCII;

        private VcdReader patchReader;

        public DecoderHeader(Stream patch)
        {
            patchReader = new VcdReader(patch);
        }

        public string ApplicationData {
            get;
            private set;
        }

        public void Run()
        {
            // Checks the first four bytes of the patch
            CheckStamp();

            // Now let's go to the header
            ReadHeader();
        }

        private void CheckStamp()
        {
            // Can't read as uint32 directly since a intenger format is non-encoded
            // in a standard format.
            uint stamp = BitConverter.ToUInt32(patchReader.ReadBytes(4), 0);
            if ((stamp & 0xFFFFFF) != MagicStamp)
                throw new FormatException("not a VCDIFF input");

            if ((stamp >> 24) > SupportedVersion)
                throw new FormatException("VCDIFF input version > 0 is not supported");
        }

        private void ReadHeader()
        {
            VcdHeaderFields header = (VcdHeaderFields)patchReader.ReadByte();
            if (header.Contains(VcdHeaderFields.NotSupported))
                throw new FormatException("unrecognized header indicator bits set");

            if (header.Contains(VcdHeaderFields.SecondaryCompression))
                throw new NotSupportedException("unavailable secondary compressor");

            if (header.Contains(VcdHeaderFields.CodeTable))
                throw new NotSupportedException("compressed code table not implemented");

            if (header.Contains(VcdHeaderFields.ApplicationData))
                ReadApplicationData();
        }

        private void ReadApplicationData()
        {
            int length = patchReader.ReadInt32();
            ApplicationData = Encoding.GetString(patchReader.ReadBytes(length));
        }
    }
}