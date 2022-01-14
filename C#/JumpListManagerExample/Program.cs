using System;
using System.Diagnostics;
using JumpListHelpers;

namespace JumpListManagerExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            ProgramManager.Run(typeof(MainForm), "JumpListManager Example");
        }
    }
}
