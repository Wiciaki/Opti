namespace Opti.Parser
{
    using System;

    public class GsaLine : AsmLine
    {
        public int Index { get; }

        public int First { get; }

        public int Second { get; }

        public GsaLine(string instruction, int index, int first, int second) : base(instruction)
        {
            this.Index = index;
            this.First = first;
            this.Second = second;
        }

        public override string ToString()
        {
            return $"GsaLine: [  {this.Index}\t{this.Instruction}\t{this.First}\t{this.Second}]";
        }

        public static GsaLine Parse(string line)
        {
            var split = line.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            return new GsaLine(split[1], int.Parse(split[0]), int.Parse(split[2]), int.Parse(split[3]));
        }

        //public static string Make(string instruction, int index, int first, int second)
        //{
        //    return $"{$"   {index} {instruction}",-12}{first,-7}{second}";
        //}
    }
}