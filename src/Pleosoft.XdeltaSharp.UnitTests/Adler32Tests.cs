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
    public class Adler32Tests
    {
        private const int NMax = 5552;

        [Test]
        public void Guards()
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);
            stream.Position = 2;

            Assert.That(() => Adler32.Run(null, 0), Throws.ArgumentNullException);
            Assert.That(() => Adler32.Run(stream, -2), Throws.InstanceOf<ArgumentOutOfRangeException>());
            Assert.That(() => Adler32.Run(stream, 3), Throws.InstanceOf<EndOfStreamException>());
        }

        [Test]
        public void EmptyStream()
        {
            using var stream = new MemoryStream();

            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void SingleByte()
        {
            using var stream = new MemoryStream();
            stream.WriteByte(0xCA);

            stream.Position = 0;
            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(13304011));
        }

        [Test]
        public void SingleByteWithContinuation()
        {
            using var stream = new MemoryStream();
            stream.WriteByte(0xCA);

            stream.Position = 0;
            uint result = Adler32.Run(stream, stream.Length, (65530u << 16) | 65530u);

            Assert.That(result, Is.EqualTo(14418131));
        }

        [Test]
        public void SmallerThan16Bytes()
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { 0xCA, 0xFE, 0xBA, 0xBE, 0xDE, 0xAD, 0xBE, 0xEF, }, 0, 8);

            stream.Position = 0;
            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(491128441));
        }

        [Test]
        public void SmallerThan16BytesWithContinuation()
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { 0xCA, 0xFE, 0xBA, 0xBE, 0xDE, 0xAD, 0xBE, 0xEF, }, 0, 8);

            stream.Position = 0;
            uint result = Adler32.Run(stream, stream.Length, (65530u << 16) | 65530u);

            Assert.That(result, Is.EqualTo(495912577));
        }

        [Test]
        public void SmallerThanNMax()
        {
            using var stream = GetTestStream(NMax - 100);

            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(3475765439));
        }

        [Test]
        public void LengthLikeNMax()
        {
            using var stream = GetTestStream(NMax);

            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(2367181278));
        }

        [Test]
        public void BiggerThanNMax()
        {
            using var stream = GetTestStream((NMax * 4) + 100);

            uint result = Adler32.Run(stream, stream.Length);

            Assert.That(result, Is.EqualTo(2570494100));
        }

        private Stream GetTestStream(long length)
        {
            var stream = new MemoryStream();
            var data = new byte[] { 0xCA, 0xFE };
            for (int i = 0; i < length / data.Length; i++) {
                stream.Write(data, 0, data.Length);
            }

            stream.Position = 0;
            return stream;
        }
    }
}
