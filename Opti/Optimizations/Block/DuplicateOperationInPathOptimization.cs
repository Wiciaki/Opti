namespace Opti.Optimizations.Block
{
    using System.Collections.Generic;
    using System.Linq;

    public class DuplicateOperationInPathOptimization : Optimization
    {
        private static bool IsSafeToRemove(string instruction)
        {
            var line = Files.Txt.GetOperations().First(line => line.Instruction == instruction);
            //System.Console.WriteLine(line.Operation);
            return int.TryParse(line.Operation, out _);
        }

        protected override int RunOptimization()
        {
            // licznik dokonanych zmian
            var count = 0;

            // dla każdej znalezionej ścieżki pomiędzy dwoma bloczkami warunkowymi
            foreach (var path in Files.Gsa.GetPaths().Select(gsaPath => gsaPath.Path))
            {
                var hashset = new HashSet<string>();

                // usuwać elementy będziemy od ostatniego do pierwszego bloczka w ścieżce
                path.Reverse();

                foreach (var line in path)
                {
                    // pobierz operacje z bieżącej linii
                    var instructions = Files.Txt.GetOperationsForInstruction(line.Instruction);
                    
                    foreach (var instruction in instructions.Where(IsSafeToRemove))
                    {
                        // już się pojawiła wcześniej, można usunąć
                        if (!hashset.Add(instruction))
                        {
                            Files.UpdateInstruction(line.Instruction, instructions.Except(hashset));
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}