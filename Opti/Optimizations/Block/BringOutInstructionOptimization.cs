namespace Opti.Optimizations.Block
{
    using System.Collections.Generic;
    using System.Linq;

    using Opti.Parser;

    public class BringOutInstructionOptimization : Optimization
    {
        private static IEnumerable<string> GetAllOperations(GsaPath p)
        {
            return p.Path.Select(line => line.Instruction).SelectMany(Files.Txt.GetOperationsForInstruction);
        }

        protected override int RunOptimization()
        {
            var count = 0;

            // parallel paths - ścieżki dzielące wspólny początek i zakończenie
            foreach (var paths in Files.Gsa.GetParallelPaths())
            {
                var intersection = GetAllOperations(paths[0]).Intersect(GetAllOperations(paths[1])).ToArray();

                // nie znaleziono elementów w tej optymalizacji
                if (intersection.Length == 0)
                {
                    continue;
                }

                // usuwanie części wspólnej z obu ścieżek
                foreach (var line in paths.SelectMany(gsaPath => gsaPath.Path))
                {
                    var instructions = Files.Txt.GetOperationsForInstruction(line.Instruction);
                    Files.UpdateInstruction(Files.PrepareInstruction(line), instructions.Except(intersection));
                }

                // przenoszenie części wspólnej do nowego bloku
                var instruction = Files.AddInstruction(paths[0].Source, intersection);

                var vertex = Files.Gsa.First(l => l.Index == paths[0].Source);
                var sourceLine = Files.Gsa.First(l => l.Instruction == instruction);

                Print("Bringing out duplicate operation(s) {0} found in children of the vertex {1} to {2}", PrintOperations(intersection), vertex, sourceLine);
                count += intersection.Length;
            }

            return count;
        }
    }
}