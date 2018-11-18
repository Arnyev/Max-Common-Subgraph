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


        public static int SelectCommon(List<(uint, uint)> mapping, Graph g, Graph h)
        {
            var val = mapping.Sum(y => Math.Abs(g.GetNeighbours(y.Item1).Count - h.GetNeighbours(y.Item2).Count));
            return val;
        }
    }
}
