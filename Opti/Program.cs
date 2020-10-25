namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Runtime.InteropServices;
    
    using Opti.Properties;

    using Process = System.Diagnostics.Process;

    /// <summary>
    /// The <c>Program</c> class. Handles all input as well as the IO.
    /// </summary>
    internal static class Program
    {
        private static readonly Lazy<string> Desktop = new Lazy<string>(() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        private static int Main(string[] args)
        {
#if DEBUG
            Console.Write("opti: ");
            Print(ConsoleColor.Cyan, "running in DEBUG mode");
#endif

            var command = new RootCommand(Resources.description_root)
                              {
                                  new Argument<string[]>("paths", Resources.description_paths),
                                  new Option<bool>("--verifyonly", Resources.description_verifyonly),
                                  new Option<string>(new[] { "--outdir", "--output" }, GetDefaultOutdir, Resources.description_outdir),
                                  new Option<string>("--resultName", GetDefaultResultName, Resources.description_resultname),
                                  new Option<bool>("--print", Resources.description_print),
                                  new Option<bool>("--open", Resources.description_open)
                              };

            command.Handler = CommandHandler.Create<string[], bool, string, string, bool, bool>(InvokeOptimizer);

            var result = command.Invoke(args);

            if (result != 0 && IsDisappearing())
            {
                Console.WriteLine(Resources.press_to_continue);
                Console.ReadKey(true);
            }

            return result;
        }

        private static string GetDefaultOutdir() => Path.Combine(Desktop.Value, "ASM_Optimized");

        private static string GetDefaultResultName() => "Result";

        private static string GetDefaultSearchFolder()
        {
#if !DEBUG
            return Directory.GetCurrentDirectory();
#else
            return Path.Combine(Desktop.Value, "Studio");
#endif
        }

        private static int InvokeOptimizer(string[] paths, bool verifyonly, string outdir, string resultName, bool print, bool open)
        {
            paths ??= new[] { GetDefaultSearchFolder() };

            var files = new List<string>();

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    files.Add(path);
                }
                else if (Directory.Exists(path))
                {
                    files.AddRange(Directory.GetFiles(path));
                }
            }

            string GetPathForExtension(string ex)
            {
                ex = '.' + ex;

                var l = files.FindAll(f => Path.GetExtension(f) == ex);

                switch (l.Count)
                {
                    case 1:
                        return l[0];
                    case 0:
                        Error(Resources.err_input_invalid, ex);
                        return null;
                    default:
                        Error(Resources.err_input_multiple, ex);
                        return null;
                }
            }

            var gsa = GetPathForExtension("gsa");
            var txt = GetPathForExtension("txt");
            var mic = GetPathForExtension("mic");

            if (gsa == null || txt == null || mic == null)
            {
                return -1;
            }

            var optimizer = new AsmOptimizer(gsa, txt, mic, resultName);
            Console.WriteLine(optimizer.IsInputValid ? Resources.input_ok : Resources.input_err);

            if (!optimizer.IsInputValid)
            {
                return -1;
            }

            if (!verifyonly)
            {
                try
                {
                    Directory.CreateDirectory(outdir);
                }
                catch
                {
                    Error(Resources.err_outdir_invalid);
                    return -1;
                }

                Console.WriteLine(Resources.info_optimized, optimizer.Optimize());

                if (print)
                {
                    var name = optimizer.GetResultName(outdir);

                    foreach (var file in optimizer.Coordinator.Files())
                    {
                        var fileName = name + file.Extension;
                        Console.WriteLine(Resources.info_file, fileName);
                        Print(ConsoleColor.DarkYellow, string.Join(Environment.NewLine, file.GetContent()));
                        Console.WriteLine();
                    }
                }

                optimizer.SaveTo(outdir);
                Console.WriteLine(Resources.info_saved, outdir);

                if (open)
                {
                    OpenExplorer(outdir);
                }
            }

            return 0;
        }

        private static void Print(ConsoleColor color, string message, params object[] arguments)
        {
            var tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message, arguments);
            Console.ForegroundColor = tmp;
        }

        public static void Error(string message, params object[] arguments) => Print(ConsoleColor.Red, "E: " + message, arguments);

        public static void Info(string message, params object[] arguments) => Print(ConsoleColor.Yellow, "I: " + message, arguments);

        private static void OpenExplorer(string directory)
        {
            Process.Start(new ProcessStartInfo { FileName = directory, Verb = "open", UseShellExecute = true });
        }

        private static bool IsDisappearing()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var query = "SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = " + Process.GetCurrentProcess().Id;
                var mos = new ManagementObjectSearcher(@"root\CIMV2", query);

                var processId = (int)(uint)mos.Get().Cast<ManagementBaseObject>().First()["ParentProcessId"];
                var processName = Process.GetProcessById(processId).ProcessName;

                Console.WriteLine(processName);
                return processName == "explorer";
            }

            return true;
        }
    }
}