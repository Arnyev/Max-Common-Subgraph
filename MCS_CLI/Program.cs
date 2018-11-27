using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            RunTimeTest();
            // RunTests();

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
                PrintUsage(usageMessage);
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

            bool[,] graph1 = null, graph2 = null;
            try
            {
                graph1 = DeserializeGraph(graph1File, delimiter);
                graph2 = DeserializeGraph(graph2File, delimiter);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Cannot find file: {e.FileName}");
                return;
            }

            List<List<(int, int)>> results = null;
            List<(int, int)> result = null;
            switch (algorithmNumber)
            {
                case 1:
                    PrintAlgorithmStart(algorithmNumber, "exact (V), first best result");
                    result = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: false, returnAll: false).Solve()[0];
                    break;
                case 2:
                    PrintAlgorithmStart(algorithmNumber, "exact (V+E), first best result");
                    result = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: true, returnAll: false).Solve()[0];
                    break;
                case 3:
                    PrintAlgorithmStart(algorithmNumber, "exact (V), all maximum results");
                    results = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: false, returnAll: true).Solve();
                    break;
                case 4:
                    PrintAlgorithmStart(algorithmNumber, "exact (V+E), all maximum results");
                    results = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: true, returnAll: true).Solve();
                    break;
                case 5:
                    PrintAlgorithmStart(algorithmNumber, "approx. A (V)");
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(graph1, graph2, edgeVersion: false);
                    break;
                case 6:
                    PrintAlgorithmStart(algorithmNumber, "approx. A (V+E)");
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(graph1, graph2, edgeVersion: true);
                    break;
                case 7:
                    PrintAlgorithmStart(algorithmNumber, "approx. B (V)");
                    result = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: false, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
                    break;
                case 8:
                    PrintAlgorithmStart(algorithmNumber, "approx. B (V+E)");
                    result = new McSplitAlgorithmSolver(graph1, graph2, edgeVersion: true, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
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
                    Console.WriteLine("\n### Result below:\n");
                    var prefix = algorithmNumber > 4 ? "Common" : "Maximum common";
                    Console.WriteLine($"=== {prefix} induced subgraph ===");
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
                    Console.WriteLine("\n### Results below:\n");
                    for (int i = 0; i < results.Count; i++)
                    {
                        Console.WriteLine($"=== Maximum common induced subgraph no. {i + 1} ===");
                        PrintResult(results[i]);
                    }
                }
            }
        }

        private static void PrintUsage(string usageMessage)
        {
            Console.WriteLine(usageMessage);
            Console.WriteLine("You can choose one of 6 algorithms:");
            Console.WriteLine("       1 - exact (V), default");
            Console.WriteLine("       2 - exact (V+E)");
            Console.WriteLine("       3 - exact (V) returning all maximum results");
            Console.WriteLine("       4 - exact (V+E) returning all maximum results");
            Console.WriteLine("       5 - approximating algorithm A (V)");
            Console.WriteLine("       6 - approximating algorithm A (V+E)");
            Console.WriteLine("       7 - approximating algorithm B (V)");
            Console.WriteLine("       8 - approximating algorithm B (V+E)");
        }

        private static void PrintAlgorithmStart(int number, string algorithm)
        {
            Console.WriteLine($"\n### Starting algorithm {number} ({algorithm})...");
        }

        static bool[,] DeserializeGraph(string csvPath, char separator)
        {
            var file = File.ReadAllLines(csvPath);

            var nodesNumber = (int)file.Length;
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
                    matrix[i, j] = row[j].Trim() == "1";
                }
            }

            return matrix;
        }

        private static bool[,] Generate(int nodesNumber, double density)
        {
            var matrix = new bool[nodesNumber, nodesNumber];
            var rand = new Random();
            for (int i = 0; i < nodesNumber - 1; i++)
                for (int j = i + 1; j < nodesNumber; j++)
                    matrix[i, j] = matrix[j, i] = rand.NextDouble() < density;

            return matrix;
        }

        static void PrintResult(List<(int, int)> result)
        {
            Console.WriteLine("G      H");
            Console.WriteLine("--------");
            foreach (var mapping in result)
            {
                Console.WriteLine($"{mapping.Item1 + 1} <==> {mapping.Item2 + 1}");
            }
            Console.WriteLine();
        }

        private static void RunTimeTest()
        {
            var rand = new Random();
            StringBuilder stringBuilder = new StringBuilder();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<(int, int)> result;
            int vertCount1, vertCount2, vertCount3, vertCount4, vertCount5, vertCount6;
            int edgeCount1, edgeCount2, edgeCount3, edgeCount4, edgeCount5, edgeCount6;
            long time1, time2, time3, time4, time5, time6;
            string s;

            stringBuilder.Append("Graph 1 vertice count \t Graph 2 vertice count \t approximate density \t");
            stringBuilder.Append("exact v vertCount \t exact v edgeCount \t Exact v time \t");
            stringBuilder.Append("exact v+e vertCount \t exact v + e edgecount \t Exact v + e time \t");
            stringBuilder.Append("apprClique v vertCount \t apprClique v edgeCount \t apprClique v time \t");
            stringBuilder.Append("apprClique v + e vertCount \t apprClique v + e edgeCount \t apprClique v + e time \t");
            stringBuilder.Append("appr v vertCount \t appr v edgeCount \t appr v time \t");
            stringBuilder.Append("appr v + e vertCount \t appr v + e edgeCount \t appr v + e time \t");
            stringBuilder.AppendLine();

            var density = 0.5;
            for (int verticeCount = 12; verticeCount < 32; verticeCount++)
            {
                time1 = time2 = time3 = time4 = time5 = time6 = vertCount1 = vertCount2 = vertCount3 = vertCount4 = vertCount5 = vertCount6 = edgeCount1 = edgeCount2 = edgeCount3 = edgeCount4 = edgeCount5 = edgeCount6 = 0;
                s = verticeCount + "\t" + verticeCount + '\t' + density + '\t';
                stringBuilder.Append(s);

                for (int iz = 0; iz < 15; iz++)
                {
                    var g1 = Generate(verticeCount, density);
                    var g2 = Generate(verticeCount, density);

                    if (verticeCount < 20)
                    {
                        sw.Restart();
                        result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false).Solve()[0];
                        vertCount1 += result.Count;
                        time1 += sw.ElapsedMilliseconds;
                        edgeCount1 += Helpers.GetEdgeCount(result, g1);

                        sw.Restart();
                        result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false).Solve()[0];
                        time2 += sw.ElapsedMilliseconds;
                        edgeCount2 += Helpers.GetEdgeCount(result, g1);
                        vertCount2 += result.Count;
                    }

                    sw.Restart();
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: false);
                    time3 += sw.ElapsedMilliseconds;
                    edgeCount3 += Helpers.GetEdgeCount(result, g1);
                    vertCount3 += result.Count;

                    sw.Restart();
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: true);
                    time4 += sw.ElapsedMilliseconds;
                    edgeCount4 += Helpers.GetEdgeCount(result, g1);
                    vertCount4 += result.Count;

                    sw.Restart();
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false, approximation: true, stepSize: 2).Solve()[0];
                    time5 += sw.ElapsedMilliseconds;
                    edgeCount5 += Helpers.GetEdgeCount(result, g1);
                    vertCount5 += result.Count;

                    sw.Restart();
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false, approximation: true, stepSize: 2).Solve()[0];
                    time6 += sw.ElapsedMilliseconds;
                    edgeCount6 += Helpers.GetEdgeCount(result, g1);
                    vertCount6 += result.Count;
                }

                time1 /= 15; time2 /= 15; time3 /= 15; time4 /= 15; time5 /= 15; time6 /= 15; vertCount1 /= 15; vertCount2 /= 15; vertCount3 /= 15; vertCount4 /= 15; vertCount5 /= 15; vertCount6 /= 15; edgeCount1 /= 15; edgeCount2 /= 15; edgeCount3 /= 15; edgeCount4 /= 15; edgeCount5 /= 15; edgeCount6 /= 15;
                stringBuilder.Append(vertCount1 + "\t" + edgeCount1 + '\t' + time1 + '\t');
                stringBuilder.Append(vertCount2 + "\t" + edgeCount2 + '\t' + time2 + '\t');
                stringBuilder.Append(vertCount3 + "\t" + edgeCount3 + '\t' + time3 + '\t');
                stringBuilder.Append(vertCount4 + "\t" + edgeCount4 + '\t' + time4 + '\t');
                stringBuilder.Append(vertCount5 + "\t" + edgeCount5 + '\t' + time5 + '\t');
                stringBuilder.Append(vertCount6 + "\t" + edgeCount6 + '\t' + time6 + '\t');
                stringBuilder.AppendLine();
            }

            File.WriteAllText("resultTime.csv", stringBuilder.ToString());
        }

        private static void RunTests()
        {
            var rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                var verticeCount = rand.Next(1, 25);
                var density = rand.NextDouble() / 2;
                var g1 = Generate(verticeCount, density);
                var verticeCount2 = rand.Next(1, 25);
                var density2 = rand.NextDouble() / 2;
                var g2 = Generate(verticeCount2, density2);

                var result1 = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false).Solve()[0];
                if (!TestResult(result1, g1, g2))
                    throw new Exception();
                var result2 = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false).Solve()[0];
                if (!TestResult(result2, g1, g2))
                    throw new Exception();
                var result5 = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: false);
                if (!TestResult(result5, g1, g2))
                    throw new Exception();
                var result6 = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: true);
                if (!TestResult(result6, g1, g2))
                    throw new Exception();
                var result7 = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
                if (!TestResult(result7, g1, g2))
                    throw new Exception();
                var result8 = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
                if (!TestResult(result8, g1, g2))
                    throw new Exception();
            }
        }

        private static bool TestResult(List<(int, int)> mapping, bool[,] g1, bool[,] g2)
        {
            foreach (var (v, w) in mapping)
            {
                foreach (var (v1, w1) in mapping)
                {
                    var r1 = g1[v, v1];
                    var r2 = g2[w, w1];
                    if (r1 != r2)
                        return false;
                }
            }

            Queue<int> q = new Queue<int>();
            q.Enqueue(mapping[0].Item1);
            bool[] connected = new bool[mapping.Count];
            connected[0] = true;
            while (q.Count != 0)
            {
                var v = q.Dequeue();
                for (int i = 0; i < mapping.Count; i++)
                {
                    var w = mapping[i].Item1;
                    if (!connected[i] && g1[v, w])
                    {
                        connected[i] = true;
                        q.Enqueue(w);
                    }
                }
            }

            if (connected.Any(x => !x))
                return false;


            return true;
        }
    }
}
