namespace Opti.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TxtFile : AsmFile<InstructionLine>
    {
        public TxtFile(string[] input) : base(input, "Txt")
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

        public override bool VerifyStructure()
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

        public void InsertInstruction(string instruction, string operations)
        {
            var i = this.Content.FindIndex(line => line.StartsWith(this.Last().Instruction)) + 1;
            this.Content.Insert(i, InstructionLine.MakeTxt(instruction, operations));
        }

        public void UpdateInstruction(string instruction, IEnumerable<string> operations)
        {
            this.UpdateInstruction(instruction, string.Join(' ', operations));
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            var index = this.Content.FindIndex(line => line.StartsWith(instruction));
            this.Content[index] = InstructionLine.MakeTxt(instruction, operations);
        }

        public int RemoveInstruction(string instruction)
        {
            return this.Content.RemoveAll(line => line.StartsWith(instruction));
        }

        public string[] GetOperationsForInstruction(string instruction)
        {
            return this.SingleOrDefault(line => line.Instruction == instruction)?.Operations ?? Array.Empty<string>();
        }

        public int RemoveOperations(Func<OperationLine, bool> predicate)
        {
            var list = this.GetOperations().Reverse().Where(predicate).ToList();

            foreach (var i in list.Select(line => this.Content.FindIndex(l => l.StartsWith(line.Instruction))))
            {
                this.Content.RemoveAt(i);
            }

            return list.Count;
        }
    }
}