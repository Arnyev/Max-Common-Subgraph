using System;
using System.Collections.Generic;
using System.Linq;

namespace Taio
{
    class Approximation
    {
        static void BronKerbosch(List<int> current, bool[,] graph, List<int> remaining, List<int> best, bool[,] graphB)
        {
            if (!CheckConnect(current, graphB))
                return;

            if (best.Count < current.Count)
            {
                best.Clear();
                best.AddRange(current);
            }

            if (remaining.Count == 0)
                return;


            var newRemaining = new List<int>();

            for (int i = 0; i < remaining.Count; i++)
            {
                var v1 = remaining[i];
                newRemaining.Clear();
                for (int j = i + 1; j < remaining.Count; j++)
                {
                    var v2 = remaining[j];
                    if (graph[v1, v2])
                        newRemaining.Add(v2);
                }

                current.Add(v1);
                BronKerbosch(current, graph, newRemaining, best, graphB);
                current.RemoveAt(current.Count - 1);
            }
        }

        static bool[,] GraphProduct(bool[,] graphA, bool[,] graphB)
        {
            var sizeA = graphA.GetLength(0);
            var sizeB = graphB.GetLength(0);
            var newGraph = new bool[sizeA * sizeB, sizeA * sizeB];

            for (int i = 0; i < sizeA; i++)
                for (int j = 0; j < sizeB; j++)
                    for (int k = i + 1; k < sizeA; k++)
                        for (int l = j + 1; l < sizeB; l++)
                            if (!(graphA[i, k] ^ graphB[j, l]))
                                newGraph[i * sizeB + j, k * sizeB + l] = newGraph[k * sizeB + l, i * sizeB + j] = true;

            return newGraph;
        }

        public static bool CheckConnect(List<int> vertices, bool[,] graphB)
        {
            if (vertices.Count < 2)
                return true;

            var sizeB = graphB.GetLength(0);
            vertices = vertices.Select(x => x % sizeB).ToList();
            foreach (var v1 in vertices)
            {
                var connected = false;
                foreach (var v2 in vertices)
                    if (graphB[v1, v2])
                    {
                        connected = true;
                        break;
                    }
                if (!connected)
                    return false;
            }

            return true;
        }

        public static List<List<(uint, uint)>> SolveMaxCommon(bool[,] graphA, bool[,] graphB)
        {
            var sizeB = graphB.GetLength(0);

            var product = GraphProduct(graphA, graphB);
            var best = new List<int>();
            BronKerbosch(new List<int>(), product, Enumerable.Range(0, product.GetLength(0)).ToList(), best, graphB);
            var returnVal = new List<List<(uint, uint)>>();
            var result = new List<(uint, uint)>();
            foreach (var vert in best)
                result.Add(((uint)(vert / sizeB), (uint)(vert % sizeB)));

            returnVal.Add(result);
            return returnVal;
        }
    }
}