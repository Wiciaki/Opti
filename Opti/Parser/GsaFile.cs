namespace Opti.Parser
{
    using System.Collections.Generic;
    using System.Linq;

    public class GsaFile : AsmFile<GsaLine>
    {
        public GsaFile(string[] input) : base(input, "Gsa")
        { }

        public int GetNumber()
        {
            return int.Parse(this.Content[0].TrimEnd());
        }

        public void SetNumber(int value)
        {
            this.Content[0] = value.ToString();
        }

        public override IEnumerator<GsaLine> GetEnumerator()
        {
            return this.Content.Skip(1).Take(this.GetNumber() + 1).Select(GsaLine.Parse).GetEnumerator();
        }

        public IEnumerable<GsaLine> GetMiddleBlocks()
        {
            return this.Where(line => !this.IsStartInstruction(line) && !this.IsEndInstruction(line));
        }

        public override bool VerifyStructure()
        {
            try
            {
                var count = this.GetNumber();

                for (var i = 1; i < this.Content.Count; i++)
                {
                    var line = this.Content[i];

                    if (i <= count + 1)
                    {
                        GsaLine.Parse(line);
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static readonly string[] StartSymbols = new[] { "begin", "start" };

        public bool IsStartInstruction(GsaLine line)
        {
            return StartSymbols.Contains(line.Instruction.ToLower());
        }

        public bool IsEndInstruction(GsaLine line)
        {
            return !this.GetDestinations(line).Any();
        }

        public GsaLine GetStartInstruction()
        {
            return this.Single(this.IsStartInstruction);
        }

        public IEnumerable<GsaLine> GetDestinations(GsaLine line)
        {
            if (line.First != 0)
            {
                yield return this.Single(l => l.Index == line.First);
            }

            if (line.Second != 0)
            {
                yield return this.Single(l => l.Index == line.Second);
            }
        }

        public void InsertInstruction(string instruction, int index, int first, int second)
        {
            var lastInstruction = this.Last().Instruction;

            var i = this.Content.FindIndex(line => line.Contains(lastInstruction));
            this.Content.Insert(i + 1, $"  {index} {instruction}    {first}    {second}");
            this.SetNumber(this.GetNumber() + 1);
        }

        public int RemoveInstruction(string instruction)
        {
            var count = this.Content.RemoveAll(line => line.Contains(instruction));
            this.SetNumber(this.GetNumber() - count);
            return count;
        }

        public void UpdateDestinations(int oldIndex, int newIndex)
        {
            foreach (var line in this)
            {
                this.UpdateDestinations(line, oldIndex, newIndex);
            }
        }

        public void UpdateDestinations(GsaLine line, int oldIndex, int newIndex)
        {
            string T(int index) => (index == oldIndex ? newIndex : index).ToString();

            var i = this.Content.FindIndex(l => l.Contains(line.Instruction));
            var str = this.Content[i];

            this.Content[i] = str[..(str.IndexOf(line.Instruction.ToString()) + line.Instruction.Length)] + $"\t\t{T(line.First)}\t\t{T(line.Second)}\t\t";
        }

        public List<GsaPath> GetPaths()
        {
            var paths = new List<GsaPath>();
            var hashset = new HashSet<int>();

            var element = this.GetDestinations(this.GetStartInstruction()).Single();
            var current = new GsaPath(element.Index);

            EnterPath(element);

            void EnterPath(GsaLine line)
            {
                var destinations = this.GetDestinations(line).ToList();

                if (destinations.Count == 1)
                {
                    current.Path.Add(line);
                    EnterPath(destinations[0]);
                }
                else
                {
                    if (current.Path.Any())
                    {
                        paths.Add(current);
                    }

                    if (!hashset.Add(line.Index))
                    {
                        return;
                    }

                    foreach (var destination in destinations)
                    {
                        current = new GsaPath(line.Index);
                        EnterPath(destination);
                    }
                }
            }

            return paths;
        }

        public IEnumerable<List<GsaPath>> GetParallelPaths()
        {
            int GetI(GsaPath gsa) => this.GetDestinations(gsa.Path.Last()).Single().Index;

            var paths = this.GetPaths();

            return from element in paths.Select(path => new { path.Source, DestinationIndex = GetI(path) }).Distinct()
                   let list = paths.FindAll(path => path.Source == element.Source && GetI(path) == element.DestinationIndex)
                   where list.Count > 1
                   select list;
        }
    }
}