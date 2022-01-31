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
                new DuplicateOperationInPathOptimization(),
                new MergeBlocksInPathOptimization(),
                new BringOutInstructionOptimization(),

                new RemoveRedundantConditionOptimization(),
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

        protected static IEnumerable<string> GetExpressions(string operation)
        {
            return Regex.Matches(operation, @"[\w\d]+").Select(match => match.Value);
        }

        protected static bool Filter(string instruction, IEnumerable<string> instructions)
        {
            var expressions = GetExpressions(ToSymbol(instruction)).Where(val => !int.TryParse(val, out _)).ToArray();

            return !instructions.Select(ToSymbol).Select(s => GetExpressions(s).First()).Any(expressions.Contains);
        }

        protected static IEnumerable<string> Filter(IEnumerable<string> instructions)
        {
            return instructions.Where(i => Filter(i, instructions.Except(new[] { i }).Distinct()));
        }

        #endregion
    }
}