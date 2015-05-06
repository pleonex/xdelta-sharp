//
//  Decoder.cs
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

namespace Xdelta
{
	public class Decoder
	{
		private const uint MagicStamp = 0xC4C3D6;
		private const byte SupportedVersion = 0x00;

		private BinaryReader inputReader;
		private BinaryReader patchReader;
		private BinaryWriter outputWriter;

		public Decoder(Stream input, Stream patch, Stream output)
		{
			Input  = input;
			Patch  = patch;
			Output = output;
		}

		public Stream Input {
			get;
			private set;
		}

		public Stream Patch {
			get;
			private set;
		}

		public Stream Output {
			get;
			private set;
		}

		public void Run()
		{
			inputReader  = new BinaryReader(Input);
			patchReader  = new BinaryReader(Patch);
			outputWriter = new BinaryWriter(Output);

			// Checks the first four bytes of the patch
			CheckStamp();
		}

		private void CheckStamp()
		{
			uint header = patchReader.ReadUInt32();

			if ((header & 0xFFFFFF) != MagicStamp)
				throw new FormatException("not a VCDIFF input");

			if ((header >> 24) > SupportedVersion)
				throw new FormatException("VCDIFF input version > 0 is not supported");
		}
	}
}

