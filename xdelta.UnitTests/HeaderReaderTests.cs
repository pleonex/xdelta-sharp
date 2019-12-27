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
using NUnit.Framework;
using System;
using System.IO;

namespace Xdelta.UnitTests
{
    [TestFixture]
    public class HeaderReaderTests
    {
        private static readonly System.Text.Encoding Encoding = System.Text.Encoding.ASCII;

        private MemoryStream input;
        private MemoryStream output;
        private MemoryStream patch;

        [SetUp]
        public void SetUp()
        {
            input  = new MemoryStream();
            output = new MemoryStream();
            patch  = new MemoryStream();
        }

        [TearDown]
        public void TearDown()
        {
            patch.Dispose();
            input.Dispose();
            output.Dispose();
        }

        private void WriteBytes(params byte[] data)
        {
            patch.Write(data, 0, data.Length);
            patch.Position -= data.Length;
        }

        private void TestThrows<T>(string message)
            where T : SystemException
        {
            T exception = Assert.Throws<T>(() => new Decoder(input, patch, output));
            Assert.AreEqual(message, exception.Message);
        }

        [Test]
        public void InvalidStamp()
        {
            WriteBytes(0x00, 0xAA, 0xBB, 0xCC);
            TestThrows<FormatException>("not a VCDIFF input");
        }

        [Test]
        public void InvalidVersion()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x01);
            TestThrows<FormatException>("VCDIFF input version > 0 is not supported");
        }

        [Test]
        public void InvalidHeaderIndicator()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0xF8);
            TestThrows<FormatException>("unrecognized header indicator bits set");
        }

        [Test]
        public void InvalidHeaderIndicator2()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x48);
            TestThrows<FormatException>("unrecognized header indicator bits set");
        }

        [Test]
        public void HasSecondaryCompressor()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x01);
            TestThrows<NotSupportedException>("unavailable secondary compressor");
        }

        [Test]
        public void HasCodeTable()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x02);
            TestThrows<NotSupportedException>("compressed code table not implemented");
        }

        [Test]
        public void DoesNotThrowExceptionIfNotHeader()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x00);
            Assert.DoesNotThrow(() => new Decoder(input, patch, output));
        }

        [Test]
        public void ReadCorrectlyApplicationData()
        {
            WriteBytes(0xD6, 0xC3, 0xC4, 0x00, 0x04, 0x07);

            patch.Position = 6;
            WriteBytes(Encoding.GetBytes("pleonex"));
            patch.Position = 0;

            Decoder decoder = new Decoder(input, patch, output);
            Assert.AreEqual("pleonex", decoder.Header.ApplicationData);
        }
    }
}

