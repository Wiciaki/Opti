namespace Opti.Optimizations
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;

    using Opti.Parser;

    using Opti.Optimizations.Block;
    using Opti.Optimizations.Vertex;

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
                new VertexOptimization()
            };
        }

        #region Helper Methods

        protected static string ToSymbol(string operation)
        {
            return Files.Txt.GetOperations().First(line => line.Instruction == operation).Operation;
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

        protected static bool IsCompatible(string operation, IEnumerable<string> operations)
        {
            var expressions = GetExpressions(ToSymbol(operation)).FindAll(val => !int.TryParse(val, out _));

            return expressions.Count == 1 || !operations.Select(ToSymbol).Select(GetExpressions).Any(e => e.Count > 1 && expressions.Contains(e[0]));
        }

        protected static IEnumerable<string> GetCompatible(IEnumerable<string> operations)
        {
            return operations.Where(i => IsCompatible(i, operations.Except(new[] { i }).Distinct()));
        }

        #endregion
    }
}