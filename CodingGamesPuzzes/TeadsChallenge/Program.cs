using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Pair : IEquatable<Pair>
{
    public Pair(int x, int y) { this.x = x; this.y = y; }
    public int x;
    public int y;
    public bool Equals(Pair obj) => obj.x == x && obj.y == y;
    public override int GetHashCode() => (x << 16) + y;
}

class Solution
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
    #endregion

    #region Global context
    static HashSet<Pair> pairs = new HashSet<Pair>();
    static HashSet<int> ids = new HashSet<int>();
    static Dictionary<int, int> connectivity = new Dictionary<int, int>(1024);
    static Dictionary<int, List<int>> pairConnections = new Dictionary<int, List<int>>(1024);
    #endregion

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
        Deb($"Starting with {n} pairs");
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi
            //pairs.Add(new Pair(xi, yi));
            //pairs.Add(new Pair(yi, xi));
            ids.Add(xi);
            ids.Add(yi);
            connectivity[xi] = connectivity.ContainsKey(xi) ? connectivity[xi] + 1 : 1;
            connectivity[yi] = connectivity.ContainsKey(yi) ? connectivity[yi] + 1 : 1;
            if (pairConnections.ContainsKey(xi))
                pairConnections[xi].Add(yi);
            else
                pairConnections[xi] = new List<int>(new int[] { yi });
            if (pairConnections.ContainsKey(yi))
                pairConnections[yi].Add(xi);
            else
                pairConnections[yi] = new List<int>(new int[] { xi });
        }

        int minTime = int.MaxValue;
        var connections = connectivity.OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
        for (int i = 0; i < connections.Count(); i++)
        {
            var weights = new Dictionary<int, int>();
            var pathedNodes = new HashSet<int>();
            var waitingNodes = new HashSet<int>();
            int firstNode = connections[i];
            weights[firstNode] = 0;
            pathedNodes.Add(firstNode);
            waitingNodes.Add(firstNode);
            while (pathedNodes.Count != ids.Count)
            {
                int currentNode = waitingNodes.First();
                waitingNodes.Remove(currentNode);
                var closeNodes = pairConnections[currentNode];
                foreach (var node in closeNodes)
                {
                    if (!pathedNodes.Contains(node))
                    {
                        weights[node] = weights[currentNode] + 1;
                        waitingNodes.Add(node);
                        pathedNodes.Add(node);
                    }
                }
            }
            int tmpMinValue = weights.Max(x => x.Value);
            if (tmpMinValue < minTime)
                minTime = tmpMinValue;
        }

        Console.WriteLine(minTime); // The minimal amount of steps required to completely propagate the advertisement
    }
}