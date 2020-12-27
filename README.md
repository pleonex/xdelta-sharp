# xdelta-sharp ![Build and test](https://github.com/pleonex/xdelta-sharp/workflows/Build%20and%20test/badge.svg) ![GitHub](https://img.shields.io/github/license/pleonex/xdelta-sharp)

**NOTE: At this stage, this projects can only decompress patch files. It cannot generate / compress.**

**xdelta-sharp** is a compression tool to create delta/patch files from binary
files using the algorithm `VCDIFF` described in the [RFC 3284](https://tools.ietf.org/html/rfc3284).

This projects offers a library and a console application written in pure C# (no calls to C libraries).
Making it compatible in every OS that can run a .NET runtime that implements .NET Standard 2.0.

The project takes the name from [xdelta](https://github.com/jmacd/xdelta).
Another compression tool in C by Joshua MacDonald that implements the algorithm VCDIFF.
It started as a port but due to huge difference between C and C#, dropped and
restarted the development just by reading the specification.

## Limitations

The latest version does not support:

* Generate patch files.
* Patch files with external compression.

## License

This software is license under the [MIT](https://choosealicense.com/licenses/mit/) license.

Although not used, originally the project was inspired in [xdelta](https://github.com/jmacd/xdelta)
with license [Apache 2.0](https://spdx.org/licenses/Apache-2.0.html).

The algorithm for the ADLER32 checksum was ported from the C version of zlib with the following license:

```plain
Copyright (C) 1995-2013 Jean-loup Gailly and Mark Adler

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not
    claim that you wrote the original software. If you use this software
    in a product, an acknowledgment in the product documentation would be
    appreciated but is not required.

2. Altered source versions must be plainly marked as such, and must not be
    misrepresented as being the original software.

3. This notice may not be removed or altered from any source distribution.

Jean-loup Gailly        Mark Adler
jloup@gzip.org          madler@alumni.caltech.edu

The data format used by the zlib library is described by RFCs (Request for
Comments) 1950 to 1952 in the files http://tools.ietf.org/html/rfc1950
(zlib format), rfc1951 (deflate format) and rfc1952 (gzip format).
```
