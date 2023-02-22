# CrashBin
CrashBin automates the process of triaging crash files produced during fuzzing campaigns. Features include crash verification, deduplication, exploitability analysis, and storing crash details into a convient local database.

## Installation

1. Install Debugging Tools, available from the WindowsSDK
2. Copy msec.dll into the Debugging Tools installation directory

## Running CrashBin

```
CrashBin.exe -in <crash file directory> -out <output directory> -- <application.exe>
```