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
    public class Decoder : IDisposable
	{
		public Decoder(Stream input, Stream patch, Stream output)
		{
			Input  = input;
			Patch  = patch;
			Output = output;

            HeaderReader headerReader = new HeaderReader();
            Header = headerReader.Read(patch);
		}

        ~Decoder()
        {
            Dispose(false);
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

        public Header Header {
            get;
            private set;
        }

        public Window LastWindow {
            get;
            private set;
        }

        public event ProgressChangedHandler ProgressChanged;
        public event FinishedHandler Finished;

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

        private void Dispose(bool disposing)
        {
            if (disposing) {
                DisposeLastWindow();
                Input  = null;
                Output = null;
                Patch  = null;
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