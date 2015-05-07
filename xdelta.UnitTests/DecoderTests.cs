//
//  DecoderTests.cs
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
    public class DecoderTests
    {
        private Decoder decoder;
        private Stream input;
        private Stream patch;
        private Stream output;

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

        #if DEBUG
        [Test]
        [Ignore]
        public void NinokuniPatch()
        {
            string gamePath = "/store/Juegos/NDS/Ninokuni [CLEAN].nds";
            string patchPath = "/home/benito/parche.xdelta";
            patch  = new FileStream(patchPath, FileMode.Open);
            input  = new FileStream(gamePath, FileMode.Open);
            output = new MemoryStream();

            Assert.DoesNotThrow(() => {
                decoder = new Decoder(input, patch, output);
                decoder.Run();
            });
        }
        #endif
    }
}

