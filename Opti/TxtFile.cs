namespace Opti
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TxtFile : AsmFile<InstructionLine>
    {
        public TxtFile(AsmCoordinator coordinator, string[] input) : base(coordinator, input, "Txt")
        { }

        public override IEnumerator<InstructionLine> GetEnumerator()
        {
            return this.GetInstructions().GetEnumerator();
        }

        public IEnumerable<InstructionLine> GetInstructions()
        {
            return this.Read(InstructionLine.ParseTxt, "instructions");
        }

        public IEnumerable<OperationLine> GetOperations()
        {
            return this.Read(OperationLine.Parse, "operations");
        }

        public IEnumerable<OperationLine> GetConditions()
        {
            return this.Read(OperationLine.Parse, "conditions", 1);
        }

        private IEnumerable<T> Read<T>(Func<string, T> parser, string word, int offset = 0)
        {
            var i = this.Content.FindIndex(s => s.ToLower().Contains(word)) + offset;

            while (true)
            {
                if (this.Content.Count <= ++i)
                {
                    break;
                }

                var line = this.Content[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                yield return parser(line);
            }
        }

        public override bool IsWellStructured()
        {
            try
            {
                var i = 0;

                void Section<T>(IEnumerable<T> enumerable, string name, int offset = 0)
                {
                    while (true)
                    {
                        var line = this.Content[i++];

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (!line.ToLower().Contains(name))
                        {
                            throw new Exception();
                        }

                        break;
                    }

                    i += enumerable.Count() + offset;
                }

                Section(this.GetInstructions(), "instructions");
                Section(this.GetOperations(), "operations");
                Section(this.GetConditions(), "conditions", 1);

                while (i < this.Content.Count)
                {
                    if (!string.IsNullOrWhiteSpace(this.Content[i++]))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}