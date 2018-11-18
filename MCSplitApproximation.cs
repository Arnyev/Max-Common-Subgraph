using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Taio
{
    class McSplitApproximation
    {
        private static int ExpectedSize;
        private static List<(uint, uint)> maxMapping;
        private static List<(List<uint>, List<uint>)> bestFuture;
        private static int bestFutureWorth;
        private static Graph _graphG;
        private static Graph _graphH;

        public static List<(uint, uint)> Find(Graph graphG, Graph graphH, int stepSize)
        {
            maxMapping = new List<(uint, uint)>();
            bestFuture = new List<(List<uint>, List<uint>)>()
            {
                    (Enumerable.Range(0, (int) graphG.Size)
                            .Select(x => (uint) x)
                            .ToList(),
                     Enumerable.Range(0, (int) graphH.Size)
                            .Select(x => (uint) x)
                            .ToList())
            };

            bestFutureWorth = int.MaxValue;
            _graphG = graphG;
            _graphH = graphH;

            ExpectedSize = stepSize;

            while (bestFuture.Count > 0)
            {
                FindRecursive(bestFuture, maxMapping);
                ExpectedSize = ExpectedSize + stepSize;
            }

            return maxMapping;
        }

        private static void FindRecursive(List<(List<uint>, List<uint>)> classes, List<(uint, uint)> mapping)
        {
            var mappingValue = Helpers.SelectCommon(mapping, _graphG, _graphH);
            if (mapping.Count > maxMapping.Count || mapping.Count == maxMapping.Count && mappingValue < bestFutureWorth)
            {
                maxMapping = mapping;
                bestFutureWorth = mappingValue;
                bestFuture = classes.ToList();
                
            }

            if (mapping.Count == ExpectedSize)
                return;

            var maximumPossible = mapping.Count + classes.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count));
            if (maximumPossible <= maxMapping.Count)
                return;

            var (g, h) = classes.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, _graphG, _graphH));
            if (g == null)
                return;

            var v = g.First();
            foreach (var w in h)
            {
                var futurePrim = new List<(List<uint>, List<uint>)>();
                foreach (var (gPrim, hPrim) in classes)
                {
                    var gBis = gPrim.Intersect(_graphG.GetNeighbours(v)).ToList();
                    var hBis = hPrim.Intersect(_graphH.GetNeighbours(w)).ToList();
                    if (gBis.Count() > 0 && hBis.Count() > 0)
                    {
                        futurePrim.Add((gBis, hBis));
                    }

                    gBis = gPrim.Intersect(_graphG.GetNonNeighbours(v)).ToList();
                    hBis = hPrim.Intersect(_graphH.GetNonNeighbours(w)).ToList();
                    if (gBis.Count() > 0 && hBis.Count() > 0)
                    {
                        futurePrim.Add((gBis, hBis));
                    }
                }

                FindRecursive(futurePrim, mapping.Union(new List<(uint, uint)>() { (v, w) }).ToList());
            }

            var gWithoutV = g.Where(x => x != v).ToList();
            classes.Remove((g, h));
            if (gWithoutV.Count() > 0)
            {
                classes.Add((gWithoutV, h));
            }

            FindRecursive(classes, mapping);
        }
    }
}
