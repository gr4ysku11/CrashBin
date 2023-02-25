# CrashBin
CrashBin automates the process of triaging crash files produced during fuzzing campaigns. Features include crash verification, deduplication, exploitability analysis, and storing crash details into a convient local database.

## Installation
1. Install the latest [.NET Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
2. Install Debugging Tools, available from the [WindowsSDK](https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
3. Copy [msec.dll](https://github.com/gr4ysku11/MSECExtensions) into the Debugging Tools installation directory

## Running CrashBin

```
CrashBin.exe -in <crash file directory> -out <output directory> -- <application.exe>
```
