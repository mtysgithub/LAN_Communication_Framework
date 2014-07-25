using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Mty.LCF.Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length < 2)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Form mainForm = new Server();
                Application.Run(mainForm);
            }
            else
            {
                MessageBox.Show("程序实例已经在运行", "提示");
            }
        }
    }
}
