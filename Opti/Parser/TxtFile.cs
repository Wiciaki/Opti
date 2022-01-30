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
            return this.GetIndex(this.Single(line => line.Instruction == instruction));
        }

        protected override int GetIndex(InstructionLine line)
        {
            var index = this.Content.FindIndex(l => l.StartsWith(line.Instruction) && char.IsWhiteSpace(l.Skip(line.Instruction.Length).First()));

            if (index == -1)
                throw new Exception();
            
            return index;
        }

        protected int GetIndex(OperationLine line)
        {
            var index = this.Content.FindIndex(l => l.StartsWith(line.Instruction) && char.IsWhiteSpace(l.Skip(line.Instruction.Length).First()));

            if (index == -1)
                throw new Exception();

            return index;
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

        public int RemoveOperations(Func<OperationLine, bool> predicate)
        {
            var instructions = this.GetOperations().Reverse().Where(predicate).ToList();

            foreach (var i in instructions.Select(this.GetIndex))
            {
                this.Content.RemoveAt(i);
            }

            return instructions.Count;
        }
    }
}