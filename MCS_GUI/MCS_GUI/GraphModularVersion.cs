using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Taio
{
    public class GraphModularVersion
    {
        public int Size { get; }
        public bool[,] vertices;
        private int[] degrees;
        public int? size2 { set; private get; }

        public GraphModularVersion(bool[,] _vertices)
        {
            Size = _vertices.GetLength(0);
            this.vertices = new bool[Size, Size];

            if (_vertices.GetLength(0) != _vertices.GetLength(1))
            {
                throw new System.ArgumentException("Matrix must be square");
            }

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    this.vertices[i, j] = _vertices[i, j];
                }
            }

            EvaluateVerticesDegrees();
        }

        private void EvaluateVerticesDegrees()
        {
            degrees = new int[this.Size];

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    degrees[i] += vertices[i, j] ? 1 : 0;
                }
            }
        }

        public int Degree(int v)
        {
            return this.degrees[v];
        }

        public List<int> GetEdges(int v)
        {
            var result = new List<int>();

            for (int i = 0; i < Size; i++)
            {
                if (this.vertices[v, i])
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public bool isEdgeBetween(int v1, int v2)
        {
            return this.vertices[v1, v2];
        }

        public override string ToString()
        {
            return this.ToString(true);
        }

        public string ToString(bool modular = true)
        {
            if (modular == false)
            {
                throw new NotImplementedException();
            }

            var sb = new StringBuilder("    ");
            for (int i = 0; i < this.Size; i++)
            {
                sb.Append($"{(int)(i / size2)}{(int)(i % size2)} ");
            }
            sb.Append(System.Environment.NewLine);
            sb.Append(System.Environment.NewLine);

            for (int i = 0; i < this.Size; i++)
            {
                sb.Append($"{(int)(i / size2)}{(int)(i % size2)}  ");

                for (int j = 0; j < this.Size; j++)
                {
                    sb.Append(this.vertices[i, j] ? "1  " : "0  ");
                }
                sb.Append(System.Environment.NewLine);
            }
            return sb.ToString();
        }

    }
}
