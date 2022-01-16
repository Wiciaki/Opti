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

        private int GetIndex(int other)
        {
            return this.Content.Skip(1).ToList().FindIndex(line => GsaLine.Parse(line).Index == other) + 1;
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

        private static readonly string[] StartSymbols = new[] { "begin" };

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
            var lastIndex = this.Last().Index;

            this.Content.Insert(GetIndex(lastIndex) + 1, $"  {index} {instruction}    {first}    {second}");
            this.SetNumber(this.GetNumber() + 1);
        }

        public int RemoveInstruction(int index)
        {
            var count = this.Content.RemoveAll(line => line != this.Content[0] && GsaLine.Parse(line).Index == index);
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

            var i = GetIndex(line.Index);
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

                    foreach (var destination in destinations.GroupBy(l => l.Index).Select(g => g.First()))
                    {
                        current = new GsaPath(line.Index);
                        EnterPath(destination);
                    }
                }
            }

            return paths;
        }

        public IEnumerable<List<GsaPath>> GetParallelPaths(bool cutOffMore = true)
        {
            int GetI(GsaPath gsa) => this.GetDestinations(gsa.Path.Last()).Single().Index;

            var paths = this.GetPaths();

            //var lists = from element in paths.Select(path => new { path.Source, DestinationIndex = GetI(path) }).GroupBy(p => p.Source).Select(g => g.First())
            //       let list = paths.FindAll(path => path.Source == element.Source && GetI(path) == element.DestinationIndex)
            //       where list.Count == 2
            //       select list;

            var lists = from source in paths.Select(path => path.Source).Distinct()
                        let list = paths.FindAll(path => path.Source == source)
                        where list.Count == 2 && !list.Select(path => GetI(path)).Distinct().Skip(1).Any()
                        select list;

            foreach (var tList in lists)
            {
                var list = tList.ToList();

                for (var i = 1; i < list[1].Path.Count; i++)
                {
                    int index;

                    if ((index = list[0].Path.FindIndex(l => l.Index == list[1].Path[i].Index)) >= 0)
                    {
                        void TruncAfter(GsaPath p, int i) => p.Path.RemoveRange(i, p.Path.Count - i);

                        if (cutOffMore)
                        {
                            TruncAfter(list[0], index);
                        }

                        TruncAfter(list[1], i);
                    }
                }

                yield return list;
            }
        }
    }
}