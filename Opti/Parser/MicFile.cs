namespace Opti.Parser
{
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

        public void InsertInstruction(string instruction, string[] operations)
        {
            var i = this.Content.FindIndex(line => line.Contains(this.Last().Instruction)) + 1;
            this.Content.Insert(i, $"{instruction}   {string.Join(' ', operations)}");
        }

        public int RemoveInstruction(string instruction)
        {
            return this.Content.RemoveAll(line => line.StartsWith(instruction));
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            var index = this.Content.FindIndex(line => line.Contains(instruction));
            var line = this.Content[index];
            var parsed = InstructionLine.ParseMic(line);

            this.Content[index] = line[..line.IndexOf(parsed.Operations[0])] + operations;
        }
    }
}