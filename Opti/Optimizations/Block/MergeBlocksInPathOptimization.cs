namespace Opti.Optimizations.Block
{
    using System.Linq;

    public class MergeBlocksInPathOptimization : Optimization
    {
        protected override int RunOptimization()
        {
            var count = 0;

            // w każdej ścieżce, od drugiego elementu
            foreach (var path in Files.Gsa.GetPaths().Select(gsaPath => gsaPath.Path))
            {
                for (int i = 1; i < path.Count; i++)
                {
                    // pomiń bloczki, do których wejście jest z więcej niż jednej strony
                    if (Files.Gsa.Where(l => Files.Gsa.GetDestinations(l).Any(line => line.Index == path[i].Index)).Skip(1).Any())
                    {
                        continue;
                    }

                    var oldOperations = Files.Txt.GetOperationsForInstruction(path[i - 1].Instruction);
                    var newOperations = Files.Txt.GetOperationsForInstruction(path[i].Instruction);

                    // przeniesienie operacji z bloczka do usunięcia do bloczka wyższego
                    Files.Txt.UpdateInstruction(path[i].Instruction, string.Empty);
                    Files.Txt.UpdateInstruction(path[i - 1].Instruction, oldOperations.Concat(newOperations));
                    count++;
                }
            }

            return count;
        }
    }
}