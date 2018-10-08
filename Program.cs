using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace taio
{
    class Program
    {
        static void Main(string[] args)
        {
            var separator = ',';
            if (args.Length < 2)
            {
                Console.WriteLine("usage: solver <input_file> [<delimiter>]");
                return;
            }
            if (args.Length >= 3)
            {
                separator = args[2][0];
            }

            ReadCsv
        }

        static bool[,] ReadCsv(string path, char separator)
        {
            var file = File.ReadAllLines(path); // spróbujmy nie udusić systemu
            if (file == null)
                throw new Exception("file not found");

            var graph = file.Select(x => x.Split(separator).Select(y => y == "1").ToArray()).ToArray();
            if (graph.Any(x => x.Length != graph.Length))
                throw new ArgumentException("Macierz sąsiedztwa nie jest kwadratowa!");

            var matrix = new bool[graph.Length, graph.Length];
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    matrix[i, j] = graph[i][j];

            return matrix;
        }
    }

    public interface IGraphSolver
    {
        IList<(int A, int B)> FindCommonSubgraph(bool[,] graphA, bool[,] graphB);
    }
}