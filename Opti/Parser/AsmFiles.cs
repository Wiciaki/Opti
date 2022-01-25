﻿namespace Opti.Parser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

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

        public bool VerifyStructure()
        {
            return this.All(file => file.VerifyStructure());
        }

        public AsmFiles(string[] gsa, string[] txt, string[] mic)
        {
            this.Gsa = new GsaFile(gsa);
            this.Txt = new TxtFile(txt);
            this.Mic = new MicFile(mic);
        }

        public void UpdateInstruction(string instruction, string operations)
        {
            this.Txt.UpdateInstruction(instruction, operations);
            this.Mic.UpdateInstruction(instruction, operations);
        }

        public void UpdateInstruction(string instruction, IEnumerable<string> operations)
        {
            this.UpdateInstruction(instruction, string.Join(' ', operations));
        }

        public void InsertLine(GsaLine line, string[] operations)
        {
            this.Gsa.InsertInstruction(line.Instruction, line.Index, line.First, line.Second);

            var str = string.Join(' ', operations);
            this.Txt.InsertInstruction(line.Instruction, str);
            this.Mic.InsertInstruction(line.Instruction, str);
        }

        public int RemoveInstruction(GsaLine line)
        {
            var count = this.Gsa.RemoveInstruction(line.Index);

            if (!this.Gsa.Any(l => l.Instruction == line.Instruction))
            {
                count += this.Txt.RemoveInstruction(line.Instruction) + this.Mic.RemoveInstruction(line.Instruction);
            }

            return count;
        }

        private readonly Regex InstructionRegex = new Regex("^\\D+", RegexOptions.Singleline | RegexOptions.Compiled);

        private string GetNewInstructionName()
        {
            var generator = from line in this.Gsa.GetMiddleBlocks().Where(line => this.Txt.Any(l => l.Instruction == line.Instruction))
                            let prefix = InstructionRegex.Match(line.Instruction).Value
                            let number = int.Parse(InstructionRegex.Replace(line.Instruction, string.Empty)) + 1
                            orderby number descending
                            select prefix + number;

            return generator.First();
        }

        public void AddInstruction(int source, string[] operations)
        {
            var newIndex = this.Gsa.Max(line => line.Index) + 1;
            var newInstruction = this.GetNewInstructionName();

            this.Gsa.SetChild(source, newIndex);
            this.InsertLine(new GsaLine(newInstruction, newIndex, source, 0), operations);
        }

        public int RemoveEmptyEntries()
        {
            var elements = (from line in this.Gsa.GetMiddleBlocks()
                            let destinations = this.Gsa.GetChildren(line).ToArray()
                            where destinations.Length == 1 && !this.Txt.GetOperationsForInstruction(line.Instruction).Any()
                            select new { Line = line, Destination = destinations[0] }).ToArray();

            if (elements.Length > 0)
            {
                var element = elements[0];
                var destination = element.Destination;

                GsaLine line;

                while ((line = Array.Find(elements, e => e.Line == destination)?.Line) != null)
                {
                    destination = this.Gsa.GetChildren(line).Single();
                }

                this.Gsa.SetChild(element.Line.Index, destination.Index);
                this.RemoveInstruction(element.Line);

                return 1 + this.RemoveEmptyEntries();
            }

            var instructions = this.Txt.SelectMany(line => line.Operations).Distinct().ToList();
            var removedOperations = this.Txt.RemoveOperations(line => !instructions.Contains(line.Instruction));

            return removedOperations;
        }
    }
}