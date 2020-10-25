namespace Opti
{
    using System;

    public class GsaLine : AsmLine
    {
        public int Index { get; }

        public int First { get; }

        public int Second { get; }

        public GsaLine(string name, int index, int first, int second) : base(name)
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
    }
}
