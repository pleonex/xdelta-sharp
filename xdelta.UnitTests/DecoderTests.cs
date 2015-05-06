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
	public class DecoderTests
	{
		private Decoder decoder;

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

			decoder = new Decoder(input, patch, output);
		}

		[Test]
		public void Getters()
		{
			Assert.AreSame(input, decoder.Input);
			Assert.AreSame(patch, decoder.Patch);
			Assert.AreSame(output, decoder.Output);
		}

		[Test]
		public void InvalidStamp()
		{
			patchWriter.Write(0x00AABBCC);
			patch.Position = 0;

			Assert.Throws<FormatException>(
				() => decoder.Run(),
				"not a VCDIFF input");
		}

		[Test]
		public void InvalidVersion()
		{
			patchWriter.Write(0x01C4C3D6);
			patch.Position = 0;

			Assert.Throws<FormatException>(
				() => decoder.Run(),
				"VCDIFF input version > 0 is not supported");
		}

		[Test]
		public void InvalidHeaderIndicator()
		{
			patchWriter.Write(0x00C4C3D6);
			patchWriter.Write(0xF8);
			patch.Position = 0;

			Assert.Throws<FormatException>(
				() => decoder.Run(),
				"unrecognized header indicator bits set");
		}
	}
}

