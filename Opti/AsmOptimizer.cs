namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Opti.Optimizations;
    using Opti.Parser;

    public class AsmOptimizer
    {
        public AsmFiles Files { get; }

        public List<Optimization> Optimizations { get; }

        private readonly string resultName;

        public AsmOptimizer(string gsaPath, string txtPath, string micPath, string resultName) : this(Read(gsaPath), Read(txtPath), Read(micPath), resultName)
        { }

        public AsmOptimizer(string[] gsa, string[] txt, string[] mic, string resultName)
        {
            this.resultName = resultName;

            this.Files = new AsmFiles(gsa, txt, mic);
            this.Optimizations = Optimization.LoadDefault(this.Files);
        }

        private static string[] Read(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return File.ReadAllLines(path);
        }

        public void SaveTo(string directory)
        {
            var name = this.GetResultName(directory);

            foreach (var file in this.Files)
            {
                File.WriteAllLines(Path.Combine(directory, name + file.Extension), file.GetContent());
            }
        }

        public string GetResultName(string directory)
        {
            IEnumerable<string> Names()
            {
                yield return this.resultName;

                for (var i = 2; i < int.MaxValue; i++)
                {
                    yield return $"{this.resultName}_{i}";
                }
            }

            var hashset = new HashSet<string>(Directory.EnumerateFiles(directory).Select(Path.GetFileNameWithoutExtension));

            return Names().First(hashset.Add);
        }

        public int Optimize(Action<int, int> onPassCallback = null, int maxPassCount = int.MaxValue)
        {
            var count = 0;

            for (var i = 1; i <= maxPassCount; ++i)
            {
                var optimized = this.Optimizations.Sum(optimization => optimization.Perform());

                if (optimized == 0)
                {
                    this.Files.Gsa.FixIndexing();
                    break;
                }

                count += optimized;
                onPassCallback?.Invoke(i, optimized);
            }

            return count;
        }
    }
}