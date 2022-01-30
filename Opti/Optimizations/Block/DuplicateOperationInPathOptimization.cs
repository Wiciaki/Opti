namespace Opti.Optimizations.Block
{
    using System;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Linq;

    public class DuplicateOperationInPathOptimization : Optimization
    {
        private static IEnumerable<string> Filter(string[] instructions)
        {
            static string ToOperation(string instruction)
            {
                return Files.Txt.GetOperations().First(line => line.Instruction == instruction).Operation;
            }

            static IEnumerable<string> GetExpressions(string operation)
            {
                return Regex.Matches(operation, "[\\w\\d]+").Select(match => match.Value);
            }

            foreach (var instruction in instructions)
            {
                var expressions = GetExpressions(ToOperation(instruction)).Where(val => !int.TryParse(val, out _)).ToArray();

                if (!instructions.Except(new[] { instruction }).Select(o => GetExpressions(ToOperation(o)).First()).Any(expressions.Contains))
                {
                    yield return instruction;
                }
            }
        }

        protected override int RunOptimization()
        {
            // licznik dokonanych zmian
            var count = 0;

            // dla każdej znalezionej ścieżki pomiędzy dwoma bloczkami warunkowymi
            foreach (var path in Files.Gsa.GetPaths().Select(gsaPath => gsaPath.Path))
            {
                var hashset = new HashSet<string>();

                // usuwać elementy będziemy od ostatniego do pierwszego bloku operacyjnego w ścieżce
                path.Reverse();

                foreach (var line in path)
                {
                    // pobierz operacje z bieżącej linii
                    var operations = Files.Txt.GetOperationsForInstruction(line.Instruction);
                    
                    foreach (var operation in Filter(operations))
                    {
                        // już się pojawiła wcześniej, można usunąć
                        if (!hashset.Add(operation))
                        {
                            Files.UpdateInstruction(Files.PrepareInstruction(line), operations.Except(hashset));
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}