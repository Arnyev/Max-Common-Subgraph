﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Taio
{
    public class Program
    {
        static void Main(string[] args)
        {
            var delimiter = ',';
            if (args.Length < 2)
            {
                Console.WriteLine("usage: solver <input_file_1> <input_file_2>[<delimiter>]");
                return;
            }
            if (args.Length >= 3)
            {
                delimiter = args[2][0];
            }

            var graph1 = DeserializeGraphFromCsv(args[0], delimiter);
            var graph2 = DeserializeGraphFromCsv(args[1], delimiter);

            var result = McSplitAlgorithm.McSplit(graph1, graph2);
            var r2 = McSplitApproximation.Find(graph1, graph2, 4);

            for (int i = 0; i < result.Count; i++)
            {
                Console.WriteLine($"=== Maximum common induced subgraph no. {i + 1} ===");
                PrintResult(result[i]);
            }


            var g1 = DeserializeG(args[0], delimiter);
            var g2 = DeserializeG(args[1], delimiter);

            var r = Approximation.SolveMaxCommon(g1, g2);

            for (int i = 0; i < r.Count; i++)
            {
                Console.WriteLine($"=== M common induced subgraph no. {i + 1} ===");
                PrintResult(r[i]);
            }


        }

        static Graph DeserializeGraphFromCsv(string csvPath, char separator)
        {
            var file = File.ReadAllLines(csvPath);

            var nodesNumber = (uint)file.Length;
            var matrix = new bool[nodesNumber, nodesNumber];

            for (int i = 0; i < nodesNumber; i++)
            {
                var row = file[i].Split(separator).Select(x => new string(x.Where(char.IsDigit).ToArray())).ToArray();
                if (row.Length != nodesNumber)
                {
                    throw new ArgumentException("Macierz sąsiedztwa nie jest kwadratowa!");
                }

                for (int j = 0; j < nodesNumber; j++)
                {
                    if (row[j] != "1" && row[j] != "0")
                        throw new Exception();

                    matrix[i, j] = row[j] == "1";
                }
            }

            for (int i = 0; i < nodesNumber; i++)
                for (int j = 0; j < nodesNumber; j++)
                    if (matrix[i, j] != matrix[j, i])
                        throw new Exception();

            return new Graph(nodesNumber, matrix);
        }

        private static Graph Generate(int nodesNumber, double density)
        {
            var matrix = new bool[nodesNumber, nodesNumber];
            var rand = new Random();
            for (int i = 0; i < nodesNumber - 1; i++)
                for (int j = i + 1; j < nodesNumber; j++)
                    matrix[i, j] = matrix[j, i] = rand.NextDouble() < density;

            return new Graph((uint)nodesNumber, matrix);
        }

        static bool[,] DeserializeG(string csvPath, char separator)
        {
            var file = File.ReadAllLines(csvPath);

            var nodesNumber = (uint)file.Length;
            var matrix = new bool[nodesNumber, nodesNumber];

            for (int i = 0; i < nodesNumber; i++)
            {
                var row = file[i].Split(separator);
                if (row.Length != nodesNumber)
                {
                    throw new ArgumentException("Macierz sąsiedztwa nie jest kwadratowa!");
                }

                for (int j = 0; j < nodesNumber; j++)
                {
                    matrix[i, j] = row[j] == "1";
                }
            }

            return matrix;
        }

        static void PrintResult(List<(uint, uint)> result)
        {
            Console.WriteLine("G      H");
            Console.WriteLine("--------");
            foreach (var mapping in result)
            {
                Console.WriteLine($"{mapping.Item1 + 1} <==> {mapping.Item2 + 1}");
            }
            Console.WriteLine();
        }
    }
}

