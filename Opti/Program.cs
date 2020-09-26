namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    using Opti.Properties;

    using Process = System.Diagnostics.Process;

    /// <summary>
    /// The <c>Program</c> class. Handles all input as well as the IO.
    /// </summary>
    internal static class Program
    {
        private static readonly string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var command = new RootCommand(Resources.description_root)
                              {
                                  new Argument<string[]>("paths", Resources.description_paths),
                                  new Option<bool>("--verifyonly", Resources.description_verifyonly),
                                  new Option<string>("--outdir", GetDefaultOutdir, Resources.description_outdir),
                                  new Option<bool>("--print", Resources.description_print),
                                  new Option<bool>("--open", Resources.description_open)
                              };

            command.Handler = CommandHandler.Create<string[], bool, string, bool, bool>(Run);

            var result = command.Invoke(args);

            if (result != 0)
            {
                Console.WriteLine(Resources.press_to_continue);
                Console.ReadKey(true);
            }

            return result;
        }

        private static string GetDefaultOutdir()
        {
            return Path.Combine(Desktop, "ASM_Optimized");
        }

        private static int Run(string[] paths, bool verifyonly, string outdir, bool print, bool open)
        {
            paths ??= new[]
                          {
#if !DEBUG
                              Directory.GetCurrentDirectory()
#else
                              Path.Combine(Desktop, "Studio")
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

            string GetPath(string ext)
            {
                var l = files.FindAll(f => Path.GetExtension(f) == ext);

                switch (l.Count)
                {
                    case 1:
                        return l[0];
                    case 0:
                        Error(string.Format(Resources.err_input_invalid, ext));
                        return null;
                    default:
                        Error(string.Format(Resources.err_input_mulltiple, ext));
                        return null;
                }
            }

            Optimizer optimizer;

            try
            {
                optimizer = new Optimizer(GetPath(".gsa"), GetPath(".txt"), GetPath(".mic"));
            }
            catch
            {
                return -1;
            }

            Console.WriteLine(optimizer.IsInputValid() ? Resources.input_ok : Resources.input_err);

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

            var optimized = optimizer.Optimize();

            Console.WriteLine(Resources.elements_optimized, optimized);

            if (print)
            {
                Console.WriteLine();
                var resultName = optimizer.GetResultNameForDirectory(outdir);

                foreach (var file in optimizer.Files())
                {
                    Console.WriteLine(resultName + file.Extension);
                    Print(string.Join(Environment.NewLine, file.GetContent()), ConsoleColor.DarkYellow);
                    Console.WriteLine();
                }
            }

            optimizer.SaveTo(outdir);

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

        private static void Error(string message)
        {
            Print("E: " + message, ConsoleColor.Red);
        }

        private static void OpenExplorer(string directory)
        {
            Process.Start(new ProcessStartInfo { FileName = directory, Verb = "open", UseShellExecute = true });
        }
    }
}