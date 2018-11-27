using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tmp_app
{
    public static class GuiHelpers
    {
        public static void CreateGraphFromArray(out Microsoft.Msagl.Drawing.Graph graph, bool[,] array, string graphLabel)
        {
            graph = new Microsoft.Msagl.Drawing.Graph(graphLabel);
            graph.Directed = false;
            graph.Attr.AddStyle(Microsoft.Msagl.Drawing.Style.None);

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (array[i, j])
                    {
                        var e = graph.AddEdge(i.ToString(), j.ToString());
                        e.Attr.ArrowheadAtTarget = Microsoft.Msagl.Drawing.ArrowStyle.None;
                    }
                }
            }
            
        }

        public static bool[,] DeserializeG(string csvPath, char separator = ',')
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
                    matrix[i, j] = row[j].Trim() == "1";
                }
            }

            return matrix;
        }
    }
}
