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
            byte actual = reader.ReadUInt8();
            Assert.AreEqual(0x10, actual);
        }

        [Test]
        public void ReadByteWithMoreBytes()
        {
            WriteBytes(0x81, 0x1E);
            byte actual = reader.ReadUInt8();
            Assert.AreEqual(0x9E, actual);
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadByteWithOverflowBits()
        {
            WriteBytes(0x80, 0x81);
            TestThrows<FormatException>(
                () => reader.ReadUInt8(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadByteWithOverflowValue()
        {
            WriteBytes(0x83, 0x01);
            TestThrows<FormatException>(
                () => reader.ReadUInt8(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadSByteWithExactSize()
        {
            WriteBytes(0x7F);
            sbyte actual = reader.ReadInt8();
            Assert.AreEqual(0x7F, actual);
        }

        [Test]
        public void ReadSByteWithMoreBytes()
        {
            WriteBytes(0x81, 0x00);
            sbyte actual = reader.ReadInt8();
            Assert.AreEqual(-128, actual);
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadSByteWithOverflowBits()
        {
            WriteBytes(0x80, 0x81);
            TestThrows<FormatException>(
                () => reader.ReadInt8(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadSByteWithOverflowValue()
        {
            WriteBytes(0x83, 0x1);
            TestThrows<FormatException>(
                () => reader.ReadInt8(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadUInt16WithExactSize()
        {
            WriteBytes(0x84, 0x00);
            ushort actual = reader.ReadUInt16();
            Assert.AreEqual(0x200, actual);
        }

        [Test]
        public void ReadUInt16WithMoreBytes()
        {
            WriteBytes(0x82, 0x81, 0x40);
            ushort actual = reader.ReadUInt16();
            Assert.AreEqual(0x80C0, actual);
            Assert.AreEqual(3, stream.Position);
        }

        [Test]
        public void ReadUInt16WithOverflowBits()
        {
            WriteBytes(0x80, 0x81, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadUInt16(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadUInt16WithOverflowValue()
        {
            WriteBytes(0x84, 0x81, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadUInt16(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadInt16WithExactSize()
        {
            WriteBytes(0x84, 0x00);
            short actual = reader.ReadInt16();
            Assert.AreEqual(0x200, actual);
        }

        [Test]
        public void ReadInt16WithMoreBytes()
        {
            WriteBytes(0x82, 0x80, 0x00);
            short actual = reader.ReadInt16();
            Assert.AreEqual(-32768, actual);
            Assert.AreEqual(3, stream.Position);
        }

        [Test]
        public void ReadInt16WithOverflowBits()
        {
            WriteBytes(0x80, 0x81, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadInt16(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadInt16WithOverflowValue()
        {
            WriteBytes(0x84, 0x81, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadInt16(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadUInt32WithExactSize()
        {
            WriteBytes(0xBA, 0xEF, 0x9A, 0x15);
            uint actual = reader.ReadUInt32();
            Assert.AreEqual(123456789, actual);
        }

        [Test]
        public void ReadUInt32WithMoreBytes()
        {
            WriteBytes(0x88, 0x80, 0x80, 0x80, 0x00);
            uint actual = reader.ReadUInt32();
            Assert.AreEqual(0x80000000, actual);
            Assert.AreEqual(5, stream.Position);
        }

        [Test]
        public void ReadUInt32WithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadUInt32(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadUInt32WithOverflowValue()
        {
            WriteBytes(0x90, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadUInt32(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadInt32WithExactSize()
        {
            WriteBytes(0xBA, 0xEF, 0x9A, 0x15);
            int actual = reader.ReadInt32();
            Assert.AreEqual(123456789, actual);
        }

        [Test]
        public void ReadInt32WithMoreBytes()
        {
            WriteBytes(0x88, 0x80, 0x80, 0x80, 0x00);
            int actual = reader.ReadInt32();
            Assert.AreEqual(Int32.MinValue, actual);
            Assert.AreEqual(5, stream.Position);
        }

        [Test]
        public void ReadInt32WithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadInt32(),
                "overflow in decode_integer");
        }

        [Test]
        public void ReadInt32WithOverflowValue()
        {
            WriteBytes(0x90, 0x80, 0x80, 0x80, 0x80);
            TestThrows<FormatException>(
                () => reader.ReadInt32(),
                "overflow in decode_integer");
        }


        [Test]
        public void ReadUInt64WithExactSize()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInt64();
            Assert.AreEqual(0x01, actual);
        }

        [Test]
        public void ReadUInt64WithMoreBytes1()
        {
            WriteBytes(0xC0, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInt64();
            Assert.AreEqual(0x4000000000000001, actual);
            Assert.AreEqual(9, stream.Position);
        }

        [Test]
        public void ReadUInt64WithMoreBytes2()
        {
            WriteBytes(0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            ulong actual = reader.ReadUInt64();
            Assert.AreEqual(0x8000000000000001, actual);
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadUInt64WithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadUInt64(),
                "overflow in decode_integer");
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadUInt64WithOverflowValue()
        {
            WriteBytes(0x82, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            ulong actual = reader.ReadUInt64();
            Assert.AreEqual(0x00, actual);
        }

        [Test]
        public void ReadInt64WithExactSize()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            long actual = reader.ReadInt64();
            Assert.AreEqual(0x01, actual);
        }

        [Test]
        public void ReadInt64WithMoreBytes1()
        {
            WriteBytes(0xC0, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x01);
            long actual = reader.ReadInt64();
            Assert.AreEqual(0x4000000000000001, actual);
            Assert.AreEqual(9, stream.Position);
        }

        [Test]
        public void ReadInt64WithMoreBytes2()
        {
            WriteBytes(0x81, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            long actual = reader.ReadInt64();
            Assert.AreEqual(Int64.MinValue, actual);
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadInt64WithOverflowBits()
        {
            WriteBytes(0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            TestThrows<FormatException>(
                () => reader.ReadInt64(),
                "overflow in decode_integer");
            Assert.AreEqual(10, stream.Position);
        }

        [Test]
        public void ReadInt64WithOverflowValue()
        {
            WriteBytes(0x82, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00);
            long actual = reader.ReadInt64();
            Assert.AreEqual(0x00, actual);
        }
    }
}

