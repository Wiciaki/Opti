namespace Opti
{
    using System.Collections.Generic;
    using System.Linq;

    public class AsmCoordinator : IVerifiable
    {
        public GsaFile Gsa { get; }

        public TxtFile Txt { get; }

        public MicFile Mic { get; }

        private readonly bool success;

        private readonly Dictionary<string, string> symbols;

        public IEnumerable<IAsmFile> Files()
        {
            yield return this.Gsa;
            yield return this.Txt;
            yield return this.Mic;
        }

        public bool IsWellStructured()
        {
            return this.success && this.Files().All(file => file.IsWellStructured());
        }

        public AsmCoordinator(string[] gsa, string[] txt, string[] mic)
        {
            this.Gsa = new GsaFile(this, gsa);
            this.Txt = new TxtFile(this, txt);
            this.Mic = new MicFile(this, mic);

            (this.symbols, this.success) = this.ParseSymbols();
        }

        private (Dictionary<string, string>, bool) ParseSymbols()
        {
            var dict = new Dictionary<string, string>();
            var success = true;



            return (dict, success);
        }
    }
}