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

        protected override int GetIndex(GsaLine line)
        {
            return this.Content.Skip(1).ToList().FindIndex(l => GsaLine.Parse(l) == line) + 1;
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
            return !this.GetChildren(line).Any();
        }

        public GsaLine GetStartInstruction()
        {
            return this.Single(this.IsStartInstruction);
        }

        public IEnumerable<GsaLine> GetChildren(GsaLine line)
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

        public GsaLine GetChild(GsaPath gsaPath)
        {
            return this.GetChildren(gsaPath.Path.Last()).Single();
        }

        public void InsertInstruction(string instruction, int index, int first, int second)
        {
            this.Content.Insert(GetIndex(this.Last()) + 1, GsaLine.Make(instruction, index, first, second));
            this.SetNumber(this.GetNumber() + 1);
        }

        public void RemoveInstruction(GsaLine line)
        {
            this.Content.RemoveAt(this.GetIndex(line));
            this.SetNumber(this.GetNumber() - 1);
        }

        public void SetChild(int oldIndex, int newIndex)
        {
            foreach (var line in this)
            {
                this.SetChild(line, oldIndex, newIndex);
            }
        }

        public void SetChild(GsaLine line, int oldIndex, int newIndex)
        {
            int T(int index) => index == oldIndex ? newIndex : index;

            this.Content[GetIndex(line)] = GsaLine.Make(line.Instruction, line.Index, T(line.First), T(line.Second));
        }

        public void SetInstruction(GsaLine line, string newInstruction)
        {
            this.Content[GetIndex(line)] = GsaLine.Make(newInstruction, line.Index, line.First, line.Second);
        }

        public void FixIndexing()
        {
            for (var i = 1; i < this.Content.Count; i++)
            {
                var line = GsaLine.Parse(this.Content[i]);
                var index = i - 1;

                this.SetChild(line.Index, index);
                this.Content[i] = GsaLine.Make(line.Instruction, index, line.First, line.Second);
            }
        }

        // algorytm rekurencyjny pozyskiwania ścieżek zawartych w diagramie
        // ścieżka - kilka występujących po sobie bloczków operacyjnych rozgraniczonych przez wierzchołki operacyjne i/lub bloczki start/end
        public List<GsaPath> GetPaths()
        {
            var paths = new List<GsaPath>();
            var hashset = new HashSet<int>();

            var element = this.GetChildren(this.GetStartInstruction()).Single();
            var current = new GsaPath(element.Index);

            EnterPath(element);

            void EnterPath(GsaLine line)
            {
                var children = this.GetChildren(line).ToList();

                if (children.Count == 1)
                {
                    current.Path.Add(line);
                    EnterPath(children[0]);
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

                    foreach (var child in children.GroupBy(line => line.Index).Select(group => group.First()))
                    {
                        current = new GsaPath(line.Index);
                        EnterPath(child);
                    }
                }
            }

            return paths;
        }

        // 'ParallelPaths' - para ścieżek pochodzących ze wspólnego wierzchołka warunkowego
        // w przypadku, gdy ścieżki się łączą w pewnym momencie, funkcja zwraca części ścieżek tylko do momentu przecięcia
        // funkcja zwraca zbiór par wszystkich równoległych ścieżek w diagramie
        public IEnumerable<List<GsaPath>> GetParallelPaths()
        {
            var lists = from path in this.GetPaths()
                        group path by path.Source into gr
                        let list = gr.ToList() 
                        where list.Count > 1
                        select list;

            foreach (var list in lists)
            {
                for (var i = 0; i < list[0].Path.Count; i++)
                {
                    var index = list[1].Path.FindIndex(l => l == list[0].Path[i]);

                    if (index >= 0)
                    {
                        static void TruncAfter(GsaPath p, int i) => p.Path.RemoveRange(i, p.Path.Count - i);

                        TruncAfter(list[0], i);
                        TruncAfter(list[1], index);
                    }
                }

                if (list.All(path => path.Path.Count > 0))
                {
                    yield return list;
                }
            }
        }
    }
}