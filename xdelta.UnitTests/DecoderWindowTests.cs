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

        [SetUp]
        public void SetUp()
        {
            input = new MemoryStream();
            inputWriter = new BinaryWriter(input);

            output = new MemoryStream();
            outputReader = new BinaryReader(output);

            patch = new MemoryStream();

            WriteGenericHeader();
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
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x00);
        }

        private void WriteBytes(params byte[] data)
        {
            patch.Write(data, 0, data.Length);
			patch.Position -= data.Length;
        }

		private void TestThrows<T>(string message)
			where T : SystemException
		{
			T exception = Assert.Throws<T>(() => decoder.Run());
			Assert.AreEqual(message, exception.Message);
		}

        [Test]
        public void WindowIndicatorWithAllBitS()
        {
            WriteBytes(0x81, 0x7F);
			TestThrows<FormatException>("unrecognized window indicator bits set");
        }

        [Test]
        public void WindowIndicatorWithInvalidBit()
        {
            WriteBytes(0x08);
			TestThrows<FormatException>("unrecognized window indicator bits set");
        }

        [Test]
        public void WindowCopyOverflow()
        {
            WriteBytes(0x01, 0x10, 0x8F, 0xFF, 0xFF, 0xFF, 0x70);
            TestThrows<FormatException>("decoder copy window overflows a file offset");
        }

        [Test]
        public void WindowCopyWindowOverflow()
        {
            WriteBytes(0x02, 0x10, 0x04);
            TestThrows<FormatException>("VCD_TARGET window out of bounds");
        }

        [Test]
        public void WindowOverflow()
        {
            WriteBytes(0x00, 0x8F, 0xFF, 0xFF, 0xFF, 0xF0, 0x01, 0x00, 0x10);
            Assert.Throws<FormatException>(
                () => decoder.Run(),
                "decoder target window overflows a UInt32");
        }

        [Test]
        public void WindowHardMaximumSize()
        {
            WriteBytes(0x00, 0x05, 0x01, 0x00, 0x8F, 0xFF, 0xFF, 0xFF, 0xF0);
            Assert.Throws<FormatException>(
                () => decoder.Run(),
                "Hard window size exceeded");
        }
    }
}

