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
namespace PleOps.XdeltaSharp.Decoder
{
    using System;
    using System.IO;
    using PleOps.XdeltaSharp.Vcdiff;

    public class Decoder : IDisposable
    {
        public Decoder(Stream input, Stream patch, Stream output)
        {
            Input = input;
            Patch = patch;
            Output = output;

            HeaderReader headerReader = new HeaderReader();
            Header = headerReader.Read(patch);
        }

        public event ProgressChangedHandler ProgressChanged;

        public event FinishedHandler Finished;

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

        public Header Header {
            get;
            private set;
        }

        public Window LastWindow {
            get;
            private set;
        }

        public void Run()
        {
            WindowReader windowReader = new WindowReader(Patch, Header);
            WindowDecoder windowDecoder = new WindowDecoder(Input, Output);

            // Decode windows until there are no more bytes to process
            while (Patch.Position < Patch.Length) {
                DisposeLastWindow();

                Window window = windowReader.Read();
                LastWindow = window;

                windowDecoder.Decode(window);
                OnProgressChanged(1.0 * Patch.Position / Patch.Length);
            }

            OnFinished();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                DisposeLastWindow();
                Input = null;
                Output = null;
                Patch = null;
                Header = null;
            }
        }

        private void DisposeLastWindow()
        {
            if (LastWindow != null) {
                LastWindow.Data.BaseStream.Dispose();
                LastWindow.Data = null;

                LastWindow.Instructions.BaseStream.Dispose();
                LastWindow.Instructions = null;

                LastWindow.Addresses.BaseStream.Dispose();
                LastWindow.Addresses = null;

                LastWindow = null;
            }
        }

        private void OnProgressChanged(double progress)
        {
            if (ProgressChanged != null)
                ProgressChanged(progress);
        }

        private void OnFinished()
        {
            if (Finished != null)
                Finished();
        }
    }
}
