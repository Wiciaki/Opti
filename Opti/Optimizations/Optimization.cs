namespace Opti.Optimizations
{
    using Opti.Optimizations.Block;

    using Opti.Parser;

    using System.Collections.Generic;

    public abstract class Optimization
    {
        protected static AsmFileCollection Files { get; private set; }

        protected abstract int RunOptimization();

        public int Perform()
        {
            return this.RunOptimization() + Files.RemoveEmptyEntries();
        }

        public static IEnumerable<Optimization> LoadOptimizations(AsmFileCollection files)
        {
            Files = files;

            var list = new List<Optimization>();
            void Add<TOptimization>() where TOptimization : Optimization, new() => list.Add(new TOptimization());

            Add<DuplicateOperationInPathOptimization>();
            Add<MergeBlocksInPathOptimization>();
            Add<BringOutInstructionOptimization>();

            return list;
        }
    }
}