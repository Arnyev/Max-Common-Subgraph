using System.Collections.Generic;
using System.Linq;
using System;

namespace taio
{
    using System.Collections.ObjectModel;

    public interface IGraphSolver
    {
        List<(uint A, uint B)> FindCommonSubgraph(bool[,] graphA, bool[,] graphB);
    }

    public class MaxInducedSubgraphCliqueApproximation : IGraphSolver
    {
        public List<(uint A, uint B)> FindCommonSubgraph(bool[,] graphA, bool[,] graphB)
        {
            var G1 = new GraphMichau(graphA);
            var G2 = new GraphMichau(graphB);

            var isomorphismSolver = new MaxClique();
            var G = isomorphismSolver.GetModularProduct(G1, G2);

            //Console.WriteLine(G.ToString()); 

            isomorphismSolver.MaxCliqueHeu(G, 1);
            var result = isomorphismSolver.DecomposeModularGraph(G1, G2, isomorphismSolver.CliqueVertices);
            var realResult = isomorphismSolver.GetMaximumConnectedGraph(G1, result);

            return realResult;
        }
    }

    public class MaxClique
    {
        public int Value;
        public List<int> CliqueVertices;

        public GraphMichau GetModularProduct(GraphMichau G1, GraphMichau G2)
        {
            var N = G1.Size;
            var M = G2.Size;
            var newSize = M * N;
            var newTab = new bool[newSize, newSize];

            for (int x = 0; x < newSize; x++)
            {
                for (int y = 0; y < newSize; y++)
                {
                    if (x == y) continue;
                    int v1 = x / M;
                    int w1 = y / M;
                    int v2 = x % M;
                    int w2 = y % M;
                    if (v1 == w1 || v2 == w2) continue;

                    if (G1.isEdgeBetween(v1, w1) && G2.isEdgeBetween(v2, w2) ||
                        !G1.isEdgeBetween(v1, w1) && !G2.isEdgeBetween(v2, w2))
                    {
                        newTab[x, y] = newTab[y, x] = true;
                    }
                }
            }

            var G = new GraphMichau(newTab);
            G.size2 = M;
            return G;
        }

        public List<(uint a, uint b)> DecomposeModularGraph(GraphMichau G1, GraphMichau G2, List<int> vertices)
        {
            var N = G1.Size;
            var M = G2.Size;
            var newSize = M * N;
            var newTab = new bool[newSize, newSize];

            var result = new List<(uint a, uint b)>();
            foreach (var v in vertices)
            {
                var _a = (uint)(v / M);
                var _b = (uint)(v % M);
                result.Add((a:_a, b:_b));
            }

            return result;
        }

     
        public void MaxCliqueHeu(GraphMichau G, int lowerBound)
        {
            var U = new List<int>();
            Value = lowerBound;
            CliqueVertices = new List<int>();
            List<int> currentClique = new List<int>();

            for (int i = 0; i < G.Size; i++)
            {
                if (G.Degree(i) >= Value)
                {
                    currentClique.Add(i);
                    U = new List<int>();
                    var neighbours = G.GetEdges(i);
                    for (int j = 0; j < neighbours.Count; j++)
                    {
                        var v_j = neighbours[j];
                        if (G.Degree(v_j) >= Value)
                        {
                            U.Add(v_j);
                        }
                    }
                    CliqueHeu(G, U, 1, currentClique);
                    currentClique.Remove(i);
                }
            }
        }

        public List<(uint a, uint b)> GetMaximumConnectedGraph(GraphMichau G, List<(uint a, uint b)> vertices)
        {
            var tmpList = new List<int>();
            foreach (var v in vertices)
            {
                tmpList.Add((int)v.a);
            }

            var graphsList = FindAllConnectedSubgraphs(G, tmpList);
            var isomorphism = new Dictionary<uint, uint>();
            foreach (var tup in vertices)
            {
                isomorphism[tup.a] = tup.b;
            }

            List<int> max = null;
            int maxCount = 0;
            foreach (var g in graphsList)
            {
                if (g.Count > maxCount)
                {
                    maxCount = g.Count;
                    max = g;
                }
            }

            var result = new List<(uint A, uint B)>();
            foreach (var v in max)
            {
                result.Add((a:(uint)v, b:isomorphism[(uint)v]));
            }

            return result;
        }

        private List<List<int>> FindAllConnectedSubgraphs(GraphMichau G, List<int> vertices)
        {
            if (G.Size == 0) return null;
            var verticesLeft = new SortedSet<int>();
            visited = new bool[G.Size];

            foreach (var v in vertices)
            {
                verticesLeft.Add(v);
            }

            var result = new List<List<int>>();
            while (verticesLeft.Any())
            {
                var partialResult = new List<int>();
                DFS(G, verticesLeft.First(), partialResult);
                result.Add(partialResult);
                verticesLeft.RemoveWhere(v => partialResult.Contains(v));
            }

            return result;
        }

        private bool[] visited;
        private void DFS(GraphMichau G, int v, List<int> result)
        {
            visited[v] = true;
            result.Add(v);

            foreach (var vertex in G.GetEdges(v))
            {
                if (!visited[vertex])
                    DFS(G, vertex, result);
            }
        }

        private void CliqueHeu(GraphMichau G, List<int> U, int size, List<int> currentClique)
        {
            if (U == null || U.Count == 0)
            {
                if (size > Value)
                {
                    Value = size;
                    CliqueVertices = new List<int>();
                    CliqueVertices.AddRange(currentClique);
                }
                return;
            }

            int max_deg = -1;
            var vertex = -1;
            foreach (var u in U)
            {
                if (G.Degree(u) > max_deg)
                {
                    max_deg = G.Degree(u);
                    vertex = u;
                }
            }

            U.Remove(vertex);
            currentClique.Add(vertex);

            var vertexNeighbours = G.GetEdges(vertex);
            var vertexNeighboursPrim = new List<int>();
            vertexNeighboursPrim.AddRange(vertexNeighbours.Where((v) => G.Degree(v) >= Value));

            var newSet = new List<int>();
            newSet.AddRange(vertexNeighboursPrim.Where((v) => U.Contains(v)));

            CliqueHeu(G, newSet, size + 1, currentClique);
            currentClique.Remove(vertex);
        }
    }
}
