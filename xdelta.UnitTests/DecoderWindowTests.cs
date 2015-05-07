//
//  DecoderTests.cs
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
using NUnit.Framework;

namespace Xdelta.UnitTests
{
    [TestFixture]
    public class DecoderWindowTests
    {
        private Decoder decoder;

        private MemoryStream input;
        private BinaryWriter inputWriter;

        private MemoryStream output;
        private BinaryReader outputReader;

        private MemoryStream patch;
        private BinaryWriter patchWriter;

        [SetUp]
        public void SetUp()
        {
            input = new MemoryStream();
            inputWriter = new BinaryWriter(input);

            output = new MemoryStream();
            outputReader = new BinaryReader(output);

            patch = new MemoryStream();
            patchWriter = new BinaryWriter(patch);

            WriteGenericHeader();
            patch.Position = 0;
            decoder = new Decoder(input, patch, output);
        }

        [TearDown]
        public void TearDown()
        {
            patch.Dispose();
            input.Dispose();
            output.Dispose();
        }

        private void WriteGenericHeader()
        {
            patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x00);
        }

        [Test]
        public void WindowIndicatorWithAllBitS()
        {
            patchWriter.Write((byte)0xFF);
            patch.Position -= 1;

            Assert.Throws<FormatException>(
                () => decoder.Run(),
                "unrecognized window indicator bits set");
        }

        [Test]
        public void WindowIndicatorWithInvalidBit()
        {
            patchWriter.Write((byte)0x08);
            patch.Position -= 1;

            Assert.Throws<FormatException>(
                () => decoder.Run(),
                "unrecognized window indicator bits set");
        }

        [Test]
        public void WindowCopyOverflow()
        {
            patchWriter.Write((byte)0x00);
            patchWriter.Write((byte)0x0);
            patchWriter.Write(UInt32.MaxValue - 0x10);
            patch.Position -= 9;

            Assert.Throws<FormatException>(
                () => decoder.Run(),
                "decoder copy window overflows a file offset");
        }
    }
}

