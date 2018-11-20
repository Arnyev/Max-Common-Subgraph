using System;
using System.Collections.Generic;
using System.Linq;

namespace Taio
{
    class Helpers
    {
        public static bool IsClassConnected(
            (IEnumerable<uint> gPart, IEnumerable<uint> hPart) vertexClass,
            List<(uint v, uint w)> mapping,
            Graph gGraph,
            Graph hGraph)
        {
            if (mapping.Count == 0)
            {
                // If there is no mapping, a class cannot be disconnected, so by
                // convention, we define it as connected.
                return true;
            }
            else
            {
                // Pairs from mapping are either:
                //   a) connected to all the vertices from the class
                // or
                //   b) not connected to any vertex from the class.
                // In more detail:
                //   a) `pair.v` is adjacent (in `gGraph`) to all vertices from `vertexClass.gPart`
                //     AND `pair.w` is adjacent (in `hGraph`) to all vertices from `vertexClass.hPart`
                // or 
                //   b) `pair.v` is not adjacent (in `gGraph`) to any vertex from `vertexClass.gPart`
                //     AND `pair.w` is not adjacent (in `hGraph`) to any vertex from `vertexClass.hPart`
                //
                // So a class can be disconnected from the mapping if and only if all the pairs
                // are not connected (b). In order to make sure that the class is connected,
                // we just have to find a single pair from the mapping that satisfies a).
                // 
                // Even further, by definition of a vertex class:
                // there cannot be such a vertex in `gPart` or `hPart` with a different
                // 'connectivity' to a given pair from mapping than the rest of the vertices.
                // In other words:
                // if a `pair.v` is adjacent to any vertex from `gPart`, then `pair.v` is adjacent
                // to all vertices from `gPart` and `pair.w` is adjacent to all vertices from `hPart`
                //
                // All considered, to check if a class is connected we have to find a single pair
                // from the mapping that is adjacent to any vertex from the class.
                // We arbitrary choose the first vertex from `gPart` for comparison. 
                return mapping.Any(pair => gGraph.AreAdjacent(pair.v, vertexClass.gPart.First()));
            }
        }


        public static int SelectCommon(List<(uint, uint)> mapping, Graph g, Graph h)
        {
            var val = mapping.Sum(y => Math.Abs(g.GetNeighbours(y.Item1).Count - h.GetNeighbours(y.Item2).Count));
            return val;
        }
    }
}
