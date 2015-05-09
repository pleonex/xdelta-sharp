//
//  VcdiffReaderTests.cs
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
#define USE_32_BITS_INTEGERS

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

        #if USE_32_BITS_INTEGERS
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
        #else
        [Test]
        public void ReadUIntegerWithExactSize()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInteger();
            Assert.AreEqual(0x01, actual);
        }

        [Test]
        public void ReadUIntegerWithMoreBytes1()
        {
            WriteBytes(0xC0, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInteger();
            Assert.AreEqual(0x4000000000000001, actual);
            Assert.AreEqual(9, stream.Position);
        }

        [Test]
        public void ReadUIntegerWithMoreBytes2()
        {
            WriteBytes(0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInteger();
            Assert.AreEqual(0x8000000000000001, actual);
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadUIntegerWithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadUInteger(),
                "overflow in decode_integer");
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadUIntegerWithOverflowValue()
        {
            WriteBytes(0x82, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            ulong actual = reader.ReadUInteger();
            Assert.AreEqual(0x00, actual);
        }

        [Test]
        public void ReadInt64WithExactSize()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            long actual = reader.ReadInteger();
            Assert.AreEqual(0x01, actual);
        }

        [Test]
        public void ReadIntegerWithMoreBytes1()
        {
            WriteBytes(0xC0, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            long actual = reader.ReadInteger();
            Assert.AreEqual(0x4000000000000001, actual);
            Assert.AreEqual(9, stream.Position);
        }

        [Test]
        public void ReadIntegerWithMoreBytes2()
        {
            WriteBytes(0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            long actual = reader.ReadInteger();
            Assert.AreEqual(Int64.MinValue, actual);
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadIntegerWithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadInteger(),
                "overflow in decode_integer");
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadIntegerWithOverflowValue()
        {
            WriteBytes(0x82, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            long actual = reader.ReadInteger();
            Assert.AreEqual(0x00, actual);
        }
        #endif
    }
}

