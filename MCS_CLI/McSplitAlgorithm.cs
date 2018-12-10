using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace Taio
{
    class McSplitAlgorithmSolver
    {
        private int maxMappingSize = 1;
        private readonly List<List<(int, int)>> maxMappings = new List<List<(int, int)>>();
        private int maxEdgesCount = -1;
        private readonly bool returnAll;
        private readonly bool approximation;
        private readonly int stepSize;
        private readonly bool[,] graphG;
        private readonly bool[,] graphH;
        private readonly bool edgeVersion;
        private List<(List<int>, List<int>)> bestFuture;
        private List<(int, int)> maxMappingEdge;
        private int bestMetric;
        private int ExpectedSize;
        private int bestFutureWorth;

        public McSplitAlgorithmSolver(bool[,] graphG, bool[,] graphH, bool edgeVersion, bool returnAll, bool approximation = false, int stepSize = 0)
        {
            this.graphG = graphG;
            this.graphH = graphH;
            this.edgeVersion = edgeVersion;
            this.returnAll = returnAll;
            this.approximation = approximation;
            this.stepSize = stepSize;
        }

        public List<List<(int, int)>> Solve()
        {
            var startingFuture = new List<(List<int>, List<int>)>()
                {(Enumerable.Range(0, graphG.GetLength(0)).ToList(), Enumerable.Range(0, graphH.GetLength(0)).ToList())};

            var startingMapping = new List<(int, int)>();
            bestFuture = startingFuture;
            maxMappings.Add(startingMapping);

            if (approximation)
            {
                if (edgeVersion)
                {
                    do
                    {
                        FindRecursiveEdge(bestFuture, maxMappings[0]);
                        ExpectedSize = ExpectedSize + stepSize;
                    } while (bestFuture.Any(x => Helpers.IsClassConnected(x, maxMappings[0], graphG, graphH)));
                    return new List<List<(int, int)>> { maxMappingEdge };
                }
                else
                {
                    do
                    {
                        FindRecursive(bestFuture, maxMappings[0]);
                        ExpectedSize = ExpectedSize + stepSize;
                    } while (bestFuture.Any(x => Helpers.IsClassConnected(x, maxMappings[0], graphG, graphH)));
                }
            }
            else
            {
                if (edgeVersion)
                    McSplitRecursiveEdgesVersion(startingFuture, startingMapping, 0);
                else
                    McSplitRecursive(startingFuture, startingMapping);
            }

            return maxMappings;
        }

        private void FindRecursive(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            if (UpdateMappingCheckFinishedApproximation(future, mapping))
                return;

            var (g, h) = future.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, graphG, graphH));
            if (g == null)
                return;

            var v = g[0];

            foreach (var w in h)
            {
                List<(List<int>, List<int>)> futurePrim = GetNewFuture(future, v, w);
                var newMapping = mapping.ToList();
                newMapping.Add((v, w));
                FindRecursive(futurePrim, newMapping);
            }

            RemoveGFromFuture(future, g, h, v);
            FindRecursive(future, mapping);
        }

        private void FindRecursiveEdge(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            if (UpdateMappingCheckFinishedApproximationEdge(future, mapping))
                return;

            var (g, h) = future.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, graphG, graphH));
            if (g == null)
                return;

            var v = g[0];
            foreach (var w in h)
            {
                List<(List<int>, List<int>)> futurePrim = GetNewFuture(future, v, w);
                var newMapping = mapping.ToList();
                newMapping.Add((v, w));
                FindRecursiveEdge(futurePrim, newMapping);
            }

            RemoveGFromFuture(future, g, h, v);
            FindRecursiveEdge(future, mapping);
        }

        void McSplitRecursive(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            if (UpdateMappingCheckFinished(future, mapping))
                return;

            var (g, h) = future.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, graphG, graphH));
            if (g == null)
                return;

            var v = g[0];
            foreach (var w in h)
            {
                List<(List<int>, List<int>)> futurePrim = GetNewFuture(future, v, w);
                var newMapping = mapping.ToList();
                newMapping.Add((v, w));
                McSplitRecursive(futurePrim, newMapping);
            }

            RemoveGFromFuture(future, g, h, v);
            McSplitRecursive(future, mapping);
        }

        void McSplitRecursiveEdgesVersion(List<(List<int>, List<int>)> future, List<(int, int)> mapping, int edgesCount)
        {
            if (UpdateMappingCheckFinishedEdge(future, mapping, edgesCount))
                return;

            var (g, h) = future.FirstOrDefault(f => Helpers.IsClassConnected(f, mapping, graphG, graphH));
            if (g == null) return;

            var v = g.First();
            foreach (var w in h)
            {
                List<(List<int>, List<int>)> futurePrim = GetNewFuture(future, v, w);
                var newMapping = mapping.ToList();
                newMapping.Add((v, w));
                var addedEdges = mapping.Where(pair => graphG[v, pair.Item1]).Count();
                McSplitRecursiveEdgesVersion(futurePrim, newMapping, edgesCount + addedEdges);
            }

            RemoveGFromFuture(future, g, h, v);
            McSplitRecursiveEdgesVersion(future, mapping, edgesCount);
        }

        private bool UpdateMappingCheckFinished(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            if (mapping.Count > maxMappingSize)
            {
                maxMappings.Clear();
                maxMappings.Add(mapping);
                maxMappingSize = mapping.Count;
            }
            else if (mapping.Count == maxMappingSize)
                maxMappings.Add(mapping);

            var maximumPossible = mapping.Count + future.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count));
            if (maximumPossible < maxMappingSize || (!returnAll && maximumPossible == maxMappingSize))
                return true;

            return false;
        }

        private bool UpdateMappingCheckFinishedApproximation(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            var mappingValue = future.Sum(x => x.Item1.Count * x.Item2.Count);
            if (mapping.Count > maxMappingSize || (mapping.Count == maxMappingSize && mappingValue > bestFutureWorth))
            {
                maxMappings.Clear();
                maxMappings.Add(new List<(int, int)>(mapping));
                maxMappingSize = mapping.Count;
                bestFutureWorth = mappingValue;
                bestFuture = future.ToList();
            }

            if (mapping.Count == ExpectedSize || future.Count == 0)
                return true;

            var maximumPossible = mapping.Count + future.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count));
            if (maximumPossible <= maxMappingSize)
                return true;

            return false;
        }

        List<(List<int>, List<int>)> RecreateFuture(List<(int, int)> mapping)
        {
            var takenA = new bool[graphG.GetLength(0)];
            var takenB = new bool[graphH.GetLength(0)];

            for (int i = 0; i < mapping.Count; i++)
            {
                takenA[mapping[i].Item1] = true;
                takenB[mapping[i].Item2] = true;
            }

            var future = new Dictionary<ulong, (List<int>, List<int>)>();

            for (int i = 0; i < takenA.Length; i++)
            {
                if (takenA[i])
                    continue;

                ulong vertexClassNumber = 0;

                for (int j = 0; j < mapping.Count; j++)
                    if (graphG[i, mapping[j].Item1])
                        vertexClassNumber |= (1ul << j);

                (List<int>, List<int>) vertexClass;
                if (future.TryGetValue(vertexClassNumber, out vertexClass))
                    vertexClass.Item1.Add(i);
                else
                    future.Add(vertexClassNumber, (new List<int> { i }, new List<int>()));
            }

            for (int i = 0; i < takenB.Length; i++)
            {
                if (takenB[i])
                    continue;

                ulong vertexClassNumber = 0;

                for (int j = 0; j < mapping.Count; j++)
                    if (graphH[i, mapping[j].Item2])
                        vertexClassNumber |= (1ul << j);

                (List<int>, List<int>) vertexClass;
                if (future.TryGetValue(vertexClassNumber, out vertexClass))
                    vertexClass.Item2.Add(i);
            }

            return future.Select(x => x.Value).Where(x => x.Item2.Count > 0).ToList();
        }

        private bool UpdateMappingCheckFinishedApproximationEdge(List<(List<int>, List<int>)> future, List<(int, int)> mapping)
        {
            var mappingValue = Helpers.GetEdgeCount(mapping, graphG);
            if (mapping.Count > maxMappingSize || (mapping.Count == maxMappingSize && mappingValue < bestFutureWorth))
            {
                maxMappings.Clear();
                maxMappings.Add(new List<(int, int)>(mapping));
                maxMappingSize = mapping.Count;
                bestFutureWorth = mappingValue;
                bestFuture = future.ToList();
            }

            var metric = mappingValue + mapping.Count;
            if (metric > bestMetric)
            {
                bestMetric = metric;
                maxMappingEdge = mapping.ToList();
            }

            if (mapping.Count == ExpectedSize || future.Count == 0)
                return true;

            return false;
        }

        private bool UpdateMappingCheckFinishedEdge(List<(List<int>, List<int>)> future, List<(int, int)> mapping, int edgesCount)
        {
            int maxMetric, possibleMetric;

            var currentMetric = mapping.Count + edgesCount;
            maxMetric = maxMappingSize + maxEdgesCount;
            if (currentMetric > maxMetric)
            {
                maxMappings.Clear();
                maxMappings.Add(mapping);
                maxMappingSize = mapping.Count;
                maxEdgesCount = edgesCount;
            }
            else if (currentMetric == maxMetric)
            {
                maxMappings.Add(mapping);
            }

            var possibleVerticesToAdd = future.Sum(lists => Min(lists.Item1.Count, lists.Item2.Count));
            int possibleEdgesToAdd = Enumerable.Range(mapping.Count, possibleVerticesToAdd).Sum();
            possibleMetric = currentMetric + possibleVerticesToAdd + possibleEdgesToAdd;

            if (possibleMetric < maxMetric || (!returnAll && possibleMetric == maxMetric))
                return true;

            return false;
        }

        private static void RemoveGFromFuture(List<(List<int>, List<int>)> future, List<int> g, List<int> h, int v)
        {
            var gWithoutV = g.Where(x => x != v).ToList();
            future.Remove((g, h));
            if (gWithoutV.Count() > 0)
                future.Add((gWithoutV, h));
        }

        private List<(List<int>, List<int>)> GetNewFuture(List<(List<int>, List<int>)> future, int v, int w)
        {
            var futurePrim = new List<(List<int>, List<int>)>();
            foreach (var (gPrim, hPrim) in future)
            {
                var gBis = new List<int>();
                foreach (var vertexG in gPrim)
                    if (graphG[v, vertexG])
                        gBis.Add(vertexG);

                var hBis = new List<int>();
                foreach (var vertexh in hPrim)
                    if (graphH[w, vertexh])
                        hBis.Add(vertexh);

                if (gBis.Count() > 0 && hBis.Count() > 0)
                    futurePrim.Add((gBis, hBis));

                gBis = new List<int>();
                foreach (var vertexG in gPrim)
                    if (!graphG[v, vertexG] && v != vertexG)
                        gBis.Add(vertexG);
                hBis = new List<int>();
                foreach (var vertexh in hPrim)
                    if (!graphH[w, vertexh] && vertexh != w)
                        hBis.Add(vertexh);

                if (gBis.Count() > 0 && hBis.Count() > 0)
                    futurePrim.Add((gBis, hBis));
            }

            return futurePrim;
        }
    }
}
