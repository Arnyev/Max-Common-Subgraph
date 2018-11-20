using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Taio
{
    class McSplitApproximationEdge
    {
        private static int ExpectedSize;
        private static List<(int, int)> maxMapping;
        private static List<(List<int>, List<int>)> bestFuture;
        private static (List<int>, List<int>) bestFutureUnconnected;
        private static List<(int, int)> maxMappingEdge;
        private static int bestEdgeCount;
        private static int bestFutureWorth;
        private static bool[,] _graphG;
        private static bool[,] _graphH;

        public static List<(int, int)> Find(bool[,] graphG, bool[,] graphH, int stepSize)
        {
            var sizeG = graphG.GetLength(0);
            var sizeH = graphH.GetLength(0);

            maxMapping = new List<(int, int)>();
            bestFuture = new List<(List<int>, List<int>)>();
            bestFutureUnconnected =
                    (Enumerable.Range(0, sizeG).ToList(),
                     Enumerable.Range(0, sizeH).ToList());

            maxMappingEdge = new List<(int, int)>();

            bestFutureWorth = int.MaxValue;
            _graphG = graphG;
            _graphH = graphH;

            ExpectedSize = stepSize;

            do
            {
                FindRecursive(bestFuture, bestFutureUnconnected, maxMapping);
                ExpectedSize = ExpectedSize + stepSize;
            } while (bestFuture.Count > 0);

            return maxMappingEdge;
        }


        private static void FindRecursive(List<(List<int>, List<int>)> classes, (List<int>, List<int>) unconnectedClass, List<(int, int)> mapping)
        {
            var mappingValue = Helpers.SelectCommon(mapping, _graphG, _graphH);
            if (mapping.Count > maxMapping.Count || (mapping.Count == maxMapping.Count && mappingValue < bestFutureWorth))
            {
                maxMapping = new List<(int, int)>(mapping);
                bestFutureWorth = mappingValue;
                bestFuture = classes.ToList();
                bestFutureUnconnected = (unconnectedClass.Item1.ToList(), unconnectedClass.Item2.ToList());
            }

            var edgeCount = Helpers.GetEdgeCount(mapping, _graphG);
            if (edgeCount > bestEdgeCount)
            {
                bestEdgeCount = edgeCount;
                maxMappingEdge = mapping.ToList();
            }

            if (mapping.Count == ExpectedSize || (classes.Count == 0 && mapping.Count != 0))
                return;

            var (g, h) = mapping.Count != 0 ? classes[0] : unconnectedClass;
            if (g.Count == 0)
                return;

            var v = g[0];

            foreach (var w in h)
            {
                var futurePrim = new List<(List<int>, List<int>)>();
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

        private static void UpdateClassesUnconnected((List<int>, List<int>) unconnectedClass, int v, int w, List<(List<int>, List<int>)> futurePrim)
        {
            var gBisUn = new List<int>();
            foreach (var vertexG in unconnectedClass.Item1)
                if (_graphG[v, vertexG])
                    gBisUn.Add(vertexG);

            var hBisUn = new List<int>();
            foreach (var vertexh in unconnectedClass.Item2)
                if (_graphH[w, vertexh])
                    hBisUn.Add(vertexh);

            if (gBisUn.Count() > 0 && hBisUn.Count() > 0)
                futurePrim.Add((gBisUn, hBisUn));
        }

        private static void UpdateClasses(int v, int w, List<(List<int>, List<int>)> futurePrim, List<int> gPrim, List<int> hPrim)
        {
            var gBis = new List<int>();
            foreach (var vertexG in gPrim)
                if (_graphG[v, vertexG])
                    gBis.Add(vertexG);

            var hBis = new List<int>();
            foreach (var vertexh in hPrim)
                if (_graphH[w, vertexh])
                    hBis.Add(vertexh);

            if (gBis.Count() > 0 && hBis.Count() > 0)
                futurePrim.Add((gBis, hBis));

            gBis = new List<int>();
            foreach (var vertexG in gPrim)
                if (!_graphG[v, vertexG] && v != vertexG)
                    gBis.Add(vertexG);
            hBis = new List<int>();
            foreach (var vertexh in hPrim)
                if (!_graphH[w, vertexh] && vertexh != w)
                    hBis.Add(vertexh);

            if (gBis.Count() > 0 && hBis.Count() > 0)
                futurePrim.Add((gBis, hBis));
        }

        private static (List<int>, List<int>) GetNewUnconnectedClass((List<int>, List<int>) unconnectedClass, int v, int w)
        {
            var newUnconnectedClass = (new List<int>(), new List<int>());
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
