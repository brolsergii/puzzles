using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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
    static HashSet<string> vocabulary = new HashSet<string>();
    #endregion

    static void Main(string[] args)
    {
        string morseLength = Console.ReadLine();
        int dictSize = int.Parse(Console.ReadLine());
        for (int i = 0; i < dictSize; i++)
            vocabulary.Add(Console.ReadLine());

        Console.WriteLine("answer");
    }
}