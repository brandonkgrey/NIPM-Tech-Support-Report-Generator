using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace NIPM_Tech_Support_Report_Generator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainWindow());
            }
            catch (InvalidNIPMException ex)
            {
                MessageBox.Show("Compatible version of NIPM is not installed. Cannot generate a report.", "Error", MessageBoxButtons.OK);
            }
        }
    }
}
