namespace Opti.Optimizations.Condition
{
    using System.Linq;

    public class RemoveVertexOptimization : Optimization
    {
        protected override int RunOptimization()
        {
            var elements = (from line in Files.Gsa
                            let children = Files.Gsa.GetChildren(line).ToArray()
                            where children.Length == 2 && children[0] == children[1]
                            select new { Line = line, Destination = children[0] }).ToArray();

            foreach (var element in elements)
            {
                Files.Gsa.SetChild(element.Line.Index, element.Destination.Index);
                Files.RemoveInstruction(element.Line);

                Print("Removing redundant vertex {0}", element.Line);
            }

            return elements.Length;
        }
    }
}