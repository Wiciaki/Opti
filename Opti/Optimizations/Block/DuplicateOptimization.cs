namespace Opti.Optimizations.Block
{
    using System.Collections.Generic;
    using System.Linq;

    public class DuplicateOptimization : Optimization
    {
        protected override int RunOptimization()
        {
            // licznik dokonanych zmian
            var count = 0;

            // dla każdej znalezionej ścieżki pomiędzy dwoma bloczkami warunkowymi
            foreach (var path in Files.Gsa.GetPaths().Select(gsaPath => gsaPath.Path))
            {
                // usuwać elementy będziemy od ostatniego do pierwszego bloku operacyjnego w ścieżce
                path.Reverse();

                var hashset = new HashSet<string>();

                foreach (var line in path)
                {
                    // pobierz operacje z bieżącej linii
                    var operations = Files.Txt.GetOperationsForInstruction(line.Instruction);

                    foreach (var operation in GetCompatible(operations))
                    {
                        // już się pojawiła wcześniej, można usunąć
                        if (!hashset.Add(operation))
                        {
                            var instruction = Files.PrepareInstruction(line);

                            var list = Files.Txt.GetOperationsForInstruction(instruction).ToList();
                            list.Remove(operation);

                            Files.UpdateInstruction(instruction, list);

                            Print("Duplicate operation: {0} in {1}", PrintOperation(operation), line);
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}