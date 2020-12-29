# Pleosoft.XdeltaSharp ![Build and release](https://github.com/pleonex/xdelta-sharp/workflows/Build%20and%20release/badge.svg) ![GitHub](https://img.shields.io/github/license/pleonex/xdelta-sharp)

**NOTE: At this stage, this projects can only decompress patch files. It cannot
generate / compress.**

**xdelta-sharp** is a compression tool to create delta/patch files from binary
files using the algorithm `VCDIFF` described in the
[RFC 3284](https://tools.ietf.org/html/rfc3284).

This projects offers a library and a console application written in pure C# (no
calls to C libraries). Making it compatible in every OS that can run a .NET
runtime that implements .NET Standard 2.0.

<!-- prettier-ignore -->
| Release | Package                                                           |
| ------- | ----------------------------------------------------------------- |
| Stable  | [![Nuget](https://img.shields.io/nuget/v/Pleosoft.XdeltaSharp?label=nuget.org&logo=nuget)](https://www.nuget.org/packages/Pleosoft.XdeltaSharp) |
| Preview | [Azure Artifacts](https://dev.azure.com/pleonex/Pleosoft/_packaging?_a=feed&feed=Pleosoft-Preview) |

The project takes the name from [xdelta](https://github.com/jmacd/xdelta).
Another compression tool in C by Joshua MacDonald that implements the algorithm
VCDIFF. It started as a port but due to huge difference between C and C#,
dropped and restarted the development just by reading the specification.

## Limitations

The latest version does not support:

- Generate patch files.
- Patch files with external compression.

## Documentation

Feel free to ask any question in the
[project Discussion site!](https://github.com/pleonex/xdelta-sharp/discussions)

Check our on-line [API documentation](https://pleonex.dev/xdelta-sharp/).

## Build

The project requires to build .NET 5.0 SDK, .NET Core 3.1 runtime and .NET
Framework 4.8 or latest Mono. If you open the project with VS Code and you did
install the
[VS Code Remote Containers](https://code.visualstudio.com/docs/remote/containers)
extension, you can have an already pre-configured development environment with
Docker or Podman.

To build, test and generate artifacts run:

```sh
# Only required the first time
dotnet tool restore

# Default target is Stage-Artifacts
dotnet cake
```

To just build and test quickly, run:

```sh
dotnet cake --target=BuildTest
```

## License

This software is license under the
[MIT](https://choosealicense.com/licenses/mit/) license.

Although not used, originally the project was inspired in
[xdelta](https://github.com/jmacd/xdelta) with license
[Apache 2.0](https://spdx.org/licenses/Apache-2.0.html).

The algorithm for the ADLER32 checksum was ported from the C version of zlib
with the following license:

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
