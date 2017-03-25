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
            RunTeadsChellenge(5);
            Console.ReadKey();
        }

        static void RunTeadsChellenge(int testId) => GenericRun(@"..\..\..\TeadsChallenge\bin\Debug\TeadsChallenge.exe", new TeadsChallenge(), testId);

        static void RunTheLabyrinth(int testId) => GenericRun(@"..\..\..\TheLabyrinth\bin\Debug\TheLabyrinth.exe", new TheLabyrinth(), testId);

        static void GenericRun(string exePath, Runner runner, int testId)
        {
            var clientProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = false,
                }
            };
            clientProc.Start();
            runner.Run(ref clientProc, testId);
            clientProc.Kill();
        }
    }
}
