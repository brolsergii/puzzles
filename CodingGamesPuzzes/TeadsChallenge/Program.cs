using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Pair : IEquatable<Pair>
{
    public Pair(int x, int y) { this.x = x; this.y = y; }
    public int x;
    public int y;
    public bool Equals(Pair obj) => obj.x == x && obj.y == y;
    public override int GetHashCode() => x + y; // (x,y) == (y,x)
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
    static HashSet<int> ids = new HashSet<int>();
    static Dictionary<int, int> connectivity = new Dictionary<int, int>(1024 * 32);
    static Dictionary<int, List<int>> pairConnections = new Dictionary<int, List<int>>(1024 * 32);
    #endregion

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine()); // the number of adjacency relations
        Deb($"Starting with {n} pairs");
        var sw = new Stopwatch();
        sw.Start();
        for (int i = 0; i < n; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int xi = int.Parse(inputs[0]); // the ID of a person which is adjacent to yi
            int yi = int.Parse(inputs[1]); // the ID of a person which is adjacent to xi
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
        Deb($"Initial contruction during {sw.ElapsedTicks}");
        sw.Reset();
        sw.Start();
        int minTime = int.MaxValue;
        //var connexions = connectivity.Where(x => x.Value > 1).OrderByDescending(x => x.Value).Select(x => x.Key).ToArray();
        //var connexions = connectivity.Where(x => x.Value > 1).OrderBy(x => x.Value).Select(x => x.Key).ToArray();
        var connexions = connectivity.Where(x => x.Value > 1).Select(x => x.Key).ToArray();
        int connexionsNumber = connexions.Count();
        Deb($"Go for {connexionsNumber} connextions");
        Dictionary<int, int> weights;
        HashSet<int> waitingNodes;
        Dictionary<Pair, int> distances = new Dictionary<Pair, int>(1024 * 32);
        for (int i = 0; i < connexionsNumber; i++)
        {
            int tmpMinWeight = int.MinValue;
            weights = new Dictionary<int, int>(1024);
            waitingNodes = new HashSet<int>();
            int firstNode = connexions[i];
            weights[firstNode] = 0;
            waitingNodes.Add(firstNode);
            while (waitingNodes.Count > 0)
            {
                int currentNode = waitingNodes.First();
                waitingNodes.Remove(currentNode);
                if (weights[currentNode] >= minTime)
                    break; // no good results are possible
                var closeNodes = pairConnections[currentNode];
                foreach (var node in closeNodes)
                {
                    if (!weights.ContainsKey(node))
                    {
                        int newWeight = weights[currentNode] + 1;
                        weights.Add(node, newWeight);
                        if (tmpMinWeight < newWeight)
                            tmpMinWeight = newWeight;
                        waitingNodes.Add(node);
                        var pair = new Pair(firstNode, node);
                        if (!distances.ContainsKey(pair))
                        {
                            distances.Add(pair, newWeight);
                        }
                    }
                }
            }
            if (tmpMinWeight < minTime)
                minTime = tmpMinWeight;
        }
        Deb($"Final calc during {sw.ElapsedTicks}");

        Console.WriteLine(minTime); // The minimal amount of steps required to completely propagate the advertisement
    }
}