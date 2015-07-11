# xdelta-sharp
<p align="center">
<a href="https://travis-ci.org/pleonex/xdelta-sharp"><img alt="Build Status" src="https://travis-ci.org/pleonex/xdelta-sharp.svg?branch=master" align="left" /></a>
<a href="http://www.gnu.org/copyleft/gpl.html"><img alt="license" src="https://img.shields.io/badge/license-GPL%20V3-blue.svg?style=flat" /></a>
<a href="https://github.com/fehmicansaglam/progressed.io"><img alt="progressed.io" src="http://progressed.io/bar/40" align="right" /></a>
</p>

<br>
<p align="center"><b>A port of XDELTA to C#.</b></p>

**xdelta-sharp** is a port of [xdelta](https://github.com/jmacd/xdelta) from
Joshua MacDonald. The original version is written in C and updated frequently.
This is a port to C# (a .NET language) with the objetive of being compatible in
many platforms with the same assembly.
*xdelta* is a compression tool to create delta/patch files from binary files using the algorithm `VCDIFF` described in the [RFC 3284](https://tools.ietf.org/html/rfc3284).

The current version supports **descompression** *without external compression* (default settings in *xdelta*).
It has been used in the [patcher tool](https://github.com/pleonex/Ninokuni/tree/master/Programs/NinoPatcher) for Ninokuni Spanish translation.
