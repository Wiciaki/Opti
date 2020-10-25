namespace Opti
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AsmOptimizer
    {
        public AsmCoordinator Coordinator { get; }

        public List<Optimization> Optimizations { get; }

        private readonly string resultName;

        private bool? valid;

        public AsmOptimizer(string gsaPath, string txtPath, string micPath, string resultName) : this(Read(gsaPath), Read(txtPath), Read(micPath), resultName)
        {

        }

        public AsmOptimizer(string[] gsa, string[] txt, string[] mic, string resultName)
        {
            this.resultName = resultName;

            this.Coordinator = new AsmCoordinator(gsa, txt, mic);
            this.Optimizations = new List<Optimization>();
        }

        private static string[] Read(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            return File.ReadAllLines(path);
        }

        public bool IsInputValid => valid ??= this.Coordinator.IsWellStructured();

        public void SaveTo(string directory)
        {
            var name = this.GetResultName(directory);

            foreach (var file in this.Coordinator.Files())
            {
                File.WriteAllLines(Path.Combine(directory, name + file.Extension), file.GetContent());
            }
        }

        public int Optimize()
        {
            return 0;
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
    }
}