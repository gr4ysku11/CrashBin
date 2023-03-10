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

            string cdbPath = "";
#if x64
            cdbPath = @"c:\program files (x86)\windows kits\10\debuggers\x64\cdb.exe";
#else
            cdbPath = @"c:\program files (x86)\windows kits\10\debuggers\x86\cdb.exe";
#endif
            startInfo.FileName= cdbPath;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            // get file list before db creation
            List<string> fileList = Directory.GetFiles(inDir).ToList();

            // create new crashbin db file
            //TODO: check for existing db?
            CrashbinContext crashbin = new CrashbinContext();
            crashbin.Database.EnsureCreated();

            // loop through crash file directory
            foreach (string file in fileList)
            {
                string fullFilePath = Path.GetFullPath(file);
#if DEBUG
                // stop at initial breakpoint for testing
                startInfo.Arguments = $"-x -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {fullFilePath}";
#else
                startInfo.Arguments = $"-x -g -c \".lastevent; r; kv8; !load msec; !exploitable; q\" {targetApp} {fullFilePath}";
#endif
                Process process = new Process();
                process.StartInfo = startInfo;
                process.Start();

                // use regex to grab specific details from cdb/exploitable
                string output = process.StandardOutput.ReadToEnd();
                string eip = $"0x{Regex.Match(output, "eip=([0-9a-f]{8})").Groups[1].Value}";
                string details = Regex.Match(output, @"Last event:.*?: (.*)quit:", RegexOptions.Singleline).Groups[1].Value;
                string exploitability = Regex.Match(output, @"Exploitability Classification: ([^\n]+)").Groups[1].Value;
                string hash = Regex.Match(output, @"Hash=([^\)]+)").Groups[1].Value;

                System.Console.WriteLine(details);

                // check for duplicates based on hash
                List<Crash> dupCrashes = crashbin.Crashes.Where(x => x.Hash == hash).ToList();

                if(dupCrashes.Count > 0 )
                    continue;

                // process new crash
                Crash crash = new Crash();
                crash.Details = details;
                crash.Exploitability = exploitability;
                crash.File = fullFilePath;
                crash.Hash = hash;

                // add crash to bin
                crashbin.Add(crash);
                crashbin.SaveChanges();

                // create output sub directories for exploitability classification (exploitable, unknown, etc)?
                Directory.CreateDirectory(outDir);
                Directory.CreateDirectory($"{outDir}/{exploitability}");

                // remove path prefix from file
                string trimFile = Path.GetFileName(file);

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