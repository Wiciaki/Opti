namespace Opti.Optimizations.Block
{
    using System.Collections.Generic;
    using System.Linq;

    using Opti.Parser;

    public class BringOutInstructionOptimization : Optimization
    {
        private readonly HashSet<string> hashset = new HashSet<string>();

        private static IEnumerable<string> GetAllOperations(GsaPath gsaPath)
        {
            return gsaPath.Path.SelectMany(line => Files.Txt.GetOperationsForInstruction(line.Instruction));
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

                // nie rób tej samej optymalizacji ponownie
                if (!hashset.Add(string.Join('.', paths.Select(gsaPath => gsaPath.Source.ToString()).Concat(intersection))))
                {
                    continue;
                }

                // usuwanie części wspólnej z obu ścieżek
                foreach (var line in paths.SelectMany(gsaPath => gsaPath.Path))
                {
                    var instructions = Files.Txt.GetOperationsForInstruction(line.Instruction);
                    Files.Txt.UpdateInstruction(line.Instruction, instructions.Except(intersection));
                }

                // przenoszenie części wspólnej do nowego bloczka
                Files.AddInstruction(paths.ConvertAll(path => path.Path.Last()), intersection);
                count++;
            }

            return count;
        }
    }
}