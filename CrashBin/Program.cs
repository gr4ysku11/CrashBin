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

            ProcessStartInfo startInfo = new ProcessStartInfo();

            //TODO: use env instead of hardcode path?
            startInfo.FileName = @"c:\program files (x86)\windows kits\10\debuggers\x86\cdb.exe";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            // create new crashbin db file
            //TODO: check for existing db?
            CrashbinContext crashbin = new CrashbinContext();
            crashbin.Database.EnsureCreated();

            // loop through crash file directory
            foreach (string file in Directory.GetFiles(inDir))
            {
#if DEBUG
                // stop at initial breakpoint for testing
                startInfo.Arguments = $"-x -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {file}";
#else
                startInfo.Arguments = $"-x -g -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {file}";
#endif
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                Process p = Process.Start(pi);

                // use regex to grab specific details from cdb/exploitable
                string output = process.StandardOutput.ReadToEnd();
                string eip = $"0x{Regex.Match(output, "eip=([0-9a-f]{8})").Groups[1].Value}";
                string details = Regex.Match(output, @"Last event:.*?: (.*)quit:", RegexOptions.Singleline).Groups[1].Value;
                string exploitability = Regex.Match(output, @"Exploitability Classification: ([^\n]+)").Groups[1].Value;
                string hash = Regex.Match(output, @"Hash=([^\)]+)").Groups[1].Value;

                System.Console.WriteLine(output);

                // check for duplicates based on hash
                List<Crash> dupCrashes = crashbin.Crashes.Where(x => x.Hash == hash).ToList();

                if(dupCrashes.Count > 0 )
                    continue;

                // process new crash
                Crash crash = new Crash();
                crash.Details = details;
                crash.Exploitability = exploitability;
                crash.File = file;
                crash.Hash = hash;

                // add crash to bin
                crashbin.Add(crash);
                crashbin.SaveChanges();

                // create output sub directories for exploitability classification (exploitable, unknown, etc)?
                Directory.CreateDirectory(outDir);
                Directory.CreateDirectory($"{outDir}/{exploitability}");

                // remove path prefix from file
                string trimFile = file.Substring(file.LastIndexOf('\\')+1);

                // copy dedup crash files to output directory
                File.Copy(file, $"{outDir}/{exploitability}/{trimFile}");
            }
        }
        static void parseCmdLine(string[] args, out string inDir, out string outDir, out string targetApp)
        {
            inDir = "";
            outDir = "";
            targetApp = "";

            // flatten args array into one string for simple regex matching
            string cmdLine = String.Join(" ", args);

            try
            {
                inDir = Regex.Match(cmdLine, @"-in ([^ ]+)").Groups[1].Value;
                outDir = Regex.Match(cmdLine, @"-out ([^ ]+)").Groups[1].Value;
                targetApp = Regex.Match(cmdLine, @"-- ([^ ]+)").Groups[1].Value;

                if(inDir == "" || outDir == "" || targetApp == "")
                    throw new Exception();
            }
            catch (Exception ex)
            {
                printUsage();
                System.Environment.Exit(0);
            }
        }
        static void printUsage()
        {
            Console.WriteLine("Usage: CrashBin.exe -in <crash file directory> -out <output directory> -- <target application>");
        }
    }
}