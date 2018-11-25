using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using taio;

namespace Taio
{
    public class Program
    {
        static void Main(string[] args)
        {
            var delimiter = ',';
            var algorithmVersionFlag = false;
            var algorithmNumber = 1;
            var quiet = false;
            var outputFile = "result.csv";
            string graph1File = null;
            string graph2File = null;
            var usageMessage = "usage: MaxCommonSubgraph.exe input1 input2 [--delimiter char] [--algo alg_number] [--output filename] [--quiet]";

            if (args.Length < 2)
            {
                Console.WriteLine(usageMessage);
                Console.WriteLine("You can choose one of 6 algorithms:");
                Console.WriteLine("       1 - exact (maximizing V), default");
                Console.WriteLine("       2 - exact (maximizing V+E)");
                Console.WriteLine("       3 - approximating algorithm A (V)");
                Console.WriteLine("       4 - approximating algorithm A (V+E)");
                Console.WriteLine("       5 - approximating algorithm B (V)");
                Console.WriteLine("       6 - approximating algorithm B (V+E)");
                return;
            }

            bool delimiterFlag = false;
            bool outputFlag = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (algorithmVersionFlag)
                {
                    algorithmVersionFlag = false;
                    int.TryParse(args[i], out algorithmNumber);
                    continue;
                }
                if (delimiterFlag)
                {
                    delimiterFlag = false;
                    delimiter = args[i][0];
                    continue;
                }
                if (outputFlag)
                {
                    outputFlag = false;
                    outputFile = args[i];
                    continue;
                }
                switch (args[i])
                {
                    case "--delimiter":
                        delimiterFlag = true;
                        break;
                    case "--algo":
                        algorithmVersionFlag = true;
                        break;
                    case "--output":
                        outputFlag = true;
                        break;
                    case "--quiet":
                        quiet = true;
                        break;
                    default:
                        if (graph1File != null) graph2File = args[i];
                        else graph1File = args[i];
                        break;
                }
            }
            if (graph1File == null || graph2File == null)
            {
                Console.WriteLine(usageMessage);
                return;
            }

            Graph graph1 = null, graph2 = null;
            try
            {
                graph1 = DeserializeGraphFromCsv(graph1File, delimiter);
                graph2 = DeserializeGraphFromCsv(graph2File, delimiter);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Cannot find file: {e.FileName}");
                return;
            }

            var g1 = DeserializeG(graph1File, delimiter);
            var g2 = DeserializeG(graph2File, delimiter);

            List<List<(uint, uint)>> results = null;
            List<(uint, uint)> result = null;

            var edgeVersion = true;
            switch (algorithmNumber)
            {
                case 1:
                    edgeVersion = false;
                    goto case 2;
                case 2:
                    results = McSplitAlgorithm.McSplit(graph1, graph2, edgeVersion);
                    break;
                case 3:
                    edgeVersion = false;
                    goto case 4;
                case 4:
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion);
                    break;
                case 5:
                    result = McSplitApproximation.Find(g1, g2, 4);
                    break;
                case 6:
                    result = McSplitApproximationEdge.Find(g1, g2, 4);
                    break;
                default:
                    Console.WriteLine("Wrong algorithm number!");
                    return;
            }


            if (results is null)
            {
                using (var file = new StreamWriter(outputFile))
                {
                    file.WriteLine(string.Join(",", result.Select(pair => pair.Item1)));
                    file.WriteLine(string.Join(",", result.Select(pair => pair.Item2)));
                    file.WriteLine();
                }
                if (!quiet)
                {
                    Console.WriteLine($"=== Common induced subgraph ===");
                    PrintResult(result);
                }
            }
            else
            {
                using (var file = new StreamWriter(outputFile))
                {
                    foreach (var r in results)
                    {
                        file.WriteLine(string.Join(",", r.Select(pair => pair.Item1)));
                        file.WriteLine(string.Join(",", r.Select(pair => pair.Item2)));
                        file.WriteLine();
                    }
                }
                if (!quiet)
                {
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine($"=== Maximum common induced subgraph no. {i + 1} ===");
                        PrintResult(results[i]);
                    }
                }
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
                    throw new ArgumentException("Provided adjacency matrix is not a square matrix!");
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
                    throw new ArgumentException("Provided adjacency matrix is not a square matrix!");
                }

                for (int j = 0; j < nodesNumber; j++)
                {
                    matrix[i, j] = row[j] == "1";
                }
            }

            return matrix;
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

        public static void SaveIsomorphismListToCsv(IList<(int a, int b)> list)
        {
            string filePath = @"..\..\..\Isomorphism.csv";
            string delimiter = ",";
            var N = list.Count;

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            for (int i = 0; i < N; i++)
            {
                sb1.Append(list[i].a);
                sb2.Append(list[i].b);
                if (i == N - 1)
                {
                    sb1.Append("\r\n");
                    sb2.Append("\r\n");
                }
                else
                {
                    sb1.Append(delimiter);
                    sb2.Append(delimiter);
                }
            }

            File.WriteAllText(filePath, sb1.ToString());
            File.AppendAllText(filePath, sb2.ToString());
        }
    }
}
