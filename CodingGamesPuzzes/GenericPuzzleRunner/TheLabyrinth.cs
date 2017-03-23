using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericPuzzleRunner
{
    public class TheLabyrinth
    {
        static Dictionary<int, int> Rows = new Dictionary<int, int>
        {
            {1, 15},
            {2, 15},
            {7, 15},
            {14, 15}
        };

        static Dictionary<int, int> Cols = new Dictionary<int, int>
        {
            {1, 30},
            {2, 30},
            {7, 30},
            {14, 30}
        };

        static Dictionary<int, int> Alarm = new Dictionary<int, int>
        {
            {1, 7},
            {2, 21},
            {7, 70},
            {14, 33}
        };

        static Dictionary<int, Tuple<int, int>> Kirk = new Dictionary<int, Tuple<int, int>>
        {
            {1, new Tuple<int,int>(6,5) },
            {2, new Tuple<int,int>(2,25) },
            {7, new Tuple<int,int>(3,6) },
            {14, new Tuple<int,int>(13,18) }
        };

        static Dictionary<int, char[][]> Maze = new Dictionary<int, char[][]>
        {
            {1, new char[][]
                    {
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "#####T......C#################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                         "##############################".ToCharArray(),
                    }
            },
            {2, new char[][]
                    {
                        "#####################.....####".ToCharArray(),
                        "#################.############".ToCharArray(),
                        "#################.#......T####".ToCharArray(),
                        "############...#..#.#######.##".ToCharArray(),
                        "###########.#######...#####.##".ToCharArray(),
                        "###########.#C..#####.########".ToCharArray(),
                        "###########.###.......########".ToCharArray(),
                        "###########.#.########..######".ToCharArray(),
                        "#############.##....##########".ToCharArray(),
                        "##############################".ToCharArray(),
                        "##############################".ToCharArray(),
                        "##############################".ToCharArray(),
                        "##############################".ToCharArray(),
                        "##############################".ToCharArray(),
                        "##############################".ToCharArray(),
                    }
            },
            {7, new char[][]
                 {
                    "##############################".ToCharArray(),
                    "#............................#".ToCharArray(),
                    "#.#######################.#..#".ToCharArray(),
                    "#.....T.................#.#..#".ToCharArray(),
                    "#.....#.................#.#..#".ToCharArray(),
                    "#.#######################.#..#".ToCharArray(),
                    "#.....##......##......#....###".ToCharArray(),
                    "#...####..##..##..##..#..#...#".ToCharArray(),
                    "#.........##......##.....#...#".ToCharArray(),
                    "###########################.##".ToCharArray(),
                    "#......#......#..............#".ToCharArray(),
                    "#...C..#.....................#".ToCharArray(),
                    "#...#..####################..#".ToCharArray(),
                    "#............................#".ToCharArray(),
                    "##############################".ToCharArray(),
                 }
            },
            {14, new char[][]
                 {
                    "##############################".ToCharArray(),
                    "#.....###.##.....##.#....##.##".ToCharArray(),
                    "#.###.......####......##....##".ToCharArray(),
                    "#.###.##.##.####.##.#.##.##..#".ToCharArray(),
                    "#.......C##........#.....##.##".ToCharArray(),
                    "#####.###......###...###.....#".ToCharArray(),
                    "##..........##.....#.###.#.###".ToCharArray(),
                    "##........##.###.###.........#".ToCharArray(),
                    "#..##.###............#.###.###".ToCharArray(),
                    "##.##........#.###.###.....###".ToCharArray(),
                    "##...#.#.##.##.##...########.#".ToCharArray(),
                    "##......#.#..###.............#".ToCharArray(),
                    "##...#######.###.######.####.#".ToCharArray(),
                    "##......##.......#T........###".ToCharArray(),
                    "##############################".ToCharArray(),
                 }
            }
        };

        #region I/O helpers
        static Process proc;
        static void OutWrite(object o) => proc.StandardInput.Write(o);
        static void OutWriteLine(object o) => proc.StandardInput.WriteLine(o);
        static string ReadLine() => proc.StandardOutput.ReadLine();
        #endregion

        static int MaxFuel = 1200;
        static bool CheckTerminal = false;
        static bool GameEnd = false;
        static bool[,] MazeMask;

        public static void UpdateMazeMask(int m, int n)
        {
            for (int i = m - 2; i <= m + 2; i++)
                for (int j = n - 2; j <= n + 2; j++)
                {
                    if (i >= 0 && i < MazeMask.GetLength(0) && j >= 0 && j < MazeMask.GetLength(1))
                        MazeMask[i, j] = true;
                }
        }

        public static void ShowMazeMapRow(int testId, int row)
        {
            for (int i = 0; i < Maze[testId][row].Length; i++)
            {
                if (MazeMask[row, i])
                    OutWrite(Maze[testId][row][i]);
                else
                    OutWrite('?');
            }
            OutWriteLine("");
        }

        public static void Run(ref Process clientProc, int testId)
        {
            proc = clientProc;
            OutWriteLine($"{Rows[testId]} {Cols[testId]} {Alarm[testId]}");
            MazeMask = new bool[Rows[testId], Cols[testId]];
            while (MaxFuel > 0 && !GameEnd)
            {
                if (CheckTerminal)
                    Alarm[testId]--;
                OutWriteLine($"{Kirk[testId].Item1} {Kirk[testId].Item2}");
                UpdateMazeMask(Kirk[testId].Item1, Kirk[testId].Item2);
                for (int i = 0; i < Rows[testId]; i++)
                    ShowMazeMapRow(testId, i);

                string action = ReadLine();
                if (string.IsNullOrEmpty(action))
                    throw new Exception("no answer");
                if (action == "UP")
                    Kirk[testId] = new Tuple<int, int>(Kirk[testId].Item1 - 1, Kirk[testId].Item2);
                else if (action == "DOWN")
                    Kirk[testId] = new Tuple<int, int>(Kirk[testId].Item1 + 1, Kirk[testId].Item2);
                else if (action == "LEFT")
                    Kirk[testId] = new Tuple<int, int>(Kirk[testId].Item1, Kirk[testId].Item2 - 1);
                else if (action == "RIGHT")
                    Kirk[testId] = new Tuple<int, int>(Kirk[testId].Item1, Kirk[testId].Item2 + 1);
                else
                    throw new Exception("action is not supported");

                if (Maze[testId][Kirk[testId].Item1][Kirk[testId].Item2] == 'C')
                    CheckTerminal = true;
                if (CheckTerminal && Maze[testId][Kirk[testId].Item1][Kirk[testId].Item2] == 'T')
                    GameEnd = true;
                if (GameEnd)
                    break;
                if (Alarm[testId] == 0)
                    throw new Exception("BOOM!");
                MaxFuel--;
            }
        }
    }
}
