//
//  Program.cs
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
using System.Diagnostics;

namespace Xdelta.Cli
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
                return;

            Stopwatch watcher = Stopwatch.StartNew();

            using (FileStream source = OpenForRead(args[0]))
            using (FileStream patch  = OpenForRead(args[1]))
            using (FileStream target = CreateForWriteAndRead(args[2]))
                new Decoder(source, patch, target).Run();

            watcher.Stop();
            Console.WriteLine("Done in {0}", watcher.Elapsed);
            Console.WriteLine("Press a key to quit...");
            Console.ReadKey(true);
        }

        private static FileStream OpenForRead(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                4096,   // Default buffer, it seems bigger does not affect time
                FileOptions.RandomAccess);
        }

        private static FileStream CreateForWriteAndRead(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Read,
                4096,   // Default buffer, it seems bigger does not affect time
                FileOptions.RandomAccess);
        }
    }
}
