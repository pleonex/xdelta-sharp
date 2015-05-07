//
//  Extensions.cs
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
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices
{
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class |
        AttributeTargets.Method)]
    internal class ExtensionAttribute : Attribute
    {
    }
}

namespace Xdelta
{
    internal static class EnumExtensions
    {
        /// <summary>
        /// Determines if actual contains a expected value.
        /// This is the same function as HasFlag for .NET 4 but we are targeting 2.0.
        /// The performance of this method is about 50 ms while the direct bitwise is 3 ms
        /// so don't use in critical-speed loops.
        /// </summary>
        /// <returns><c>true</c> if actual contains expected; otherwise, <c>false</c>.</returns>
        /// <param name="actual">Actual value.</param>
        /// <param name="expected">Expected value.</param>
        public static bool Contains(this Enum actual, Enum expected)
        {
            if (actual == null || expected == null)
                return false;

            if (!actual.GetType().IsInstanceOfType(expected))
                return false;

            ulong actualValue = Convert.ToUInt64(actual);
            ulong expectedValue = Convert.ToUInt64(expected);

            return (actualValue & expectedValue) != 0;
        }
    }

    internal static class UInt32Extensions
    {
        public static bool CheckOverflow(this uint address, uint length)
        {
            return length > (UInt32.MaxValue - address);
        }
    }
}

