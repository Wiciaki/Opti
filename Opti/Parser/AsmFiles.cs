namespace Opti.Parser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class AsmFiles : IEnumerable<IAsmFile>, IVerifiable
    {
        public GsaFile Gsa { get; }

        public TxtFile Txt { get; }

        public MicFile Mic { get; }

        public IEnumerator<IAsmFile> GetEnumerator()
        {
            IEnumerable<IAsmFile> GetFiles()
            {
                yield return this.Gsa;
                yield return this.Txt;
                yield return this.Mic;
            }

            return GetFiles().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool VerifyStructure() => this.All(file => file.VerifyStructure());

        public AsmFiles(string[] gsa, string[] txt, string[] mic)
        {
            this.Gsa = new GsaFile(gsa);
            this.Txt = new TxtFile(txt);
            this.Mic = new MicFile(mic);
        }

        public void UpdateInstruction(string instruction, IEnumerable<string> operations)
        {
            this.UpdateInstruction(instruction, string.Join(' ', operations));
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            this.Txt.UpdateInstruction(instruction, operations);
            this.Mic.UpdateInstruction(instruction, operations);
        }

        public void InsertInstruction(string instruction, IEnumerable<string> operations)
        {
            this.InsertInstruction(instruction, string.Join(' ', operations));
        }

        public void InsertInstruction(string instruction, string operations)
        {
            this.Txt.InsertInstruction(instruction, operations);
            this.Mic.InsertInstruction(instruction, operations);
        }

        public void InsertLine(GsaLine line, IEnumerable<string> operations)
        {
            this.Gsa.InsertInstruction(line.Instruction, line.Index, line.First, line.Second);
            this.InsertInstruction(line.Instruction, string.Join(' ', operations));
        }

        public int RemoveInstruction(GsaLine line)
        {
            this.Gsa.RemoveInstruction(line);

            var count = 1;

            if (!this.Gsa.Any(l => l.Instruction == line.Instruction))
            {
                this.RemoveInstruction(line.Instruction);
                count++;
            }

            return count;
        }

        public void RemoveInstruction(string instruction)
        {
            this.Txt.RemoveInstruction(instruction);
            this.Mic.RemoveInstruction(instruction);
        }

        private string GetNewInstructionName()
        {
            var generator = from num in Enumerable.Range(1, int.MaxValue)
                            let result = "Y" + num
                            where this.Gsa.Cast<AsmLine>().Concat(this.Txt).All(l => l.Instruction != result)
                            select result;

            return generator.First();
        }

        public void AddInstruction(int source, string[] operations)
        {
            var newIndex = this.Gsa.Max(line => line.Index) + 1;
            var newInstruction = this.GetNewInstructionName();

            this.Gsa.SetChild(source, newIndex);
            this.InsertLine(new GsaLine(newInstruction, newIndex, source, 0), operations);
        }

        public string PrepareInstruction(GsaLine line)
        {
            if (this.Gsa.Count(l => l.Instruction == line.Instruction) == 1)
            {
                return line.Instruction;
            }

            var instruction = this.GetNewInstructionName();

            this.InsertInstruction(instruction, this.Txt.GetOperationsForInstruction(line.Instruction));
            this.Gsa.SetInstruction(line, instruction);

            return instruction;
        }

        public int RemoveEmptyEntries()
        {
            var elements = (from line in this.Gsa.GetMiddleBlocks()
                            let destinations = this.Gsa.GetChildren(line).ToArray()
                            where destinations.Length == 1 && !this.Txt.GetOperationsForInstruction(line.Instruction).Any()
                            select new { Line = line, Destination = destinations[0] }).ToArray();

            foreach (var e in elements)
            {
                var element = e;
                var line = element.Line;

                int index;

                do
                {
                    var destination = element.Destination;
                    index = destination.Index;
                    element = Array.Find(elements, e => e.Line == destination);
                }
                while (element != null);

                this.Gsa.SetChild(line.Index, index);
                this.RemoveInstruction(line);
            }

            var count = elements.Length;

            foreach (var instruction in this.Txt.Select(line => line.Instruction).Where(instruction => this.Gsa.All(l => l.Instruction != instruction)))
            {
                this.RemoveInstruction(instruction);
                count++;
            }

            var operations = this.Txt.SelectMany(line => line.Operations).Distinct().ToList();
            count += this.Txt.RemoveOperations(line => !operations.Contains(line.Instruction));

            return count;
        }
    }
}