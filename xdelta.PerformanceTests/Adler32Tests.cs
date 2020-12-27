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
namespace Xdelta.PerformanceTests
{
    using System.IO;
    using BenchmarkDotNet.Attributes;

    public class Adler32Tests
    {
        Stream stream;

        [Params(2 * 1024, 512 * 1024, 4 * 1024 * 1024)]
        public uint Length { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            stream = new MemoryStream();
            byte[] data = new byte[Length];
            for (int i = 0; i < Length; i++) {
                data[i] = (byte)(i % 256);
            }

            stream.Write(data, 0, data.Length);
        }

        [GlobalCleanup]
        public void CleanUp()
        {
            stream.Dispose();
        }

        [Benchmark]
        public uint Adler32Run()
        {
            stream.Position = 0;
            return Adler32.Run(1, stream, Length);
        }
    }
}
