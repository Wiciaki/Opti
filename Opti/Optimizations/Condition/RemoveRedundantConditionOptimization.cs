namespace Opti.Optimizations.Condition
{
    using System.Linq;

    public class RemoveRedundantConditionOptimization : Optimization
    {
        protected override int RunOptimization()
        {
            var elements = (from line in Files.Gsa
                            let destinations = Files.Gsa.GetChildren(line).ToArray()
                            where destinations.Length == 2 && destinations[0].Index == destinations[1].Index
                            select new { Line = line, Destination = destinations[0] }).ToArray();

            foreach (var element in elements)
            {
                Files.Gsa.SetChild(element.Line.Index, element.Destination.Index);
                Files.RemoveInstruction(element.Line);
            }

            return elements.Length;
        }
    }
}