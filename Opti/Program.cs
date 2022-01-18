namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.IO;
    using System.Threading.Tasks;

    using Resources = Properties.Resources;

    /// <summary>
    /// The <c>Program</c> class. Handles all input as well as the IO.
    /// </summary>
    internal static class Program
    {
        private const string DEFAULT_OUTDIR = "ASM_Optimized";

        private const string DEFAULT_RESULTNAME = "Result";

        private const string DEFAULT_SEARCHFOLDER = "Studio";

        private static async Task<int> Main(string[] args)
        {
            var paths = new Argument<string[]>("paths", Resources.description_paths);
            var outdir = new Option<string>("--outdir", GetDefaultOutdir, Resources.description_outdir);
            var resultName = new Option<string>("--resultName", GetDefaultResultName, Resources.description_resultname);
            var print = new Option<bool>("--print", Resources.description_print);

            var command = new RootCommand(Resources.description_root) { paths, outdir, resultName, print };
            command.SetHandler((Action<string[], string, string, bool>)InvokeOptimizer, paths, outdir, resultName, print);

            return await command.InvokeAsync(args);
        }

        private static readonly Lazy<string> Desktop = new Lazy<string>(() => Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

        private static string GetDefaultOutdir() => Path.Combine(Desktop.Value, DEFAULT_OUTDIR);

        private static string GetDefaultResultName() => DEFAULT_RESULTNAME;

        private static string GetDefaultSearchFolder()
        {
#if !DEBUG
            return Directory.GetCurrentDirectory();
#else
            return Path.Combine(Desktop.Value, DEFAULT_SEARCHFOLDER);
#endif
        }

        private static void InvokeOptimizer(string[] paths, string outdir, string resultName, bool print)
        {
            if (paths.Length == 0)
            {
                paths = new[] { GetDefaultSearchFolder() };
            }

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
                return;
            }

            var optimizer = new AsmOptimizer(gsa, txt, mic, resultName);

            if (!optimizer.Files.VerifyStructure())
            {
                Info(Resources.input_err);
                return;
            }

            Info(Resources.input_ok);

            try
            {
                Directory.CreateDirectory(outdir);
            }
            catch
            {
                Error(Resources.err_outdir_invalid);
                return;
            }

            Info(Resources.info_optimized, optimizer.Optimize(OnPassCallback));

            var name = optimizer.GetResultName(outdir);

            if (print)
            {
                foreach (var file in optimizer.Files)
                {
                    var fileName = name + file.Extension;
                    Info(Resources.info_file, fileName);
                    Print(ConsoleColor.DarkYellow, string.Join(Environment.NewLine, file.GetContent()));
                    Console.WriteLine();
                }
            }

            optimizer.SaveTo(outdir);
            Info(Resources.info_saved, outdir, name);
        }

        private static void OnPassCallback(int optimizedInPass, int optimizedTotal)
        {
            Info(Resources.info_pass, optimizedInPass, optimizedTotal);
        }

        public static void Error(string message, params object[] arguments) => Print(ConsoleColor.Red, "E: " + message, arguments);

        public static void Info(string message, params object[] arguments) => Print(ConsoleColor.Yellow, "I: " + message, arguments);

        private static void Print(ConsoleColor color, string message, params object[] arguments)
        {
            var tmp = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message, arguments);
            Console.ForegroundColor = tmp;
        }
    }
}