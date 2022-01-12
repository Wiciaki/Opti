namespace Opti.Parser
{
    using System;
    using System.Linq;

    public class InstructionLine : AsmLine
    {
        public string[] Operations { get; }

        public InstructionLine(string instruction, string[] operations) : base(instruction)
        {
            this.Operations = operations;
        }

        public static InstructionLine ParseTxt(string line)
        {
            var split = line.Split('=');

            if (split.Length != 2)
            {
                throw new ArgumentException(null, nameof(line));
            }

            var instruction = split[0].TrimEnd();
            var operations = split[1].Trim().Split(null);

            return new InstructionLine(instruction, Array.FindAll(operations, o => !string.IsNullOrWhiteSpace(o)));
        }

        public static InstructionLine ParseMic(string line)
        {
            var split = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            var instruction = split[0];
            var operations = split.Skip(1).ToArray();

            return new InstructionLine(instruction, operations);
        }

        public override string ToString()
        {
            return $"InstructionLine: [{this.Instruction} = {string.Join(' ', this.Operations)}]";
        }

        //public static string MakeTxt(string instruction, string[] operations)
        //{
        //    return MakeTxt(instruction, string.Join(' ', operations));
        //}

        //public static string MakeTxt(string instruction, string operations)
        //{
        //    return $"{instruction} = {operations}";
        //}

        //public static string MakeMic(string instruction, string[] operations)
        //{
        //    return MakeMic(instruction, string.Join(' ', operations));
        //}

        //public static string MakeMic(string instruction, string operations)
        //{
        //    return $"{instruction}  {operations}";
        //}
    }
}
