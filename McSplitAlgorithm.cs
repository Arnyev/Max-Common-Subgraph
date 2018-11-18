
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Taio
{
    class McSplitAlgorithm
    {
        static public List<List<(uint, uint)>> McSplit(Graph graphG, Graph graphH)
        {
            var maxMappingSize = -1;
            var maxMappings = new List<List<(uint, uint)>>();

            McSplitRecursive(
                new List<(List<uint>, List<uint>)>()
                    {
                        (Enumerable.Range(0, (int)graphG.Size)
                                   .Select(x => (uint)x)
                                   .ToList(),
                         Enumerable.Range(0, (int)graphH.Size)
                                   .Select(x => (uint)x)
                                   .ToList())
                    },
                new List<(uint, uint)>());

            return maxMappings;

            void McSplitRecursive(List<(List<uint>, List<uint>)> future, List<(uint, uint)> mapping)
            {
                if (mapping.Count > maxMappingSize)
                {
                    maxMappings.Clear();
                    maxMappings.Add(mapping);
                    maxMappingSize = mapping.Count;
                }
                else if (mapping.Count == maxMappingSize)
                {
                    maxMappings.Add(mapping);
                }

                var maximumPossible = mapping.Count + future.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count));
                if (maximumPossible < maxMappingSize) return;

                var (g, h) = future.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, graphG, graphH));
                if (g == null) return;

                var v = g.First();
                foreach (var w in h)
                {
                    var futurePrim = new List<(List<uint>, List<uint>)>();
                    foreach (var (gPrim, hPrim) in future)
                    {
                        var gBis = gPrim.Intersect(graphG.GetNeighbours(v)).ToList();
                        var hBis = hPrim.Intersect(graphH.GetNeighbours(w)).ToList();
                        if (gBis.Count() > 0 && hBis.Count() > 0)
                        {
                            futurePrim.Add((gBis, hBis));
                        }

                        gBis = gPrim.Intersect(graphG.GetNonNeighbours(v)).ToList();
                        hBis = hPrim.Intersect(graphH.GetNonNeighbours(w)).ToList();
                        if (gBis.Count() > 0 && hBis.Count() > 0)
                        {
                            futurePrim.Add((gBis, hBis));
                        }
                    }
                    McSplitRecursive(futurePrim, mapping.Union(new List<(uint, uint)>() { (v, w) }).ToList());
                }
                var gWithoutV = g.Where(x => x != v).ToList();
                future.Remove((g, h));
                if (gWithoutV.Count() > 0)
                {
                    future.Add((gWithoutV, h));
                }
                McSplitRecursive(future, mapping);
            }
        }


    }
}
