namespace Opti
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AsmFile
    {
        public string[] Input { get; }

        public string Name { get; }

        public string Extension { get; }

        public string[] GetContent()
        {
            return this.Content.ToArray();
        }

        protected readonly List<string> Content;

        protected AsmFile(string name, string[] input)
        {
            this.Name = name;
            this.Extension = $".{name.ToLower()}";
            this.Input = input;
            this.Content = input.ToList();
        }
    }
}