namespace Opti.Optimizations.Vertex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Opti.Parser;

    public class VertexOptimization : Optimization
    {
        private record Connection(List<GsaLine> Lines, List<int> States);

        private static List<Connection> GetConnections()
        {
            var list = new List<Connection>();

            foreach (var block in Files.Gsa.Where(line => Files.Gsa.GetChildren(line).Count() == 1))
            {
                void EnterPath(Connection connection)
                {
                    Console.WriteLine(connection.Lines.Count + " - " + connection.States.Count );
                    var children = Files.Gsa.GetChildren(connection.Lines.Last()).ToArray();

                    switch (children.Length)
                    {
                        case 0: // End
                            break;
                        case 1:
                            if (connection.States.Count != 0)
                            {
                                list.Add(connection);
                            }
                            break;
                        case 2:
                            if (!connection.Lines.Contains(children[0]))
                            {
                                EnterPath(new Connection(connection.Lines.Concat(new[] { children[0] }).ToList(), connection.States.Concat(new[] { 0 }).ToList()));
                            }
                            if (!connection.Lines.Contains(children[1]))
                            {
                                EnterPath(new Connection(connection.Lines.Concat(new[] { children[1] }).ToList(), connection.States.Concat(new[] { 1 }).ToList()));
                            }
                            break;
                    }
                }

                EnterPath(new Connection(new List<GsaLine> { block }, new List<int>()));
            }

            return list;
        }

        protected override int RunOptimization()
        {
            var connections = GetConnections();
            Console.WriteLine(connections.Count);

            return 0;
        }
    }
}