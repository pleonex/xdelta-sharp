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
using System.IO;
using NUnit.Framework;

namespace Xdelta.UnitTests
{
    [TestFixture]
    public class VcdReaderTests
    {
        VcdReader reader;
        Stream stream;

        [SetUp]
        public void SetUp()
        {
            stream = new MemoryStream();
            reader = new VcdReader(stream);
        }

        [TearDown]
        public void TearDown()
        {
            stream.Dispose();
        }

        private void WriteBytes(params byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Position -= data.Length;
        }

        private void TestThrows<T>(TestDelegate code, string message)
            where T : SystemException
        {
            T exception = Assert.Throws<T>(code);
            Assert.AreEqual(message, exception.Message);
        }

        [Test]
        public void ReadByteWithExactSize()
        {
            WriteBytes(0x10);
            byte actual = reader.ReadByte();
            Assert.AreEqual(0x10, actual);
        }

        [Test]
        public void ReadByteWithMoreBytes()
        {
            WriteBytes(0x9E);
            byte actual = reader.ReadByte();
            Assert.AreEqual(0x9E, actual);
            Assert.AreEqual(1, stream.Position);
        }

        [Test]
        public void ReadBytes()
        {
            byte[] expected = new byte[] { 0xCA, 0xFE, 0xBE, 0xBE, 0xBE };
            WriteBytes(expected);
            byte[] actual = reader.ReadBytes(5);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ReadIntegerWithExactSize()
        {
            WriteBytes(0xBA, 0xEF, 0x9A, 0x15);
            uint actual = reader.ReadInteger();
            Assert.AreEqual(123456789, actual);
        }

        [Test]
        public void ReadIntegerWithMoreBytes()
        {
            WriteBytes(0x88, 0x80, 0x80, 0x80, 0x00);
            uint actual = reader.ReadInteger();
            Assert.AreEqual(0x80000000, actual);
            Assert.AreEqual(5, stream.Position);
        }

        [Test]
        public void ReadIntegerWithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadInteger(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadIntegerWithOverflowValue()
        {
            WriteBytes(0x90, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadInteger(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadMoreThanAllowedBytes()
        {
            TestThrows<FormatException>(
                () => reader.ReadBytes(0x80000010),
                "Trying to read more than UInt32.MaxValue bytes");
        }
    }
}

