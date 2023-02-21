using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CrashBin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // parse command line params
            string inDir;
            string outDir;
            string targetApp;
            parseCmdLine(args, out inDir, out outDir, out targetApp);

            ProcessStartInfo pi = new ProcessStartInfo();

            //TODO: use env instead of hardcode path?
            pi.FileName = @"c:\program files (x86)\windows kits\10\debuggers\x86\cdb.exe";

            // loop through crash file directory
            foreach (string file in Directory.GetFiles(inDir))
            {
#if DEBUG
                // stop at initial breakpoint for testing
                pi.Arguments = $"-x -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {file}";
#else
            pi.Arguments = $"-x -g -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {file}";
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
        static void parseCmdLine(string[] args, out string inDir, out string outDir, out string targetApp)
        {
            inDir = "";
            outDir = "";
            targetApp = "";

            List<string> argsList = args.ToList();

            try
            {
                inDir = args[argsList.FindIndex(x => x.Equals("-in")) + 1];
                outDir = args[argsList.FindIndex(x => x.Equals("-out")) + 1];
                targetApp = args[argsList.FindIndex(x => x.Equals("--")) + 1];
            }
            catch (Exception ex)
            {
                printUsage();
            }
        }
        static void printUsage()
        {
            Console.WriteLine("Usage: CrashBin.exe -in <crash file directory> -out <output directory> -- <target application>");
        }
    }
}