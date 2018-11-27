﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Taio;

namespace tmp_app
{
    public partial class Form1 : Form
    {
        private int algorithmNumber;
        public bool[,] arrayGraphA;
        public bool[,] arrayGraphB;
        public List<(int, int)> result;
        List<List<(int, int)>> results;

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

            List<List<(int, int)>> results = null;
            List<(int, int)> result = null;

            switch (algorithmNumber)
            {
                case 1:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false).Solve()[0];
                    break;
                case 2:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false).Solve()[0];
                    break;
                case 3:
                    results = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: true).Solve();
                    break;
                case 4:
                    results = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: true).Solve();
                    break;
                case 5:
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: false);
                    break;
                case 6:
                    result = new MaxInducedSubgraphCliqueApproximation().FindCommonSubgraph(g1, g2, edgeVersion: true);
                    break;
                case 7:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: false, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
                    break;
                case 8:
                    result = new McSplitAlgorithmSolver(g1, g2, edgeVersion: true, returnAll: false, approximation: true, stepSize: 4).Solve()[0];
                    break;
                default:
                    LogError("Wrong algorithm number!");
                    return;
            }

            this.result = result;
            LogInfo("Done");

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

        Random rand = new Random();
        private Microsoft.Msagl.Drawing.Color GetRandomColor()
        {
            System.Array colorsArray = Enum.GetValues(typeof(KnownColor));
            KnownColor[] allColors = new KnownColor[colorsArray.Length];
            Array.Copy(colorsArray, allColors, colorsArray.Length);

            Color sysColor = Color.FromKnownColor(allColors[rand.Next() % allColors.Length]);
            return new Microsoft.Msagl.Drawing.Color(sysColor.R, sysColor.G, sysColor.B);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialogAndInitializeGraph(ref arrayGraphA))
            {
                Microsoft.Msagl.Drawing.Graph graphA;
                GuiHelpers.CreateGraphFromArray(out graphA, arrayGraphA, "GraphA");
                viewerA.Graph = graphA;
                
                button1.BackColor = Color.LightGreen;
            }
            else button1.BackColor = Color.Red;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialogAndInitializeGraph(ref arrayGraphB))
            {
                Microsoft.Msagl.Drawing.Graph graphB;
                GuiHelpers.CreateGraphFromArray(out graphB, arrayGraphB, "GraphB");
                viewerB.Graph = graphB;

                button2.BackColor = Color.LightGreen;
            }
            else button2.BackColor = Color.Red;
        }

        private bool openFileDialogAndInitializeGraph(ref bool[,] graph)
        {
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }
            if (!openFileDialog1.CheckFileExists)
            {
                LogError("Such file doesn't exit");
                return false;
            }
            if (!openFileDialog1.FileName.EndsWith(".csv"))
            {
                LogError("Wrong file extension");
                return false;
            }

            graph = GuiHelpers.DeserializeG(openFileDialog1.FileName);
            return true;
        }

        private void CheckedChangedGeneral(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked) algorithmNumber = 1;
            else if (this.radioButton1.Checked) algorithmNumber = 2;
            else if (this.radioButton1.Checked) algorithmNumber = 5;
            else if (this.radioButton1.Checked) algorithmNumber = 6;
            else if (this.radioButton1.Checked) algorithmNumber = 7;
            else if (this.radioButton1.Checked) algorithmNumber = 8;
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

        }

        // Abort handler
        private void button5_Click(object sender, EventArgs e)
        {
            LogInfo("Computation aborted");
        }
    }
}