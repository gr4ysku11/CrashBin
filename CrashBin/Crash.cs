using System;
using System.Collections.Generic;

namespace CrashBin;

public partial class Crash
{
    public long Id { get; set; }

    public string? Details { get; set; }

    public string? Exploitability { get; set; }

    public string? File { get; set; }

    public string? Hash { get; set; }
}
