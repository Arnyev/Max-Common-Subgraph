using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Taio;
using System.IO;
using System.Linq;

namespace tmp_app
{
    public partial class Form1 : Form
    {
        private int algorithmNumber;
        public bool[,] arrayGraphA;
        public bool[,] arrayGraphB;

        public List<(int, int)> result;
        List<List<(int, int)>> results;

        public List<(int, int)> isomorphism;

        public Form1()
        {
            algorithmNumber = 1;
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var g1 = arrayGraphA;
            var g2 = arrayGraphB;
            if (g1 == null || g2 == null)
            {
                LogError("Graph is null");
                return;
            }
            foreach (var node in viewerA.Graph.Nodes)
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;
            foreach (var node in viewerB.Graph.Nodes)
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.White;

            switch (algorithmNumber)
            {
                case 1:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false).Solve()[0];
                    break;
                case 2:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false).Solve()[0];
                    break;
                case 5:
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: false);
                    break;
                case 6:
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: true);
                    break;
                case 7:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false, approximation: true, stepSize: 2).Solve()[0];
                    break;
                case 8:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false, approximation: true, stepSize: 2).Solve()[0];
                    break;
                default:
                    LogError("Wrong algorithm number!");
                    return;
            }


            int resultSize = results?[0].Count() ?? result.Count();
            var edgeCount = Helpers.GetEdgeCount(result, g1);
            var density = edgeCount / (resultSize * (resultSize - 1.0) / 2);
            LogInfo("Done");
            LogInfo("Size of graph 1:");
            LogInfo("  " + this.arrayGraphA.GetLength(0).ToString());
            LogInfo("Size of graph 2:");
            LogInfo("  " + this.arrayGraphB.GetLength(0).ToString());
            LogInfo($"Mapping size (V):");
            LogInfo("  " + resultSize.ToString());
            LogInfo($"Mapping size (V + E): {resultSize + edgeCount}");
            LogInfo($"Mapping density: {density:N2}");


            foreach (var pair in this.result)
            {
                var color = GetRandomColor();
                var node = viewerA.Graph.FindNode(pair.Item1.ToString());
                node.Attr.FillColor = color;

                var node2 = viewerB.Graph.FindNode(pair.Item2.ToString());
                node2.Attr.FillColor = color;
            }
            viewerA.Refresh();
            viewerB.Refresh();
        }

        double lastHue = 0;
        private Microsoft.Msagl.Drawing.Color GetRandomColor()
        {
            lastHue += 0.618033988749895;
            if (lastHue > 1) { lastHue -= 1; }
            var hueAsDegree = lastHue * 360;
            var color = ColorFromHSV(hueAsDegree, 0.5, 0.95);
            return new Microsoft.Msagl.Drawing.Color(color.R, color.G, color.B);
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        // Load the first Graph
        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialogAndInitializeGraph(ref arrayGraphA, out string filename))
            {
                this.textBox1.Text = filename;
                Microsoft.Msagl.Drawing.Graph graphA;
                GuiHelpers.CreateGraphFromArray(out graphA, arrayGraphA, "GraphA");
                viewerA.Graph = graphA;

                button1.BackColor = Color.LightGreen;
            }
            else button1.BackColor = Color.Red;

        }

        // Load the second Graph
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialogAndInitializeGraph(ref arrayGraphB, out string filename))
            {
                this.textBox2.Text = filename;
                Microsoft.Msagl.Drawing.Graph graphB;
                GuiHelpers.CreateGraphFromArray(out graphB, arrayGraphB, "GraphB");
                viewerB.Graph = graphB;

                button2.BackColor = Color.LightGreen;
            }
            else button2.BackColor = Color.Red;
        }

        private bool openFileDialogAndInitializeGraph(ref bool[,] graph, out string filename)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                filename = "";
                return false;
            }
            if (!openFileDialog1.CheckFileExists)
            {
                filename = "";
                LogError("Such file doesn't exit");
                return false;
            }
            if (!openFileDialog1.FileName.EndsWith(".csv"))
            {
                filename = "";
                LogError("Wrong file extension");
                return false;
            }

            graph = GuiHelpers.DeserializeG(filename = openFileDialog1.FileName);
            return true;
        }

        private void CheckedChangedGeneral(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked) algorithmNumber = 1;
            else if (this.radioButton2.Checked) algorithmNumber = 2;
            else if (this.radioButton3.Checked) algorithmNumber = 5;
            else if (this.radioButton4.Checked) algorithmNumber = 6;
            else if (this.radioButton5.Checked) algorithmNumber = 7;
            else if (this.radioButton6.Checked) algorithmNumber = 8;
        }

        private void LogError(string text)
        {
            outputInfo.SelectionStart = outputInfo.TextLength;
            outputInfo.SelectionLength = 0;

            this.outputInfo.SelectionColor = Color.Red;
            outputInfo.AppendText(text + Environment.NewLine);
            outputInfo.SelectionColor = outputInfo.ForeColor;
        }

        private void LogInfo(string text)
        {
            outputInfo.SelectionStart = outputInfo.TextLength;
            outputInfo.SelectionLength = 0;

            this.outputInfo.SelectionColor = Color.Black;
            outputInfo.AppendText(text + Environment.NewLine);
            outputInfo.SelectionColor = outputInfo.ForeColor;
        }

        // Export to csv handler
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                var a = Path.GetFileNameWithoutExtension(this.textBox1.Text);
                var b = Path.GetFileNameWithoutExtension(this.textBox2.Text);

                if (saveFileDialog1.ShowDialog() != DialogResult.OK) return;


                if (this.result != null)
                {
                    using (var file = new StreamWriter(saveFileDialog1.FileName))
                    {
                        file.WriteLine(string.Join(",", result.Select(pair => pair.Item1)));
                        file.WriteLine(string.Join(",", result.Select(pair => pair.Item2)));
                        file.WriteLine();
                    }
                }
                else if (this.results != null)
                {
                    using (var file = new StreamWriter(saveFileDialog1.FileName))
                    {
                        foreach (var r in results)
                        {
                            file.WriteLine(string.Join(",", r.Select(pair => pair.Item1)));
                            file.WriteLine(string.Join(",", r.Select(pair => pair.Item2)));
                            file.WriteLine();
                        }
                    }
                }
                else
                {
                    LogError("Run algorithm first");
                    return;
                }
                LogInfo("Exported to csv");
            }
            catch (Exception ee)
            {
                LogError("Cannot convert to csv");
            }

        }

        // Abort handler
        private void button5_Click(object sender, EventArgs e)
        {
            LogInfo("Computation aborted");
        }
    }
}
