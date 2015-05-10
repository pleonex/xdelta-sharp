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
    public class WindowReaderTests
    {
        private Decoder decoder;

        private MemoryStream input;
        private MemoryStream output;
        private MemoryStream patch;

        [SetUp]
        public void SetUp()
        {
            input  = new MemoryStream();
            output = new MemoryStream();
            patch  = new MemoryStream();

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
        public void ThrowsIfWindowIndicatorWithAllBits()
        {
            WriteBytes(0x81, 0x7F);
			TestThrows<FormatException>("unrecognized window indicator bits set");
        }

        [Test]
        public void ThrowsIfWindowIndicatorWithInvalidBit()
        {
            WriteBytes(0x08);
			TestThrows<FormatException>("unrecognized window indicator bits set");
        }

        [Test]
        public void ThrowsIfWindowCopyOverflow()
        {
            WriteBytes(0x01, 0x10, 0x8F, 0xFF, 0xFF, 0xFF, 0x70);
            TestThrows<FormatException>("decoder copy window overflows a file offset");
        }

        [Test]
        public void ThrowsIfWindowCopyWindowOverflow()
        {
            WriteBytes(0x02, 0x10, 0x04);
            TestThrows<FormatException>("VCD_TARGET window out of bounds");
        }

        [Test]
        public void ThrowsIfWindowOverflow()
        {
            WriteBytes(0x01, 0x8F, 0xFF, 0xFF, 0xFF, 0x70, 0x00, 0x00, 0x10);
            TestThrows<FormatException>("decoder target window overflows a UInt32");
        }

        [Test]
        public void ThrowsIfWindowHardMaximumSize()
        {
            WriteBytes(0x01, 0x04, 0x00, 0x00, 0x8F, 0xFF, 0xFF, 0xFF, 0x70);
            TestThrows<FormatException>("Hard window size exceeded");
        }

        [Test]
        public void ThrowsIfAllFieldsCompressed()
        {
            WriteBytes(0x00, 0x00, 0x00, 0xFF);
            TestThrows<FormatException>("unrecognized delta indicator bits set");
        }

        [Test]
        public void ThrowsExceptionIfInvalidFieldCompressed()
        {
            WriteBytes(0x00, 0x00, 0x00, 0xF8);
            TestThrows<FormatException>("unrecognized delta indicator bits set");
        }

        [Test]
        public void ThrowsExceptionIfCompressedActivate()
        {
            WriteBytes(0x00, 0x00, 0x00, 0x01);
            TestThrows<FormatException>("invalid delta indicator bits set");
        }

        [Test]
        public void TestValidWindowFields()
        {
            WriteBytes(0x05, 0x10, 0x81, 0x00, 0x04, 0x00, 0x00,
                0x04, 0x0, 0x02, 0x00, 0x00, 0x00, 0x01,
                0x0A, 0x0B, 0x0C, 0x0D, 0xCA, 0xFE);

            Assert.DoesNotThrow(() => decoder.Run());
            Assert.AreEqual(patch.Length, patch.Position);
            Window window = decoder.LastWindow;

            Assert.AreEqual(WindowFields.Source | WindowFields.Adler32, window.Source);
            Assert.AreEqual(0x10, window.SourceSegmentLength);
            Assert.AreEqual(0x80, window.SourceSegmentOffset);
            Assert.AreEqual(0x00, window.TargetWindowLength);
            Assert.AreEqual(WindowCompressedFields.None, window.CompressedFields);
            Assert.AreEqual(0x04, window.Data.BaseStream.Length);
            Assert.AreEqual(0x00, window.Instructions.BaseStream.Length);
            Assert.AreEqual(0x02, window.Addresses.BaseStream.Length);
            Assert.AreEqual(0x01, window.Checksum);
            Assert.AreEqual(new byte[] { 0xA, 0xB, 0xC, 0xD }, window.Data.ReadBytes(4));
            //Assert.AreEqual(new byte[] { 0x0F }, window.Instructions.ReadBytes(1)); // No instruction to process
            Assert.AreEqual(new byte[] { 0xCA, 0xFE }, window.Addresses.ReadBytes(2));
        }
    }
}

