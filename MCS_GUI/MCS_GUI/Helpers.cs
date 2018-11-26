using System;
using System.Collections.Generic;
using System.Linq;

namespace Taio
{
    class Helpers
    {
        public static bool IsClassConnected((List<int> gPart, List<int> hPart) vertexClass,
        List<(int v, int w)> mapping, bool[,] gGraph, bool[,] hGraph)
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
                var vertexFromClass = vertexClass.gPart[0];
                return mapping.Any(pair => gGraph[vertexFromClass, pair.v]);
            }
        }


        public static int SelectCommon(List<(int, int)> mapping, bool[,] g, bool[,] h)
        {
            int mappingValue = 0;
            var sizeG = g.GetLength(0);
            var sizeH = h.GetLength(0);

            foreach (var pair in mapping)
            {
                var neighboursG = 0;
                var neighboursH = 0;
                var vertexG = pair.Item1;
                var vertexH = pair.Item2;

                for (int i = 0; i < sizeG; i++)
                    if (g[vertexG, i])
                        neighboursG++;

                for (int i = 0; i < sizeH; i++)
                    if (h[vertexH, i])
                        neighboursH++;

                mappingValue += (int)Math.Abs(neighboursG - neighboursH);
            }

            return mappingValue;
        }

        public static int GetEdgeCount(List<(int, int)> mapping, bool[,] graphG)
        {
            int edgeCount = 0;
            for (int i = 0; i < mapping.Count; i++)
                for (int j = i + 1; j < mapping.Count; j++)
                    if (graphG[mapping[i].Item1, mapping[j].Item1])
                        edgeCount++;
            return edgeCount;
        }
    }
}
