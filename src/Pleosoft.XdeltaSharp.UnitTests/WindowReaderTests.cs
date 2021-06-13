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
namespace Pleosoft.XdeltaSharp.UnitTests
{
    using System;
    using System.IO;
    using NUnit.Framework;

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
            input = new MemoryStream();
            output = new MemoryStream();
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
            WriteBytes(new byte[] {
                0x05, 0x10, 0x81, 0x00, 0x04, 0x00, 0x00,
                0x04, 0x0, 0x02, 0x00, 0x00, 0x00, 0x01,
                0x0A, 0x0B, 0x0C, 0x0D, 0xCA, 0xFE,
            });

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
            Assert.AreEqual(new byte[] { 0xCA, 0xFE }, window.Addresses.ReadBytes(2));
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
    }
}
