using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace tmp_app
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            var form = new Form1();
            
            form.ShowDialog();
        }

    }
}
