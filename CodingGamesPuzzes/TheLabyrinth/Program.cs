using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    #region Auxilary methods
    static void TryCatch(Action a) { try { a(); } catch (Exception) { } }
    static int TryGetInt(Func<int> a) { try { return a(); } catch (Exception) { return 0; } }
    static void Deb(object o) => Console.Error.WriteLine(o);
    static void DebList(IEnumerable<object> e) => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x} {y}"));
    static void DebObjList(IEnumerable<object> e) => TryCatch(() => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x}\n{y}")));
    static void DebDict(Dictionary<int, int> d)
    {
        foreach (var pair in d)
            Console.Error.WriteLine($"[{pair.Key}]:{pair.Value}");
    }
    static void DebMaze(char[,] maze)
    {
        for (int i = 0; i < maze.GetLength(0); i++)
        {
            for (int j = 0; j < maze.GetLength(1); j++)
            {
                Console.Error.Write(maze[i, j]);
            }
            Console.Error.WriteLine();
        }
    }
    #endregion

    #region The game environement
    static char[,] MAZE;
    static bool FoundCommandCenter = false;
    static Tuple<int, int> StartPoint = new Tuple<int, int>(0, 0);
    static Tuple<int, int> CenterPoint;
    #endregion


    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int rowsCount = int.Parse(inputs[0]); // number of rows.
        int colsCount = int.Parse(inputs[1]); // number of columns.
        int alarmCoolDown = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.
        Deb($"The maze has {rowsCount} rows and {colsCount} cols");
        MAZE = new char[rowsCount, colsCount];
        while (true) // game loop
        {
            inputs = Console.ReadLine().Split(' ');
            int currentRow = int.Parse(inputs[0]); // row where Kirk is located.
            int currentCol = int.Parse(inputs[1]); // column where Kirk is located.
            for (int i = 0; i < rowsCount; i++)
            {
                string mazeRow = Console.ReadLine(); // C of the characters in '#.TC?' (i.e. one line of the ASCII maze).
                for (int j = 0; j < mazeRow.Length; j++)
                {
                    MAZE[i, j] = mazeRow[j];
                    if (mazeRow[j] == 'T')
                        StartPoint = new Tuple<int, int>(i, j);
                    if (mazeRow[j] == 'C')
                        CenterPoint = new Tuple<int, int>(i, j);
                }
            }
            DebMaze(MAZE);
            if (!FoundCommandCenter)
            {
                // Explore
            }
            else
            {
                // GetBack
            }

            Console.WriteLine("RIGHT"); // Kirk's next move (UP DOWN LEFT or RIGHT).
        }
    }
}