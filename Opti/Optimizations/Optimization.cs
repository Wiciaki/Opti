namespace Opti.Optimizations
{
    using Opti.Optimizations.Block;
    using Opti.Optimizations.Condition;

    using Opti.Parser;

    using System.Collections.Generic;

    public abstract class Optimization
    {
        protected static AsmFiles Files { get; private set; }

        protected abstract int RunOptimization();

        public int Perform()
        {
            return this.RunOptimization() + Files.RemoveEmptyEntries();
        }

        public static List<Optimization> LoadDefault(AsmFiles files)
        {
            Files = files;

            return new List<Optimization>
            {
                //new DuplicateOperationInPathOptimization(),
                //new MergeBlocksInPathOptimization(),
                new BringOutInstructionOptimization(),

                new RemoveRedundantConditionOptimization(),
            };
        }
    }
}