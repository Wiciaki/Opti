namespace Opti
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Optimizer
    {
        private readonly GsaFile gsa;

        private readonly TxtFile txt;

        private readonly MicFile mic;

        private static string[] FromPath(string path)
        {
            return File.ReadAllLines(path);
        }

        public Optimizer(string gsaPath, string txtPath, string micPath) : this(FromPath(gsaPath), FromPath(txtPath), FromPath(micPath))
        {

        }

        public Optimizer(string[] gsa, string[] txt, string[] mic)
        {
            this.gsa = new GsaFile(gsa);
            this.txt = new TxtFile(txt);
            this.mic = new MicFile(mic);
        }

        private string resultName;

        public string ResultName
        {
            get => this.resultName ?? "Result";
            set => this.resultName = value;
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

                var i = 2;

                while (true)
                {
                    yield return $"{name}_{i++}";
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

        public bool IsInputValid()
        {
            return true;
        }

        public int Optimize()
        {
            return 0;
        }
    }
}