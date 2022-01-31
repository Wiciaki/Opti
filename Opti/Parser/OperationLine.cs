namespace Opti.Parser
{
    using System;
    using System.Linq;

    public class OperationLine : AsmLine
    {
        public string Operation { get; }

        public OperationLine(string instruction, string operation) : base(instruction)
        {
            this.Operation = operation;
        }

        public static OperationLine Parse(string line)
        {
            var split = new string(Array.FindAll(line.ToCharArray(), c => !char.IsWhiteSpace(c))).Split(':');

            return new OperationLine(split[0], string.Join(':', split.Skip(1)));
        }

        public override string ToString()
        {
            return $"[{this.Instruction}\t: {this.Operation}]";
        }
    }
}