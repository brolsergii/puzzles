using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

#region Auxilary structs
public enum Direction
{
    S = 0, // South
    E = 1, // East
    N = 2, // North
    W = 3  // West
}

public struct Point : IComparable<Point>, IEquatable<Point>
{
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x;
    public int y;

    public int CompareTo(Point obj) => (Equals(obj)) ? 0 : 1;
    public bool Equals(Point other) => (other.x == x && other.y == y);
    public override string ToString() => $"({x},{y})";
    public override int GetHashCode() => (x << 16) + y;
}
#endregion

class Solution
{
    #region Auxilary methods
    static void Deb(object o) => Console.Error.WriteLine(o);
    static void DebList(IEnumerable<object> e) => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x} {y}"));
    static void DebMap(char[,] map)
    {
        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
                Console.Error.Write(map[i, j]);
            Console.Error.WriteLine();
        }
    }
    #endregion

    #region GameState
    static char[,] Gmap;
    static Direction CurrentDirection = Direction.S;
    static Point CurrentPosition;
    static List<Point> teleports = new List<Point>();
    static bool Destroyer = false;
    static bool Inverted = false;
    #endregion

    static void ChangeDirection(bool init)
    {
        if (Inverted)
        {
            if (init)
                CurrentDirection = Direction.W;
            else
            {
                CurrentDirection--;
                if ((int)CurrentDirection < 0) { CurrentDirection = Direction.W; }
            }
        }
        else
        {
            if (init)
                CurrentDirection = Direction.S;
            else
            {
                CurrentDirection++;
                if ((int)CurrentDirection > 3) { CurrentDirection = Direction.S; }
            }
        }
    }

    static Point GetNextPos(Point old, Direction dir, ref string direction)
    {
        Point nextPos;
        if (CurrentDirection == Direction.S)
        {
            nextPos = new Point(CurrentPosition.x + 1, CurrentPosition.y);
            direction = "SOUTH";
        }
        else if (CurrentDirection == Direction.E)
        {
            nextPos = new Point(CurrentPosition.x, CurrentPosition.y + 1);
            direction = "EAST";
        }
        else if (CurrentDirection == Direction.N)
        {
            nextPos = new Point(CurrentPosition.x - 1, CurrentPosition.y);
            direction = "NORTH";
        }
        else // (CurrentDirection == Direction.W)
        {
            nextPos = new Point(CurrentPosition.x, CurrentPosition.y - 1);
            direction = "WEST";
        }
        return nextPos;
    }

    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int rows = int.Parse(inputs[0]);
        int cols = int.Parse(inputs[1]);
        Gmap = new char[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            string row = Console.ReadLine();
            for (int j = 0; j < row.Length; j++)
            {
                Gmap[i, j] = row[j];
                if (Gmap[i, j] == '@')
                    CurrentPosition = new Point(i, j);
                if (Gmap[i, j] == 'T')
                    teleports.Add(new Point(i, j));
            }
        }
        DebMap(Gmap);
        Deb($"Current position {CurrentPosition}");
        bool end = false;
        int numberOfSteps = 0;
        var result = new List<string>();
        while (!end)
        {
            numberOfSteps++;
            Point nextPos;
            string direction = "";
            nextPos = GetNextPos(CurrentPosition, CurrentDirection, ref direction);
            Deb($"Next {nextPos}: {Gmap[nextPos.x, nextPos.y]}");
            switch (Gmap[nextPos.x, nextPos.y])
            {
                case 'X':
                    {
                        //Deb($"{numberOfSteps}: X");
                        if (Destroyer)
                        {
                            Gmap[nextPos.x, nextPos.y] = ' '; // The wall is down
                            CurrentPosition = nextPos;
                            result.Add(direction);
                        }
                        else
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                ChangeDirection(i == 0);
                                string usless = "";
                                nextPos = GetNextPos(CurrentPosition, CurrentDirection, ref usless);
                                if (Gmap[nextPos.x, nextPos.y] != '#' && (Gmap[nextPos.x, nextPos.y] != 'X' || Destroyer))
                                    break;
                            }
                        }
                        break;
                    }
                case '#':
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            ChangeDirection(i == 0);
                            string usless = "";
                            nextPos = GetNextPos(CurrentPosition, CurrentDirection, ref usless);
                            if (Gmap[nextPos.x, nextPos.y] != '#' && (Gmap[nextPos.x, nextPos.y] != 'X' || Destroyer))
                                break;
                        }
                        break;
                    }
                case '$':
                    {
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        end = true;
                        break;
                    }
                case 'I':
                    {
                        Inverted = !Inverted;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'B':
                    {
                        Destroyer = !Destroyer;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'S':
                    {
                        CurrentDirection = Direction.S;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'W':
                    {
                        CurrentDirection = Direction.W;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'N':
                    {
                        CurrentDirection = Direction.N;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'E':
                    {
                        CurrentDirection = Direction.E;
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                case 'T':
                    {
                        nextPos = teleports.Where(x => !x.Equals(nextPos)).First();
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
                default:
                    {
                        CurrentPosition = nextPos;
                        result.Add(direction);
                        break;
                    }
            }
            if (numberOfSteps > 5000)
            {
                Deb("Looped");
                DebList(result);
                end = true;
                result.Clear();
                result.Add("LOOP");
            }

        }
        foreach (var step in result)
            Console.Out.WriteLine(step);
    }
}