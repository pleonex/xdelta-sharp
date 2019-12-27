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
using System.IO;

namespace Xdelta
{
    public static class Adler32
    {
        private const uint Base = 65521;    // Largest prime smaller than 65536
        private const uint Nmax = 5552;        // Nmax is the largest n such that 255n(n+1)/2 + (n+1)(Base-1) <= 2^32-1

        private static void DoTimes(ref uint adler, ref uint sum2, Stream buffer, int times)
        {
            while (times-- > 0) {
                adler += (byte)buffer.ReadByte();
                sum2  += adler;
            }
        }

        public static uint Run(uint adler, Stream buffer, uint length)
        {
            uint n;

            // Split Adler-32 into component sums
            uint sum2 = (adler >> 16) & 0xFFFF;
            adler &= 0xFFFF;

            // In case user likes doing a byte at a time, keep it fast
            if (length == 1) {
                adler += (byte)buffer.ReadByte();
                if (adler >= Base)
                    adler -= Base;

                sum2 += adler;
                if (sum2 >= Base)
                    sum2 -= Base;

                return adler | (sum2 << 16);
            }

            // In case short lengths are provided, keep it somewhat fast
            if (length < 16) {
                while (length-- > 0) {
                    adler += (byte)buffer.ReadByte();
                    sum2  += adler;
                }

                if (adler >= Base)
                    adler -= Base;

                sum2 %= Base;
                return adler | (sum2 << 16);
            }

            // Do length Nmax blocks -- requires just one module operation
            while (length >= Nmax) {
                length -= Nmax;
                n = Nmax / 16;                            // Nmax is divisible by 16
                do
                    DoTimes(ref adler, ref sum2, buffer, 16);    // 16 sums unrolled
                while (--n > 0);

                adler %= Base;
                sum2  %= Base;
            }

            // Do remaining bytes (less than Nmax, still just one module)
            if (length > 0) {    // avoid modules if none remaining
                while (length >= 16) {
                    length -= 16;
                    DoTimes(ref adler, ref sum2, buffer, 16);
                }

                while (length-- > 0) {
                    adler += (byte)buffer.ReadByte();
                    sum2 += adler;
                }

                adler %= Base;
                sum2  %= Base;
            }

            // return recombined sums
            return adler | (sum2 << 16);
        }
    }
}

