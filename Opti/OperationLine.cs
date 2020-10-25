namespace Opti
{
    using System;
    using System.Linq;

    public class OperationLine
    {
        public string Instruction { get; }

        public string Operation { get; }

        public OperationLine(string instruction, string operation)
        {
            this.Instruction = instruction;
            this.Operation = operation;
        }

        public static OperationLine Parse(string line)
        {
            var split = new string(Array.FindAll(line.ToCharArray(), c => !char.IsWhiteSpace(c))).Split(':');

            return new OperationLine(split[0], string.Join(':', split.Skip(1)));
        }
    }
}
