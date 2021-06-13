// Original code in C form Zlib by Mark Adler 1995-1998
// https://github.com/madler/zlib/blob/50893291621658f355bc5b4d450a8d06a563053d/adler32.c
//
//   Copyright (C) 1995-2013 Jean-loup Gailly and Mark Adler
//   This software is provided 'as-is', without any express or implied
//   warranty.  In no event will the authors be held liable for any damages
//   arising from the use of this software.
//   Permission is granted to anyone to use this software for any purpose,
//   including commercial applications, and to alter it and redistribute it
//   freely, subject to the following restrictions:
//   1. The origin of this software must not be misrepresented; you must not
//      claim that you wrote the original software. If you use this software
//      in a product, an acknowledgment in the product documentation would be
//      appreciated but is not required.
//   2. Altered source versions must be plainly marked as such, and must not be
//      misrepresented as being the original software.
//   3. This notice may not be removed or altered from any source distribution.
//   Jean-loup Gailly        Mark Adler
//   jloup@gzip.org          madler@alumni.caltech.edu
//   The data format used by the zlib library is described by RFCs (Request for
//   Comments) 1950 to 1952 in the files http://tools.ietf.org/html/rfc1950
//   (zlib format), rfc1951 (deflate format) and rfc1952 (gzip format).

// Copyright (c) 2019 Benito Palacios Sánchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Pleosoft.XdeltaSharp
{
    using System;
    using System.Buffers;
    using System.IO;

    /// <summary>
    /// Adler-32 checksum algorithm.
    /// </summary>
    /// <remarks>
    /// <para>For more information: https://en.wikipedia.org/wiki/Adler-32.</para>
    /// </remarks>
    public static class Adler32
    {
        // By definition the algorithm starts with value 1.
        private const uint StartValue = 1;

         // Modulo of the sums. Largest prime smaller than 65536
        private const int SumModulo = 65521;

        // Max number of bytes that requires a maximum one modulo.
        // Largest n such that 255n(n+1)/2 + (n+1)(Base-1) <= 2^32-1
        private const int MaxModuleBlock = 5552;

        /// <summary>
        /// Computes the checksum Adler-32 from a stream.
        /// </summary>
        /// <param name="stream">The stream to read data. It starts at the current position.</param>
        /// <param name="length">The amount of bytes to read from the stream for the checksum.</param>
        /// <param name="start">The start value of the checksum.</param>
        /// <returns>The Adler-32 checksum of the data.</returns>
        public static uint Run(Stream stream, long length, uint start = StartValue)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Invalid length");
            if (stream.Position + length > stream.Length)
                throw new EndOfStreamException("Not enough bytes to read.");

            uint sumA = start & 0xFFFF;
            uint sumB = start >> 16;

            // Computes the checksum in blocks that requires maximum one modulo
            byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxModuleBlock);
            while (length > 0) {
                int read = stream.Read(buffer, 0, MaxModuleBlock);
                length -= read;

                for (int i = 0; i < read; i++) {
                    sumA += buffer[i];
                    sumB += sumA;
                }

                sumA %= SumModulo;
                sumB %= SumModulo;
            }

            ArrayPool<byte>.Shared.Return(buffer);
            return sumA | (sumB << 16);
        }
    }
}
