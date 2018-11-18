using System.Collections.Generic;

namespace Taio
{
    class Graph
    {
        public bool[,] _graph;
        private uint _size;

        public Graph(uint size)
        {
            _graph = new bool[size, size];
            _size = size;
        }

        public Graph(uint size, bool[,] graph)
        {
            _graph = graph;
            _size = size;
        }

        public uint Size => _size;

        public bool AreAdjacent(uint u, uint v)
        {
            return _graph[u, v];
        }

        public List<uint> GetNeighbours(uint v)
        {
            var result = new List<uint>();
            for (uint i = 0; i < _size; i++)
            {
                if (i == v) continue;
                if (_graph[v, i]) result.Add(i);
            }
            return result;
        }

        public List<uint> GetNonNeighbours(uint v)
        {
            var result = new List<uint>();
            for (uint i = 0; i < _size; i++)
            {
                if (i == v) continue;
                if (!_graph[v, i]) result.Add(i);
            }
            return result;
        }
    }
}