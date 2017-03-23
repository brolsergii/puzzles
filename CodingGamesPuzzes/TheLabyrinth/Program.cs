using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Player
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
    static List<Tuple<int, int>> ExplorationPath;
    static int AlarmCooldown = int.MaxValue;
    #endregion

    /// <summary>
    /// Check if the bot can access the case
    /// </summary>
    static bool IsCaseAccessible(int i, int j) => (MAZE[i, j] == '.' || MAZE[i, j] == 'T' || MAZE[i, j] == 'C');

    /// <summary>
    /// Gets all possible points to explore
    /// </summary>
    static IEnumerable<Tuple<int, int>> GetPossiblePoints()
    {
        for (int i = 0; i < MAZE.GetLength(0); i++)
        {
            for (int j = 0; j < MAZE.GetLength(1); j++)
            {
                if (MAZE[i, j] == '.' || MAZE[i, j] == 'T')
                {
                    if (i > 0 && MAZE[i - 1, j] == '?')
                        yield return new Tuple<int, int>(i, j);
                    if (i < MAZE.GetLength(0) - 1 && MAZE[i + 1, j] == '?')
                        yield return new Tuple<int, int>(i, j);
                    if (j > 0 && MAZE[i, j - 1] == '?')
                        yield return new Tuple<int, int>(i, j);
                    if (j < MAZE.GetLength(1) - 1 && MAZE[i, j + 1] == '?')
                        yield return new Tuple<int, int>(i, j);
                }
            }
        }
    }

    /// <summary>
    /// Gets a direction string between two points
    /// </summary>
    static string GetDirection(Tuple<int, int> from, Tuple<int, int> to)
    {
        if (from.Item1 == to.Item1)
        {
            if (from.Item2 < to.Item2)
                return "RIGHT";
            else
                return "LEFT";
        }
        else
        {
            if (from.Item1 > to.Item1)
                return "UP";
            else
                return "DOWN";
        }
    }

    /// <summary>
    /// Score a distance between 2 points for A* algo
    /// </summary>
    static double DistanceScoreSqr(Tuple<int, int> p1, Tuple<int, int> p2) => Math.Sqrt((p1.Item1 - p2.Item1) * (p1.Item1 - p2.Item1) + (p1.Item2 - p2.Item2) * (p1.Item2 - p2.Item2));
    static double DistanceScoreLin(Tuple<int, int> p1, Tuple<int, int> p2) => Math.Abs(p1.Item1 - p2.Item1) + Math.Abs(p1.Item2 - p2.Item2);
    static double DistanceScore(Tuple<int, int> p1, Tuple<int, int> p2) => DistanceScoreSqr(p1, p2); // Only for exploration

    /// <summary>
    /// Get add accessible neibors for a point
    /// </summary>
    static IEnumerable<Tuple<int, int>> GetAccessibleNeighbors(Tuple<int, int> p)
    {
        int i = p.Item1;
        int j = p.Item2;
        if (IsCaseAccessible(i, j))
        {
            if (i > 0 && IsCaseAccessible(i - 1, j))
                yield return new Tuple<int, int>(i - 1, j);
            if (i < MAZE.GetLength(0) - 1 && IsCaseAccessible(i + 1, j))
                yield return new Tuple<int, int>(i + 1, j);
            if (j > 0 && IsCaseAccessible(i, j - 1))
                yield return new Tuple<int, int>(i, j - 1);
            if (j < MAZE.GetLength(1) - 1 && IsCaseAccessible(i, j + 1))
                yield return new Tuple<int, int>(i, j + 1);
        }
    }

    /// <summary>
    /// Implements A* to find the shortest way between two points
    /// </summary>
    static List<Tuple<int, int>> GetShortestWay(Tuple<int, int> from, Tuple<int, int> to, Func<Tuple<int, int>, Tuple<int, int>, double> distCalc, bool accurate = false)
    {
        bool pathFound = false;
        var pathNodes = new HashSet<Tuple<int, int>>();
        var allNodes = new HashSet<Tuple<int, int>>();
        var cameFrom = new Dictionary<Tuple<int, int>, Tuple<int, int>>();
        var nodeScore = new Dictionary<Tuple<int, int>, int>();
        pathNodes.Add(from);
        allNodes.Add(from);
        nodeScore[from] = 0;
        Tuple<int, int> currentPoint = from;
        while (!pathFound || pathNodes.Count > 0)
        {
            //Deb($"  [A*]: c p {currentPoint}. Dist {DistanceScore(to, currentPoint)}");
            pathNodes.Remove(currentPoint);
            var closestNodes = GetAccessibleNeighbors(currentPoint);
            foreach (var node in closestNodes)
            {
                if (!cameFrom.ContainsKey(node) || nodeScore[currentPoint] < nodeScore[cameFrom[node]])
                {
                    cameFrom[node] = currentPoint;
                    nodeScore[node] = nodeScore[currentPoint] + 1;
                }
                //Deb($"  [A*]: adding neighbor {node}. Dist {distCalc(to, node)}. Depth {nodeScore[node]}");
                if (!allNodes.Contains(node))
                {
                    allNodes.Add(node);
                    pathNodes.Add(node);
                }
            }
            if (pathNodes.Count == 0) // No path
                break;
            currentPoint = pathNodes.OrderBy(x => distCalc(x, to) + nodeScore[x]).First();

            if (currentPoint.Item1 == to.Item1 && currentPoint.Item2 == to.Item2)
            {
                Deb($"  [A*]: Got the target. Node score {nodeScore[currentPoint]}");
                if (!accurate || nodeScore[currentPoint] <= AlarmCooldown)
                {
                    pathFound = true;
                    break;
                }
            }
        }
        if (!pathFound)
            return null;
        else
        {
            var path = new List<Tuple<int, int>>();
            var currNode = to;
            while (currNode != from)
            {
                path.Add(currNode);
                currNode = cameFrom[currNode];
            }
            path.Reverse();
            return path;
        }
    }

    static void Explore(int currentRow, int currentCol)
    {
        if (ExplorationPath == null || ExplorationPath.Count == 0)
        {
            var exploreOptions = GetPossiblePoints().OrderBy(x => DistanceScore(x, new Tuple<int, int>(currentRow, currentCol)));
            Deb($"Exploring from ({currentRow},{currentCol})");
            DebList(exploreOptions);
            foreach (var option in exploreOptions)
            {
                var path = GetShortestWay(new Tuple<int, int>(currentRow, currentCol), option, DistanceScore);
                if (path != null)
                {
                    var nextPoint = path[0];
                    path.Remove(nextPoint);
                    ExplorationPath = path;
                    Deb($"Go explore to {nextPoint}");
                    Console.WriteLine(GetDirection(new Tuple<int, int>(currentRow, currentCol), nextPoint));
                    break;
                }
            }
        }
        else
        {
            var nextPoint = ExplorationPath[0];
            ExplorationPath.Remove(nextPoint);
            Deb($"Go explore to {nextPoint}");
            Console.WriteLine(GetDirection(new Tuple<int, int>(currentRow, currentCol), nextPoint));
        }
    }

    public static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int rowsCount = int.Parse(inputs[0]); // number of rows.
        int colsCount = int.Parse(inputs[1]); // number of columns.
        AlarmCooldown = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.
        Deb($"The maze has {rowsCount} rows and {colsCount} cols. Alert is {AlarmCooldown}");
        MAZE = new char[rowsCount, colsCount];
        while (true) // game loop
        {
            inputs = Console.ReadLine().Split(' ');
            int currentRow = int.Parse(inputs[0]); // row where Kirk is located.
            int currentCol = int.Parse(inputs[1]); // column where Kirk is located.
            for (int i = 0; i < rowsCount; i++)
            {
                string mazeRow = Console.ReadLine(); // C of the characters in '#.TC?' (one line of the ASCII maze).
                for (int j = 0; j < mazeRow.Length; j++)
                {
                    MAZE[i, j] = mazeRow[j];
                    if (mazeRow[j] == 'T')
                        StartPoint = new Tuple<int, int>(i, j);
                    if (mazeRow[j] == 'C')
                    {
                        CenterPoint = new Tuple<int, int>(i, j);
                        if (currentRow == i && currentCol == j)
                        {
                            Deb($"The command center is found");
                            FoundCommandCenter = true; // The command center is found
                        }
                    }
                }
            }
            if (!FoundCommandCenter) // Explore
            {
                if (CenterPoint != null) // Go strait to command center
                {
                    var path = GetShortestWay(new Tuple<int, int>(currentRow, currentCol), CenterPoint, DistanceScore);
                    var pathCenterToStart = GetShortestWay(CenterPoint, StartPoint, DistanceScore, true);
                    if (path == null || pathCenterToStart == null)
                    {
                        Deb("Can't reach center");
                        Explore(currentRow, currentCol);
                    }
                    else
                    {
                        var nextPoint = path[0];
                        Deb($"Go to center to {nextPoint}");
                        Console.WriteLine(GetDirection(new Tuple<int, int>(currentRow, currentCol), nextPoint));
                    }
                }
                else
                {
                    Explore(currentRow, currentCol);
                }
            }
            else // Get back
            {
                var path1 = GetShortestWay(new Tuple<int, int>(currentRow, currentCol), StartPoint, DistanceScoreSqr, true);
                var path2 = GetShortestWay(new Tuple<int, int>(currentRow, currentCol), StartPoint, DistanceScoreLin, true);
                var path = (path1.Count > path2.Count) ? path2 : path1;
                var nextPoint = path[0];
                Deb($"Go back to {nextPoint}. Turns left {path.Count}. Alert in {AlarmCooldown}");
                Console.WriteLine(GetDirection(new Tuple<int, int>(currentRow, currentCol), nextPoint));
                foreach (var point in path)
                    MAZE[point.Item1, point.Item2] = '*';
            }
            DebMaze(MAZE); // Output the maze in debug mode
        }
    }
}