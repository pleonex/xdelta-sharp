//
//  Adler32.cs
//
//  Author:
//       Benito Palacios <benito356@gmail.com>
//       Original code in C form Zlib by Mark Adler 1995-1998
//       https://github.com/madler/zlib/blob/50893291621658f355bc5b4d450a8d06a563053d/adler32.c
//
//  Copyright (c) 2015 Benito Palacios
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
	public static class Adler32
	{
		private const uint Base = 65521;	// Largest prime smaller than 65536
		private const uint Nmax = 5552;		// Nmax is the largest n such that 255n(n+1)/2 + (n+1)(Base-1) <= 2^32-1

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
				n = Nmax / 16;							// Nmax is divisible by 16
				do
					DoTimes(ref adler, ref sum2, buffer, 16);	// 16 sums unrolled
				while (--n > 0);

				adler %= Base;
				sum2  %= Base;
			}

			// Do remaining bytes (less than Nmax, still just one module)
			if (length > 0) {	// avoid modules if none remaining
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

