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
        private static (List<uint>, List<uint>) bestFutureUnconnected;
        private static uint bestFutureWorth;
        private static bool[,] _graphG;
        private static bool[,] _graphH;

        public static List<(uint, uint)> Find(bool[,] graphG, bool[,] graphH, int stepSize)
        {
            var sizeG = graphG.GetLength(0);
            var sizeH = graphH.GetLength(0);

            maxMapping = new List<(uint, uint)>();
            bestFuture = new List<(List<uint>, List<uint>)>();
            bestFutureUnconnected =
                    (Enumerable.Range(0, sizeG).Select(x => (uint)x).ToList(),
                     Enumerable.Range(0, sizeH).Select(x => (uint)x).ToList());

            bestFutureWorth = uint.MaxValue;
            _graphG = graphG;
            _graphH = graphH;

            ExpectedSize = stepSize;

            do
            {
                FindRecursive(bestFuture, bestFutureUnconnected, maxMapping);
                ExpectedSize = ExpectedSize + stepSize;
            } while (bestFuture.Count > 0);

            return maxMapping;
        }

        private static void FindRecursive(List<(List<uint>, List<uint>)> classes, (List<uint>, List<uint>) unconnectedClass, List<(uint, uint)> mapping)
        {
            var mappingValue = Helpers.SelectCommon(mapping, _graphG, _graphH);
            if (mapping.Count > maxMapping.Count || (mapping.Count == maxMapping.Count && mappingValue < bestFutureWorth))
            {
                maxMapping = new List<(uint, uint)>(mapping);
                bestFutureWorth = mappingValue;
                bestFuture = classes.ToList();
                bestFutureUnconnected = (unconnectedClass.Item1.ToList(), unconnectedClass.Item2.ToList());
            }

            if (mapping.Count == ExpectedSize || (classes.Count == 0 && mapping.Count != 0))
                return;

            var maximumPossible = mapping.Count + classes.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count))
                                    + Min(unconnectedClass.Item1.Count, unconnectedClass.Item2.Count);
            if (maximumPossible <= maxMapping.Count)
                return;

            var (g, h) = mapping.Count != 0 ? classes[0] : unconnectedClass;
            if (g.Count == 0)
                return;

            var v = g[0];

            foreach (var w in h)
            {
                var futurePrim = new List<(List<uint>, List<uint>)>();
                foreach (var (gPrim, hPrim) in classes)
                    UpdateClasses(v, w, futurePrim, gPrim, hPrim);

                UpdateClassesUnconnected(unconnectedClass, v, w, futurePrim);

                var newUnconnectedClass = GetNewUnconnectedClass(unconnectedClass, v, w);

                var newMapping = mapping.ToList();
                newMapping.Add((v, w));
                FindRecursive(futurePrim, newUnconnectedClass, newMapping);
            }

            if (mapping.Count == 0)
                unconnectedClass.Item1.Remove(v);
            else
            {
                var gWithoutV = g.Where(x => x != v).ToList();
                classes.Remove((g, h));
                if (gWithoutV.Count() > 0)
                    classes.Add((gWithoutV, h));
            }

            FindRecursive(classes, unconnectedClass, mapping);
        }

        private static void UpdateClassesUnconnected((List<uint>, List<uint>) unconnectedClass, uint v, uint w, List<(List<uint>, List<uint>)> futurePrim)
        {
            var gBisUn = new List<uint>();
            foreach (var vertexG in unconnectedClass.Item1)
                if (_graphG[v, vertexG])
                    gBisUn.Add(vertexG);

            var hBisUn = new List<uint>();
            foreach (var vertexh in unconnectedClass.Item2)
                if (_graphH[w, vertexh])
                    hBisUn.Add(vertexh);

            if (gBisUn.Count() > 0 && hBisUn.Count() > 0)
                futurePrim.Add((gBisUn, hBisUn));
        }

        private static void UpdateClasses(uint v, uint w, List<(List<uint>, List<uint>)> futurePrim, List<uint> gPrim, List<uint> hPrim)
        {
            var gBis = new List<uint>();
            foreach (var vertexG in gPrim)
                if (_graphG[v, vertexG])
                    gBis.Add(vertexG);

            var hBis = new List<uint>();
            foreach (var vertexh in hPrim)
                if (_graphH[w, vertexh])
                    hBis.Add(vertexh);

            if (gBis.Count() > 0 && hBis.Count() > 0)
                futurePrim.Add((gBis, hBis));

            gBis = new List<uint>();
            foreach (var vertexG in gPrim)
                if (!_graphG[v, vertexG] && v != vertexG)
                    gBis.Add(vertexG);
            hBis = new List<uint>();
            foreach (var vertexh in hPrim)
                if (!_graphH[w, vertexh] && vertexh != w)
                    hBis.Add(vertexh);

            if (gBis.Count() > 0 && hBis.Count() > 0)
                futurePrim.Add((gBis, hBis));
        }

        private static (List<uint>, List<uint>) GetNewUnconnectedClass((List<uint>, List<uint>) unconnectedClass, uint v, uint w)
        {
            var newUnconnectedClass = (new List<uint>(), new List<uint>());
            foreach (var vertexG in unconnectedClass.Item1)
                if (!_graphG[v, vertexG] && v != vertexG)
                    newUnconnectedClass.Item1.Add(vertexG);

            foreach (var vertexh in unconnectedClass.Item2)
                if (!_graphH[w, vertexh] && w != vertexh)
                    newUnconnectedClass.Item2.Add(vertexh);

            return newUnconnectedClass;
        }
    }
}
