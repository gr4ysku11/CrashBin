using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CrashBin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TODO: parse command line params
            // separate params from target application

            //TODO: read input directory and loop through crash files

            ProcessStartInfo pi = new ProcessStartInfo();

            //TODO: use env instead of hardcode path?
            pi.FileName = @"c:\program files (x86)\windows kits\10\debuggers\x86\cdb.exe";

#if DEBUG
            // stop at initial breakpoint for testing
            pi.Arguments = $"-x -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {args[0]}";
#else
            pi.Arguments = $"-x -g -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {args[0]}";
#endif

            pi.RedirectStandardOutput = true;
            pi.UseShellExecute = false;

            Process p = Process.Start(pi);

            string output = p.StandardOutput.ReadToEnd();
            string eip = $"0x{Regex.Match(output, "eip=([0-9a-f]{8})").Groups[1].Value}";
            string details = Regex.Match(output, @"Last event:.*?: (.*)quit:", RegexOptions.Singleline).Groups[1].Value;
            string exploitability = Regex.Match(output, @"Exploitability Classification: ([^\n]+)").Groups[1].Value;
            string hash = Regex.Match(output, @"Hash=([^\)]+)").Groups[1].Value;

            System.Console.WriteLine(output);

            //TODO: add sqlite processing
            // create db
            // check for duplicates
            // store details

            //TODO: copy dedup crash files to output directory
            // create output sub directories for exploitability classification (exploitable, unknown, etc)?
        }
    }
}