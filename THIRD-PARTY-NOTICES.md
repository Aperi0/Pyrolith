# Third-party notices

Pyrolith uses the following open-source projects. Their licenses apply to the respective components; the full license texts are available at the linked repositories.

## Vestris.ResourceLib

- Used in: Pyrolith (GUI) — writes icons and version information into generated executables
- Version: 2.2.0
- License: MIT
- Copyright: Daniel Doubrovkine, Vestris Inc. and contributors
- Source: https://github.com/resourcelib/resourcelib

## DotNetZip

- Used in: Pyrolith (GUI) — creates the ZIP payload, optionally with WinZip AES-256 encryption
- Version: 1.16.0
- License: Microsoft Public License (Ms-PL)
- Copyright: Dino Chiesa, Henrik Feldt and contributors
- Source: https://github.com/haf/DotNetZip.Semverd

## SharpCompress

- Used in: PyrolithLoader — extracts the payload when a generated executable starts (compiled into every generated executable)
- Version: 0.50.5
- License: MIT
- Copyright: Adam Hathcock and contributors
- Source: https://github.com/adamhathcock/sharpcompress

## python-build-standalone

- Used in: downloaded at build time and included in every generated executable
- License: MPL-2.0 (build scripts); the distributions contain CPython (PSF License) and its bundled libraries (OpenSSL, SQLite, Tcl/Tk, zlib, …), each under its own license — see the license files inside the distribution
- Source: https://github.com/astral-sh/python-build-standalone

## .NET

- Used in: Pyrolith and PyrolithLoader
- License: MIT
- Source: https://github.com/dotnet/runtime, https://github.com/dotnet/winforms
