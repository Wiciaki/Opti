namespace Opti.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class AsmFileCollection : IVerifiable
    {
        public GsaFile Gsa { get; }

        public TxtFile Txt { get; }

        public MicFile Mic { get; }

        public IEnumerable<IAsmFile> Files()
        {
            yield return this.Gsa;
            yield return this.Txt;
            yield return this.Mic;
        }

        public bool VerifyStructure()
        {
            return this.Files().All(file => file.VerifyStructure());
        }

        public AsmFileCollection(string[] gsa, string[] txt, string[] mic)
        {
            this.Gsa = new(gsa);
            this.Txt = new(txt);
            this.Mic = new(mic);
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            this.Txt.UpdateInstruction(instruction, operations);
            this.Mic.UpdateInstruction(instruction, operations);
        }

        public void UpdateInstruction(string instruction, string[] operations)
        {
            this.UpdateInstruction(instruction, string.Join(' ', operations));
        }

        public int RemoveInstruction(string instruction, int index)
        {
            var ct = this.Gsa.RemoveInstruction(index);

            if (!this.Gsa.Any(line => line.Instruction == instruction))
            {
                ct += this.Txt.RemoveInstruction(instruction) + this.Mic.RemoveInstruction(instruction);
            }

            return ct;
        }

        private readonly Regex InstructionRegex = new("^\\D+", RegexOptions.Singleline | RegexOptions.Compiled);

        public void AddInstruction(List<GsaLine> sources, string[] operations)
        {
            // tu pojawi się błąd jeśli podamy jako argument bloczki pod które nie da się postawić nowego
            var oldIndex = sources.Select(line => this.Gsa.GetDestinations(line).Single().Index).Distinct().Single();
            var newIndex = this.Gsa.Max(line => line.Index) + 1;

            var instruction = (from line in this.Gsa.GetMiddleBlocks().Where(line => this.Txt.Any(l => l.Instruction == line.Instruction))
                               let prefix = InstructionRegex.Match(line.Instruction).Value
                               let number = int.Parse(InstructionRegex.Replace(line.Instruction, string.Empty)) + 1
                               orderby number descending
                               select prefix + number).First();

            this.Gsa.InsertInstruction(instruction, newIndex, oldIndex, 0);
            this.Txt.InsertInstruction(instruction, operations);
            this.Mic.InsertInstruction(instruction, operations);

            sources.ForEach(line => this.Gsa.UpdateDestinations(line, oldIndex, newIndex));
        }

        public int RemoveEmptyEntries()
        {
            var elements = (from line in this.Gsa.GetMiddleBlocks()
                            let destinations = this.Gsa.GetDestinations(line).ToList()
                            where destinations.Count == 1 && !this.Txt.GetOperationsForInstruction(line.Instruction).Any()
                            select new { Line = line, Destination = destinations[0] }).ToList();

            foreach (var element in elements)
            {
                var destination = element.Destination.Index;

                GsaLine line;
                while ((line = elements.Find(e => e.Line.Index == destination)?.Line) != null)
                {
                    destination = this.Gsa.GetDestinations(line).Single().Index;
                }

                this.Gsa.UpdateDestinations(element.Line.Index, destination);
                this.RemoveInstruction(element.Line.Instruction, element.Line.Index);

                return 1 + this.RemoveEmptyEntries();
            }

            var instructions = this.Txt.SelectMany(line => line.Operations).Distinct().ToList();
            var removedOperations = this.Txt.RemoveOperations(line => !instructions.Contains(line.Instruction));

            return removedOperations;
        }
    }
}