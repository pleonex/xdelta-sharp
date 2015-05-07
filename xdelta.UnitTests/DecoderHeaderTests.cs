//
//  Test.cs
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
using NUnit.Framework;
using System;
using System.IO;

namespace Xdelta.UnitTests
{
	[TestFixture]
	public class DecoderHeaderTests
	{
        private static readonly System.Text.Encoding Encoding = System.Text.Encoding.ASCII;

		private MemoryStream input;
		private MemoryStream patch;
		private MemoryStream output;

		private BinaryWriter inputWriter;
		private BinaryWriter patchWriter;
		private BinaryReader outputReader;

		[SetUp]
		public void SetUp()
		{
			input  = new MemoryStream();
			patch  = new MemoryStream();
			output = new MemoryStream();

			inputWriter = new BinaryWriter(input);
			patchWriter = new BinaryWriter(patch);
			outputReader = new BinaryReader(output);
		}

		[Test]
		public void InvalidStamp()
		{
			patchWriter.Write(0x00AABBCC);
			patch.Position = 0;

			Assert.Throws<FormatException>(
                () => new Decoder(input, patch, output),
				"not a VCDIFF input");
		}

		[Test]
		public void InvalidVersion()
		{
			patchWriter.Write(0x01C4C3D6);
			patch.Position = 0;

			Assert.Throws<FormatException>(
                () => new Decoder(input, patch, output),
				"VCDIFF input version > 0 is not supported");
		}

		[Test]
		public void InvalidHeaderIndicator()
		{
			patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0xF8);
			patch.Position = 0;

			Assert.Throws<FormatException>(
                () => new Decoder(input, patch, output),
				"unrecognized header indicator bits set");
		}

		[Test]
		public void HasSecondaryCompressor()
		{
			patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x01);
			patch.Position = 0;

			Assert.Throws<NotSupportedException>(
                () => new Decoder(input, patch, output),
				"unavailable secondary compressor");
		}

		[Test]
		public void HasCodeTable()
		{
			patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x02);
			patch.Position = 0;

			Assert.Throws<NotSupportedException>(
                () => new Decoder(input, patch, output),
				"compressed code table not implemented");
		}

        [Test]
        public void DoesNotThrowExceptionIfNotHeader()
        {
            patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x00);
            patch.Position = 0;

            Assert.DoesNotThrow(() => new Decoder(input, patch, output));
        }

        [Test]
        public void ReadCorrectlyApplicationData()
        {
            patchWriter.Write(0x00C4C3D6);
            patchWriter.Write((byte)0x04);
            patchWriter.Write(0x07);
            patchWriter.Write(Encoding.GetBytes("pleonex"));
            patch.Position = 0;

            Decoder decoder = new Decoder(input, patch, output);
            Assert.AreEqual("pleonex", decoder.ApplicationData);
        }
	}
}

