namespace Opti
{
    using System;
    using System.Linq;

    public class InstructionLine : AsmLine
    {
        public string[] Instructions { get; }

        public InstructionLine(string name, string[] instructions) : base(name)
        {
            this.Instructions = instructions;
        }

        public static InstructionLine ParseTxt(string line)
        {
            var split = line.Split('=');

            if (split.Length != 2)
            {
                throw new ArgumentException();
            }

            var name = split[0].TrimEnd();
            var instructions = split[1].Trim().Split(null);

            return new InstructionLine(name, instructions);
        }

        public static InstructionLine ParseMic(string line)
        {
            var split = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length != 2)
            {
                throw new ArgumentException();
            }

            var name = split[0];
            var instructions = split.Skip(1).ToArray();

            return new InstructionLine(name, instructions);
        }
    }
}
