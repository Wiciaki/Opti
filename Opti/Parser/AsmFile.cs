namespace Opti.Parser
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AsmFile<TLine> : IAsmFile, IEnumerable<TLine> where TLine : AsmLine
    {
        public string[] Input { get; }

        public string Name { get; }

        public string Extension { get; }

        public string[] GetContent() => this.Content.ToArray();

        public abstract bool VerifyStructure();

        protected List<string> Content { get; }

        public abstract IEnumerator<TLine> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        protected AsmFile(string[] input, string name)
        {
            this.Input = input;
            this.Content = input.ToList();
            this.Name = name;
            this.Extension = $".{name.ToLower()}";
        }
    }
}