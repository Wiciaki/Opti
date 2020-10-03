namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.IO;

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
            var command = new RootCommand(Resources.description_root)
                              {
                                  new Argument<string[]>("paths", Resources.description_paths),
                                  new Option<bool>("--verifyonly", Resources.description_verifyonly),
                                  new Option<string>("--outdir", GetDefaultOutdir, Resources.description_outdir),
                                  new Option<bool>("--print", Resources.description_print),
                                  new Option<bool>("--open", Resources.description_open)
                              };

            command.Handler = CommandHandler.Create<string[], bool, string, bool, bool>(InvokeOptimizer);

            var result = command.Invoke(args);

            if (result != 0 && IsDisappearing())
            {
                Console.WriteLine(Resources.press_to_continue);
                Console.ReadKey(true);
            }

            return result;
        }

        private static string GetDefaultOutdir()
        {
            return Path.Combine(Desktop.Value, "ASM_Optimized");
        }

        private static int InvokeOptimizer(string[] paths, bool verifyonly, string outdir, bool print, bool open)
        {
            paths ??= new[]
                          {
#if !DEBUG
                              Directory.GetCurrentDirectory()
#else
                              Path.Combine(Desktop.Value, "Studio")
#endif
                          };

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

            string GetPath(string ex)
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

            var gsa = GetPath("gsa");
            var txt = GetPath("txt");
            var mic = GetPath("mic");

            if (gsa == null || txt == null || mic == null)
            {
                return -1;
            }

            var optimizer = new AsmOptimizer(gsa, mic, txt);

            Console.WriteLine(optimizer.IsWellStructured() ? Resources.input_ok : Resources.input_err);

            if (verifyonly)
            {
                return 0;
            }

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
                var resultName = optimizer.GetResultNameForDirectory(outdir);

                foreach (var file in optimizer.Files())
                {
                    var fileName = resultName + file.Extension;
                    Console.WriteLine(Resources.info_file, fileName);
                    Print(string.Join(Environment.NewLine, file.GetContent()), ConsoleColor.DarkYellow);
                    Console.WriteLine();
                }
            }

            optimizer.SaveTo(outdir);
            Console.WriteLine(Resources.info_saved, outdir);

            if (open)
            {
                OpenExplorer(outdir);
            }

            return 0;
        }

        private static void Print(string message, ConsoleColor color)
        {
            var r = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = r;
        }

        private static void Error(string message, params object[] arguments)
        {
            Print("E: " + string.Format(message, arguments), ConsoleColor.Red);
        }

        private static void OpenExplorer(string directory)
        {
            Process.Start(new ProcessStartInfo { FileName = directory, Verb = "open", UseShellExecute = true });
        }

        private static bool IsDisappearing()
        {
            //var myId = Process.GetCurrentProcess().Id;
            //var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", myId);
            //var search = new ManagementObjectSearcher("root\\CIMV2", query);
            //var results = search.Get().GetEnumerator();
            //results.MoveNext();
            //var queryObj = results.Current;
            //var parentId = (uint)queryObj["ParentProcessId"];
            //var parent = Process.GetProcessById((int)parentId);
            //return parent.ProcessName == "explorer";

            return true;
        }
    }
}