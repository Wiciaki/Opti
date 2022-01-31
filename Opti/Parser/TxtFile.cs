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

        protected int GetIndex(string instruction)
        {
            return this.Content.FindIndex(l =>
            {
                if (l.StartsWith(instruction))
                {
                    var c = l.Skip(instruction.Length).First();
                    return char.IsWhiteSpace(c) || c == '=' && char.IsUpper(instruction[0]);
                }

                return false;
            });
        }

        protected int GetOperationIndex(string instruction)
        {
            return this.Content.FindIndex(l =>
            {
                if (l.StartsWith(instruction))
                {
                    var c = l.Skip(instruction.Length).First();
                    return char.IsWhiteSpace(c) || c == ':' && char.IsLower(instruction[0]);
                }

                return false;
            });
        }

        protected override int GetIndex(InstructionLine line)
        {
            return this.GetIndex(line.Instruction);
        }

        public string[] GetOperationsForInstruction(string instruction)
        {
            return this.Single(line => line.Instruction == instruction).Operations;
        }

        public void InsertInstruction(string instruction, string operations)
        {
            this.Content.Insert(this.GetIndex(this.Last()) + 1, InstructionLine.MakeTxt(instruction, operations));
        }

        public void UpdateInstruction(string instruction, IEnumerable<string> operations)
        {
            this.UpdateInstruction(instruction, string.Join(' ', operations));
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            this.Content[this.GetIndex(instruction)] = InstructionLine.MakeTxt(instruction, operations);
        }

        public void RemoveInstruction(string instruction)
        {
            this.Content.RemoveAt(this.GetIndex(instruction));
        }

        public void RemoveOperation(string operation)
        {
            this.Content.RemoveAt(this.GetOperationIndex(operation));
        }

        public int RemoveOperations(Func<OperationLine, bool> predicate)
        {
            var indexes = this.GetOperations().Reverse().Where(predicate).Select(line => line.Instruction).Select(this.GetIndex).ToList();

            foreach (var i in indexes)
            {
                this.Content.RemoveAt(i);
            }

            return indexes.Count;
        }
    }
}