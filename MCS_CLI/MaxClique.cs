using System.Collections.Generic;
using System.Linq;

namespace Taio
{
    public class CliqueSolver
    {
        private readonly bool[,] graphA;
        private readonly bool[,] graphB;
        private readonly bool edgeVersion;
        private readonly bool[,] modularGraph;
        private readonly int[] neighbourCounts;
        private readonly List<int> maxClique = new List<int>();

        public CliqueSolver(bool[,] graphA, bool[,] graphB, bool edgeVersion = false)
        {
            this.graphA = graphA;
            this.graphB = graphB;
            this.edgeVersion = edgeVersion;
            modularGraph = GetModularProduct(graphA, graphB);
            neighbourCounts = CreateNeighbourCounts(modularGraph);
        }

        public List<(int, int)> Solve()
        {
            if (graphA.GetLength(1) == 1 || graphB.GetLength(1) == 1)
                return new List<(int, int)>(new[] { (0, 0) });

            for (int i = 0; i < modularGraph.GetLength(0); i++)
                if (neighbourCounts[i] >= maxClique.Count - 1)
                    FindCliqueWithVertex(i);

            var graphBSize = graphB.GetLength(0);
            var decomposed = maxClique.Select(v => (v / graphBSize, v % graphBSize)).ToList();
            return GetMaximumConnectedGraph(graphA, decomposed);
        }

        private void FindCliqueWithVertex(int i)
        {
            List<int> clique = new List<int> { i };
            var remaining = new List<int>();

            for (int j = 0; j < modularGraph.GetLength(0); j++)
                if (modularGraph[i, j] && neighbourCounts[j] >= maxClique.Count)
                    remaining.Add(j);

            while (remaining.Count > maxClique.Count - clique.Count && remaining.Count > 0)
            {
                int vertex = edgeVersion ? SelectVertexVE(remaining, clique) : SelectVertex(remaining);

                remaining.Remove(vertex);
                clique.Add(vertex);

                var newRemaining = new List<int>();
                foreach (var remainingVertex in remaining)
                    if (modularGraph[vertex, remainingVertex] &&
                        neighbourCounts[remainingVertex] >= maxClique.Count)
                        newRemaining.Add(remainingVertex);

                remaining = newRemaining;
            }

            if (clique.Count > maxClique.Count)
            {
                maxClique.Clear();
                maxClique.AddRange(clique);
            }
        }

        private int SelectVertex(List<int> verticesSelectable)
        {
            var best = -1;
            var bestNeighbours = -1;
            foreach (var vertex in verticesSelectable)
                if (neighbourCounts[vertex] >= bestNeighbours)
                {
                    best = vertex;
                    bestNeighbours = neighbourCounts[vertex];
                }

            return best;
        }

        private int SelectVertexVE(List<int> verticesSelectable, List<int> verticesSelected)
        {
            var graphBSize = graphB.GetLength(0);
            var bestValue = -1;
            var selectedVertice = -1;

            var verticesSelectedinA = new List<int>();
            foreach (var vertex in verticesSelected)
                verticesSelectedinA.Add(vertex / graphBSize);

            for (int i = 0; i < verticesSelectable.Count; i++)
            {
                var selectable = verticesSelectable[i] / graphBSize;
                var value = 0;
                foreach (var selected in verticesSelectedinA)
                    if (graphA[selectable, selected])
                        value++;

                if (value > bestValue)
                {
                    bestValue = value;
                    selectedVertice = verticesSelectable[i];
                }
            }
            return selectedVertice;
        }

        private static List<(int, int)> GetMaximumConnectedGraph(bool[,] G, List<(int, int)> vertices)
        {
            var graphsList = FindAllConnectedSubgraphs(G, vertices);

            var max = new List<(int, int)>();
            foreach (var graph in graphsList)
                if (graph.Count > max.Count)
                    max = graph;

            return max;
        }

        private static List<List<(int, int)>> FindAllConnectedSubgraphs(bool[,] graph, List<(int, int)> vertices)
        {
            var visited = new bool[vertices.Count];
            var queue = new Queue<(int, int)>();

            var result = new List<List<(int, int)>>();
            for (int i = 0; i < vertices.Count; i++)
            {
                if (!visited[i])
                {
                    var list = new List<(int, int)>();
                    list.Add(vertices[i]);
                    queue.Enqueue(vertices[i]);
                    visited[i] = true;

                    while (queue.Count != 0)
                    {
                        var vertex = queue.Dequeue();

                        for (int j = 0; j < vertices.Count; j++)
                            if (!visited[j] && graph[vertex.Item1, vertices[j].Item1])
                            {
                                queue.Enqueue(vertices[j]);
                                visited[j] = true;
                                list.Add(vertices[j]);
                            }
                    }
                    result.Add(list);
                }
            }

            return result;
        }

        private static bool[,] GetModularProduct(bool[,] graphA, bool[,] graphB)
        {
            var N = graphA.GetLength(0);
            var M = graphB.GetLength(0);
            var newSize = M * N;
            var modularGraph = new bool[newSize, newSize];

            for (int x = 0; x < newSize; x++)
            {
                for (int y = 0; y < newSize; y++)
                {
                    if (x == y)
                        continue;
                    int v1 = x / M;
                    int w1 = y / M;
                    int v2 = x % M;
                    int w2 = y % M;
                    if (v1 == w1 || v2 == w2)
                        continue;

                    if (graphA[v1, w1] && graphB[v2, w2] || !graphA[v1, w1] && !graphB[v2, w2])
                        modularGraph[x, y] = modularGraph[y, x] = true;
                }
            }

            return modularGraph;
        }

        private static int[] CreateNeighbourCounts(bool[,] graph)
        {
            var neighbours = new int[graph.GetLength(0)];

            for (int i = 0; i < neighbours.Length; i++)
            {
                for (int j = 0; j < neighbours.Length; j++)
                    if (graph[i, j])
                        neighbours[i]++;
            }

            return neighbours;
        }
    }
}
