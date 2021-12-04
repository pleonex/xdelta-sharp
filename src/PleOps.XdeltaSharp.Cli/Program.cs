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
namespace PleOps.XdeltaSharp.Cli
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using PleOps.XdeltaSharp.Decoder;

    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
                return;

            using FileStream source = OpenForRead(args[0]);
            using FileStream patch = OpenForRead(args[1]);
            using FileStream target = CreateForWriteAndRead(args[2]);

            Stopwatch watcher = Stopwatch.StartNew();
            var decoder = new Decoder(source, patch, target);
            decoder.Run();

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
