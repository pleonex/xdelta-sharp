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
    using System.IO;
    using NUnit.Framework;

    [TestFixture]
    public class DecoderTests
    {
        private Decoder decoder;
        private Stream input;
        private Stream patch;
        private Stream output;

        [TearDown]
        public void TearDown()
        {
            if (input != null)
                input.Dispose();

            if (patch != null)
                patch.Dispose();

            if (output != null)
                output.Dispose();
        }

        [Test]
        public void Getters()
        {
            InitWithStandardHeader();

            Assert.AreSame(input, decoder.Input);
            Assert.AreSame(patch, decoder.Patch);
            Assert.AreSame(output, decoder.Output);
        }

        private void InitWithStandardHeader()
        {
            input = new MemoryStream();
            patch = new MemoryStream();
            output = new MemoryStream();

            BinaryWriter patchWriter = new BinaryWriter(patch);
            patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x00);
            patch.Position = 0;

            decoder = new Decoder(input, patch, output);
        }
    }
}
