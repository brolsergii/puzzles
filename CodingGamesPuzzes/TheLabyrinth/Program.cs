using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

#region Auxilary structs
public struct Point : IComparable, IEquatable<Point>
{
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x;
    public int y;

    public int CompareTo(object obj) => (obj is Point && Equals((Point)obj)) ? 0 : 1;
    public bool Equals(Point other) => (other.x == x && other.y == y);
    public override string ToString() => $"({x},{y})";
    public override int GetHashCode() => (x << 6) + y;
}
#endregion

public class Player
{
    #region Auxilary methods
    static void TryCatch(Action a) { try { a(); } catch (Exception) { } }
    static int TryGetInt(Func<int> a) { try { return a(); } catch (Exception) { return 0; } }
    static void Deb(object o) => Console.Error.WriteLine(o);
    static void DebList(IEnumerable<Point> e) => Console.Error.WriteLine(e.Select(x => x.ToString()).Aggregate((x, y) => $"{x} {y}"));
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
    static Point StartPoint = new Point { x = 0, y = 0 };
    static Point? CenterPoint = null;
    static List<Point> ExplorePath;
    static List<Point> HomePath;
    static int AlarmCooldown = int.MaxValue;
    #endregion

    /// <summary>
    /// Check if the bot can access the case
    /// </summary>
    static bool IsCaseAccessible(int i, int j) => (MAZE[i, j] == '.' || MAZE[i, j] == 'T' || MAZE[i, j] == 'C');

    /// <summary>
    /// Gets all possible points to explore
    /// </summary>
    static IEnumerable<Point> GetPossiblePoints()
    {
        for (int i = 0; i < MAZE.GetLength(0); i++)
        {
            for (int j = 0; j < MAZE.GetLength(1); j++)
            {
                if (MAZE[i, j] == '.' || MAZE[i, j] == 'T')
                {
                    if (i > 0 && MAZE[i - 1, j] == '?')
                        yield return new Point { x = i, y = j };
                    if (i < MAZE.GetLength(0) - 1 && MAZE[i + 1, j] == '?')
                        yield return new Point { x = i, y = j };
                    if (j > 0 && MAZE[i, j - 1] == '?')
                        yield return new Point { x = i, y = j };
                    if (j < MAZE.GetLength(1) - 1 && MAZE[i, j + 1] == '?')
                        yield return new Point { x = i, y = j };
                }
            }
        }
    }

    /// <summary>
    /// Gets a direction string between two points
    /// </summary>
    static string GetDirection(Point from, Point to)
    {
        if (from.x == to.x)
        {
            if (from.y < to.y)
                return "RIGHT";
            else
                return "LEFT";
        }
        else
        {
            if (from.x > to.x)
                return "UP";
            else
                return "DOWN";
        }
    }

    /// <summary>
    /// Score a distance between 2 points for A* algo
    /// </summary>
    static double DistanceScoreSqr(Point p1, Point p2) => Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y));
    static double DistanceScoreLin(Point p1, Point p2) => Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
    static double DistanceScore(Point p1, Point p2) => DistanceScoreSqr(p1, p2); // Only for exploration

    /// <summary>
    /// Get add accessible neibors for a point
    /// </summary>
    static List<Point> GetAccessibleNeighbors(Point p)
    {
        int i = p.x;
        int j = p.y;
        int mazeCount0 = MAZE.GetLength(0);
        int mazeCount1 = MAZE.GetLength(1);
        var result = new List<Point>();
        if (IsCaseAccessible(i, j))
        {
            if (i > 0 && IsCaseAccessible(i - 1, j))
                result.Add(new Point { x = i - 1, y = j });
            if (i < mazeCount0 - 1 && IsCaseAccessible(i + 1, j))
                result.Add(new Point { x = i + 1, y = j });
            if (j > 0 && IsCaseAccessible(i, j - 1))
                result.Add(new Point { x = i, y = j - 1 });
            if (j < mazeCount1 - 1 && IsCaseAccessible(i, j + 1))
                result.Add(new Point { x = i, y = j + 1 });
        }
        return result;
    }

    /// <summary>
    /// Implements A* to find the shortest way between two points
    /// </summary>
    static List<Point> GetShortestWay(Point from, Point to, Func<Point, Point, double> distCalc, bool accurate = false)
    {
        bool pathFound = false;
        var pathNodes = new HashSet<Point>();
        var allNodes = new HashSet<Point>();
        var cameFrom = new Dictionary<Point, Point>(15 * 30);
        var nodeScore = new Dictionary<Point, int>(15 * 30);
        pathNodes.Add(from);
        allNodes.Add(from);
        nodeScore[from] = 0;
        Point currentPoint = from;
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

            int minScore = int.MaxValue;
            foreach (var point in pathNodes)
            {
                int tmpScore = (int)distCalc(point, to) + nodeScore[point];
                if (tmpScore < minScore)
                {
                    minScore = tmpScore;
                    currentPoint = point;
                }
            }

            if (currentPoint.x == to.x && currentPoint.y == to.y)
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
            var path = new List<Point>();
            var currNode = to;
            while (!currNode.Equals(from))
            {
                path.Insert(0, currNode);
                currNode = cameFrom[currNode];
            }
            return path;
        }
    }

    static void Explore(int currentRow, int currentCol)
    {
        if (ExplorePath == null || ExplorePath.Count == 0)
        {
            var exploreOptions = GetPossiblePoints().OrderBy(x => DistanceScore(x, new Point { x = currentRow, y = currentCol }));
            Deb($"Exploring from ({currentRow},{currentCol})");
            //DebList(exploreOptions);
            foreach (var option in exploreOptions)
            {
                var path = GetShortestWay(new Point { x = currentRow, y = currentCol }, option, DistanceScore);
                if (path != null)
                {
                    var nextPoint = path[0];
                    path.Remove(nextPoint);
                    ExplorePath = path;
                    Deb($"Go explore to {nextPoint}");
                    Console.WriteLine(GetDirection(new Point { x = currentRow, y = currentCol }, nextPoint));
                    break;
                }
            }
        }
        else
        {
            var nextPoint = ExplorePath[0];
            ExplorePath.Remove(nextPoint);
            Deb($"Go explore to {nextPoint}");
            Console.WriteLine(GetDirection(new Point { x = currentRow, y = currentCol }, nextPoint));
        }
    }

    public static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int rowsCount = int.Parse(inputs[0]); // number of rows.
        int colsCount = int.Parse(inputs[1]); // number of columns.
        AlarmCooldown = int.Parse(inputs[2]); // number of rounds between the time the alarm countdown is activated and the time the alarm goes off.
        //Deb($"The maze has {rowsCount} rows and {colsCount} cols. Alert is {AlarmCooldown}");
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
                        StartPoint = new Point { x = i, y = j };
                    if (mazeRow[j] == 'C')
                    {
                        CenterPoint = new Point { x = i, y = j };
                        if (currentRow == i && currentCol == j)
                        {
                            //Deb($"The command center is found");
                            FoundCommandCenter = true; // The command center is found
                        }
                    }
                }
            }
            if (!FoundCommandCenter) // Explore
            {
                if (CenterPoint != null) // Go strait to command center
                {
                    var path = GetShortestWay(new Point { x = currentRow, y = currentCol }, CenterPoint.Value, DistanceScore);
                    var pathCenterToStart = GetShortestWay(CenterPoint.Value, StartPoint, DistanceScore, true);
                    if (path == null || pathCenterToStart == null)
                    {
                        Deb("Can't reach center");
                        Explore(currentRow, currentCol);
                    }
                    else
                    {
                        var nextPoint = path[0];
                        Deb($"Go to center to {nextPoint}");
                        Console.WriteLine(GetDirection(new Point { x = currentRow, y = currentCol }, nextPoint));
                    }
                }
                else
                {
                    Explore(currentRow, currentCol);
                }
            }
            else // Get back
            {
                var sw = new Stopwatch();
                sw.Start();
                if (HomePath == null)
                {
                    HomePath = GetShortestWay(new Point { x = currentRow, y = currentCol }, StartPoint, DistanceScoreLin, true);
                    if (HomePath == null)
                        HomePath = GetShortestWay(new Point { x = currentRow, y = currentCol }, StartPoint, DistanceScoreLin, false);
                }
                var nextPoint = HomePath[0];
                HomePath.Remove(nextPoint);
                Deb($"Go back to {nextPoint}. Turns left {HomePath.Count}. Alert in {AlarmCooldown}");
                Deb($"sw {sw.ElapsedTicks}");
                Console.WriteLine(GetDirection(new Point { x = currentRow, y = currentCol }, nextPoint));
                foreach (var point in HomePath)
                    MAZE[point.x, point.y] = '*';
            }
            //DebMaze(MAZE); // Output the maze in debug mode
        }
    }
}