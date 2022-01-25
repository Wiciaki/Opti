namespace Opti.Optimizations.Block
{
    using System.Linq;

    public class MergeBlocksInPathOptimization : Optimization
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

                    System.Console.WriteLine("merge: " + path[i]);
                    System.Console.WriteLine(string.Join(System.Environment.NewLine,Files.Gsa.GetContent()));

                    var oldOperations = Files.Txt.GetOperationsForInstruction(path[i - 1].Instruction);
                    var newOperations = Files.Txt.GetOperationsForInstruction(path[i].Instruction);

                    // przeniesienie operacji z bloczka do usunięcia do bloczka wyższego
                    Files.UpdateInstruction(path[i].Instruction, string.Empty);
                    Files.UpdateInstruction(path[i - 1].Instruction, oldOperations.Concat(newOperations));
                    count++;
                }
            }

            return count;
        }
    }
}