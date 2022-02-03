namespace Opti.Optimizations
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;

    using Opti.Optimizations.Block;
    using Opti.Optimizations.Condition;

    using Opti.Parser;

    public abstract class Optimization
    {
        protected static AsmFiles Files { get; private set; }

        private static Action<string, object[]> onOptimizeItemCallback;

        protected static void Print(string format, params object[] arguments) => onOptimizeItemCallback?.Invoke(format, arguments);

        protected abstract int RunOptimization();

        public int Perform() => this.RunOptimization() + Files.RemoveEmptyEntries();

        public static List<Optimization> LoadDefault(AsmFiles files, Action<string, object[]> callback = null)
        {
            Files = files;
            onOptimizeItemCallback = callback;

            return new List<Optimization>
            {
                new DuplicateOptimization(),
                new MergeOptimization(),
                new BringOutOptimization(),

                new RemoveVertexOptimization(),
            };
        }

        #region Helper Methods

        protected static string ToSymbol(string instruction)
        {
            return Files.Txt.GetOperations().First(line => line.Instruction == instruction).Operation;
        }

        protected static string PrintOperation(string operation)
        {
            return $"{operation} [ {ToSymbol(operation)} ]";
        }

        protected static string PrintOperations(IEnumerable<string> operations)
        {
            return "[" + string.Join("; ", operations.Select(PrintOperation)) + "]";
        }

        protected static List<string> GetExpressions(string operation)
        {
            return Regex.Matches(operation, @"[\w\d]+").Select(match => match.Value).ToList();
        }

        protected static bool IsCompatible(string instruction, IEnumerable<string> instructions)
        {
            var expressions = GetExpressions(ToSymbol(instruction)).FindAll(val => !int.TryParse(val, out _));

            return expressions.Count == 1 || !instructions.Select(ToSymbol).Select(GetExpressions).Any(e => e.Count > 1 && expressions.Contains(e[0]));
        }

        protected static IEnumerable<string> GetCompatible(IEnumerable<string> instructions)
        {
            return instructions.Where(i => IsCompatible(i, instructions.Except(new[] { i }).Distinct()));
        }

        #endregion
    }
}