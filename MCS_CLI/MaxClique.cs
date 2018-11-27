using System.Collections.Generic;
using System.Linq;

namespace taio
{
    public class MaxInducedSubgraphCliqueApproximation
    {
        public List<(int A, int B)> FindCommonSubgraph(bool[,] graphA, bool[,] graphB, bool edgeVersion = false)
        {
            var G1 = new GraphModularVersion(graphA);
            var G2 = new GraphModularVersion(graphB);

            if (G1.Size == 1 || G2.Size == 1)
            {
                return new List<(int A, int B)>( new[] { (A: 0, B: 0) } );
            }

            var isomorphismSolver = new MaxClique(G1, G2);
            var G = isomorphismSolver.GetModularProduct();

            isomorphismSolver.MaxCliqueHeu(G, 1, edgeVersion);
            var result = isomorphismSolver.DecomposeModularGraph(isomorphismSolver.CliqueVertices);
            var realResult = isomorphismSolver.GetMaximumConnectedGraph(G1, result);

            return realResult;
        }
    }

    public class MaxClique
    {
        public int Value;
        public List<int> CliqueVertices;
        private GraphModularVersion G1;
        private GraphModularVersion G2;

        public MaxClique(GraphModularVersion g1, GraphModularVersion g2)
        {
            G1 = g1;
            G2 = g2;
        }

        public GraphModularVersion GetModularProduct()
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

            var G = new GraphModularVersion(newTab);
            G.size2 = M;
            return G;
        }

        public List<(int a, int b)> DecomposeModularGraph(List<int> vertices)
        {
            var N = G1.Size;
            var M = G2.Size;
            var newSize = M * N;
            var newTab = new bool[newSize, newSize];

            var result = new List<(int a, int b)>();
            foreach (var v in vertices)
            {
                var _a = v / M;
                var _b = v % M;
                result.Add((a: _a, b: _b));
            }

            return result;
        }

        public void MaxCliqueHeu(GraphModularVersion G, int lowerBound, bool edgeVersion)
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
                    CliqueHeu(G, G1, G2, U, 1, currentClique, edgeVersion);
                    currentClique.Remove(i);
                }
            }
        }

        public List<(int a, int b)> GetMaximumConnectedGraph(GraphModularVersion G, List<(int a, int b)> vertices)
        {
            var tmpList = new List<int>();
            foreach (var v in vertices)
            {
                tmpList.Add(v.a);
            }

            var graphsList = FindAllConnectedSubgraphs(G, tmpList);
            var isomorphism = new Dictionary<int, int>();
            foreach (var tup in vertices)
            {
                isomorphism[tup.a] = tup.b;
            }

            List<int> max = new List<int>();
            int maxCount = 0;
            foreach (var g in graphsList)
            {
                if (g.Count > maxCount)
                {
                    maxCount = g.Count;
                    max = g;
                }
            }

            var result = new List<(int A, int B)>();
            foreach (var v in max)
            {
                result.Add((v, isomorphism[v]));
            }

            return result;
        }

        private List<List<int>> FindAllConnectedSubgraphs(GraphModularVersion G, List<int> vertices)
        {
            var visited = new bool[vertices.Count];
            var queue = new Queue<int>();

            var result = new List<List<int>>();
            for (int i = 0; i < vertices.Count; i++)
            {
                if (!visited[i])
                {
                    var list = new List<int>();
                    list.Add(vertices[i]);
                    queue.Enqueue(vertices[i]);
                    visited[i] = true;

                    while (queue.Count != 0)
                    {
                        var vertex = queue.Dequeue();

                        for (int j = 0; j < vertices.Count; j++)
                            if (!visited[j] && G.vertices[vertex, vertices[j]])
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

        private void CliqueHeu(GraphModularVersion G, GraphModularVersion G1, GraphModularVersion G2, List<int> U, int size, List<int> currentClique, bool edgeVersion)
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
            int vertex = edgeVersion ? SelectVertexVE(U, currentClique) : SelectVertex(G, U, ref max_deg);

            U.Remove(vertex);
            currentClique.Add(vertex);

            var vertexNeighbours = G.GetEdges(vertex);
            var vertexNeighboursPrim = new List<int>();
            vertexNeighboursPrim.AddRange(vertexNeighbours.Where((v) => G.Degree(v) >= Value));

            var newSet = new List<int>();
            newSet.AddRange(vertexNeighboursPrim.Where((v) => U.Contains(v)));

            CliqueHeu(G, G1, G2, newSet, size + 1, currentClique, edgeVersion);
            currentClique.Remove(vertex);
        }

        private static int SelectVertex(GraphModularVersion G, List<int> U, ref int max_deg)
        {
            var vertex = -1;
            foreach (var u in U)
            {
                if (G.Degree(u) > max_deg)
                {
                    max_deg = G.Degree(u);
                    vertex = u;
                }
            }

            return vertex;
        }

        private int SelectVertexVE(List<int> verticesSelectable, List<int> verticesSelected)
        {
            var decomposedSelectable = DecomposeModularGraph(verticesSelectable);
            var decomposedSelected = DecomposeModularGraph(verticesSelected);
            var bestValue = -1;
            var selectedVertice = -1;
            for (int i = 0; i < decomposedSelectable.Count; i++)
            {
                var selectable = decomposedSelectable[i];
                var value = 0;
                foreach (var selected in decomposedSelected)
                    if (G1.vertices[selectable.a, selected.a])
                        value++;
                if (value > bestValue)
                {
                    bestValue = value;
                    selectedVertice = verticesSelectable[i];
                }
            }
            return selectedVertice;
        }
    }
}
