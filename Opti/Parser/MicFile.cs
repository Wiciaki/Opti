namespace Opti.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class MicFile : AsmFile<InstructionLine>
    {
        public MicFile(string[] input) : base(input, "Mic")
        { }

        public override IEnumerator<InstructionLine> GetEnumerator()
        {
            foreach (var line in this.Content.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                yield return InstructionLine.ParseMic(line);
            }
        }

        public override bool VerifyStructure()
        {
            try
            {
                if (this.Content[0].TrimEnd() != "Y0")
                {
                    return false;
                }

                for (var i = 1; i < this.Content.Count; i++)
                {
                    var line = this.Content[i];

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        InstructionLine.ParseMic(line);
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

        public void InsertInstruction(string instruction, string operations)
        {
            this.Content.Insert(this.GetIndex(this.Last()) + 1, InstructionLine.MakeMic(instruction, operations));
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            this.Content[this.GetIndex(instruction)] = InstructionLine.MakeMic(instruction, operations);
        }

        public void RemoveInstruction(string instruction)
        {
            this.Content.RemoveAt(this.GetIndex(instruction));
        }
    }
}