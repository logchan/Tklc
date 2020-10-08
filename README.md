## TKLC

The personal .Net library of logchan. Contains code shared by my personal projects. Released in hope that you find it useful.

> **T**asu**k**ete, **l**og**c**han!

### Highlights

In this repository, you can find in C#:

- An [scrypt implementation](src/Tklc.Cryptography/Scrypt.cs)
- An [SSE accerelated scrypt implementaion](src/Tklc.Cryptography.Intrinsics/ScryptSse.cs)
- A [GIF decoder](src/Tklc.Drawing/Gif) and a ready-to-use [WPF binding](src/Tklc.Wpf/Media/GifImage.cs)
- A [function](src/Tklc.IO/IOHelpers.Sorting.cs) that sorts file names in "natural order", so `TV Show Ep 2.mp4` comes before `TV Show Ep 10.mp4`
- A [function](src/Tklc.Json/JObjectSubtree.cs) that takes a subtree of a JSON object based on lambda expression navigation

And many more.

### Usage

Take any code you need :)

### License

The source code is published under the [DBAD](https://dbad-license.org/) public license.
