using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericPuzzleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var clientProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"..\..\..\TheLabyrinth\bin\Debug\TheLabyrinth.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = false,
                }
            };
            clientProc.Start();
            TheLabyrinth.Run(ref clientProc, 2);
            clientProc.Kill();
            Console.ReadKey();
        }
    }
}
