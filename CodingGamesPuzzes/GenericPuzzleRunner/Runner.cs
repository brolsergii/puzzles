using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericPuzzleRunner
{
    public abstract class Runner
    {
        #region I/O helpers
        public static Process proc;
        public static void OutWrite(object o) => proc.StandardInput.Write(o);
        public static void OutWriteLine(object o) => proc.StandardInput.WriteLine(o);
        public static string ReadLine() => proc.StandardOutput.ReadLine();
        #endregion

        public abstract void Run(ref Process clientProc, int testId);
    }
}
