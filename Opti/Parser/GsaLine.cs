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

        public static GsaLine Parse(string line)
        {
            var split = line.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            return new GsaLine(split[1], int.Parse(split[0]), int.Parse(split[2]), int.Parse(split[3]));
        }

        public static string Make(string instruction, int index, int first, int second)
        {
            var s = $"  {index,-4}{instruction,-7}{first,-4}{second,-4}";
            return s;
        }

        public override string ToString()
        {
            return $"GsaLine: [{Make(this.Instruction, this.Index, this.First, this.Second)}]";
        }

        public override bool Equals(object obj)
        {
            return obj is GsaLine line && this == line;
        }

        public override int GetHashCode() => this.Index;

        public static bool operator ==(GsaLine left, GsaLine right)
        {
            return left?.Index == right?.Index;
        }

        public static bool operator !=(GsaLine left, GsaLine right)
        {
            return !(left == right);
        }
    }
}