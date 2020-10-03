namespace Opti
{
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AsmFile : IVerifiable
    {
        public string[] Input { get; }

        public string Name { get; }

        public string Extension { get; }

        protected List<string> Content { get; }

        protected AsmCoordinator Coordinator { get; }

        public string[] GetContent() => this.Content.ToArray();

        public abstract bool IsWellStructured();

        protected AsmFile(AsmCoordinator coordinator, string[] input, string name)
        {
            this.Coordinator = coordinator;
            this.Input = input;
            this.Content = input.ToList();
            this.Name = name;
            this.Extension = $".{name.ToLower()}";
        }
    }
}