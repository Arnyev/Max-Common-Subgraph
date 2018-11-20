using System;
using System.Collections.Generic;
using System.Linq;

namespace Taio
{
    class Helpers
    {
        public static bool IsClassConnected((IEnumerable<uint>, IEnumerable<uint>) vClass, List<(uint, uint)> mapping,
            Graph g, Graph h)
        {
            if (mapping.Count == 0)
            {
                return true;
            }
            else
            {
                return vClass.Item1.All(vcg =>
                           mapping.Select(m => m.Item1)
                               .Any(vmg => g.AreAdjacent(vcg, vmg))) &&
                       vClass.Item2.All(vch =>
                           mapping.Select(m => m.Item2)
                               .Any(vmh => h.AreAdjacent(vch, vmh)));
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

                mappingValue += Math.Abs(neighboursG - neighboursH);
            }
            
            return mappingValue;
        }
    }
}
