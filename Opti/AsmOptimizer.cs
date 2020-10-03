namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class AsmOptimizer : IVerifiable
    {
        public string ResultName { get; set; } = "Result";

        private readonly AsmCoordinator coordinator;

        private readonly GsaFile gsa;

        private readonly TxtFile txt;

        private readonly MicFile mic;

        private static string[] FromPath(string path)
        {
            return File.ReadAllLines(path);
        }

        public AsmOptimizer(string gsaPath, string txtPath, string micPath) : this(FromPath(gsaPath), FromPath(txtPath), FromPath(micPath))
        {

        }

        public AsmOptimizer(string[] gsa, string[] txt, string[] mic)
        {
            var coordinator = new AsmCoordinator(/**/);

            this.gsa = new GsaFile(coordinator, gsa);
            this.txt = new TxtFile(coordinator, txt);
            this.mic = new MicFile(coordinator, mic);

            this.coordinator = coordinator;
        }

        public IEnumerable<AsmFile> Files()
        {
            yield return this.gsa;
            yield return this.txt;
            yield return this.mic;
        }

        public string GetResultNameForDirectory(string directory)
        {
            IEnumerable<string> Names()
            {
                var name = this.ResultName;
                yield return name;

                for (var i = 2; i < int.MaxValue; i++)
                {
                    yield return $"{name}_{i}";
                }
            }

            var hashset = new HashSet<string>(Directory.EnumerateFiles(directory).Select(Path.GetFileNameWithoutExtension));

            return Names().First(hashset.Add);
        }

        public void SaveTo(string directory)
        {
            var name = this.GetResultNameForDirectory(directory);

            foreach (var file in this.Files())
            {
                File.WriteAllLines(Path.Combine(directory, name + file.Extension), file.GetContent());
            }
        }

        public bool IsWellStructured()
        {
            return this.Files().Cast<IVerifiable>().Prepend(coordinator).All(file => file.IsWellStructured());
        }

        public int Optimize()
        {
            return 0;
        }
    }
}