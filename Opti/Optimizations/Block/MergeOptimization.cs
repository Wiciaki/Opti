namespace Opti.Optimizations.Block
{
    using System.Linq;

    public class MergeOptimization : Optimization
    {
        protected override int RunOptimization()
        {
            var count = 0;

            // w każdej ścieżce, od drugiego elementu
            foreach (var path in Files.Gsa.GetPaths().Select(p => p.Path))
            {
                for (int i = 1; i < path.Count; i++)
                {
                    // pomiń bloczki, do których wejście jest z więcej niż jednej strony
                    if (Files.Gsa.SelectMany(Files.Gsa.GetChildren).Count(line => line == path[i]) > 1)
                    {
                        continue;
                    }

                    var oldOperations = Files.Txt.GetOperationsForInstruction(path[i - 1].Instruction);
                    var newOperations = Files.Txt.GetOperationsForInstruction(path[i].Instruction);
                    
                    var toRemove = newOperations.Where(operation => IsCompatible(operation, oldOperations)).ToList();

                    if (toRemove.Count == 0)
                    {
                        continue;
                    }

                    // przeniesienie operacji z bloczka do usunięcia do bloczka wyższego
                    Files.UpdateInstruction(Files.PrepareInstruction(path[i]), newOperations.Except(toRemove));
                    Files.UpdateInstruction(Files.PrepareInstruction(path[i - 1]), oldOperations.Concat(toRemove));

                    Print("Merged operations: {0} from {1} into {2}", PrintOperations(toRemove), path[i], path[i - 1]);
                    count += toRemove.Count;
                }
            }

            return count;
        }
    }
}