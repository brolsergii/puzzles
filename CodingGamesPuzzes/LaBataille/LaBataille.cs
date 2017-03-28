using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Card
{
    public Card(string value)
    {
        Color = value.Last().ToString();
        ValueText = value.Substring(0, value.Length - 1);
        Value = int.Parse(ValueText.Replace("J", "11").Replace("Q", "12").Replace("K", "13").Replace("A", "14"));
    }
    public int Value;
    public string ValueText;
    public string Color;
    public override string ToString() => ValueText + Color;
}

class Result
{
    public int holder;
    public Queue<Card> firstCards;
    public Queue<Card> secondCards;
}

class Solution
{
    #region Auxilary methods
    static void Deb(object o) => Console.Error.WriteLine(o);
    static void DebList(IEnumerable<object> e) => Console.Error.WriteLine(e.Aggregate((x, y) => $"{x} {y}"));
    #endregion

    #region Game state
    static Queue<Card> tas1 = new Queue<Card>();
    static Queue<Card> tas2 = new Queue<Card>();
    #endregion

    static Result Combat(bool inBataille = false)
    {
        if (inBataille && (tas1.Count == 0 || tas2.Count == 0))
        {
            tas1.Clear();
            tas2.Clear();
            return null;
        }
        var c1 = tas1.Dequeue();
        var c2 = tas2.Dequeue();
        if (c1.Value > c2.Value)
        {
            Deb($"  [B:{inBataille}] First wins");
            return new Result { holder = 1, firstCards = new Queue<Card>(new Card[] { c1 }), secondCards = new Queue<Card>(new Card[] { c2 }) };
        }
        if (c2.Value > c1.Value)
        {
            Deb($"  [B:{inBataille}] Second wins");
            return new Result { holder = 2, firstCards = new Queue<Card>(new Card[] { c1 }), secondCards = new Queue<Card>(new Card[] { c2 }) };
        }
        if (c1.Value == c2.Value)
        {
            Deb($"  [B:{inBataille}] Go battle");
            return Bataille();
        }
        return null;
    }

    static Result Bataille()
    {
        if (tas1.Count < 3 || tas2.Count < 3)
        {
            tas1.Clear();
            tas2.Clear();
            return null;
        }
        var res1 = new Queue<Card>();
        var res2 = new Queue<Card>();
        res1.Enqueue(tas1.Dequeue());
        res1.Enqueue(tas1.Dequeue());
        res1.Enqueue(tas1.Dequeue());
        res2.Enqueue(tas2.Dequeue());
        res2.Enqueue(tas2.Dequeue());
        res2.Enqueue(tas2.Dequeue());
        var comb = Combat(true);
        while (comb.firstCards.Count > 0)
            res1.Enqueue(comb.firstCards.Dequeue());
        while (comb.secondCards.Count > 0)
            res2.Enqueue(comb.secondCards.Dequeue());
        return new Result { holder = comb.holder, firstCards = res1, secondCards = res2 };
    }

    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        for (int i = 0; i < n; i++)
            tas1.Enqueue(new Card(Console.ReadLine()));
        int m = int.Parse(Console.ReadLine());
        for (int i = 0; i < m; i++)
            tas2.Enqueue(new Card(Console.ReadLine()));

        int count = 0;
        while (tas1.Count > 0 && tas2.Count > 0)
        {
            count++;
            Deb($"Step {count}");
            DebList(tas1);
            DebList(tas2);
            var res = Combat();
            if (res == null) { break; }
            if (res.holder == 1)
            {
                while (res.firstCards.Count > 0)
                    tas1.Enqueue(res.firstCards.Dequeue());
                while (res.secondCards.Count > 0)
                    tas1.Enqueue(res.secondCards.Dequeue());
            }
            else
            {
                while (res.firstCards.Count > 0)
                    tas2.Enqueue(res.firstCards.Dequeue());
                while (res.secondCards.Count > 0)
                    tas2.Enqueue(res.secondCards.Dequeue());
            }

        }
        if (tas1.Count == tas2.Count && tas2.Count == 0)
            Console.WriteLine("PAT");
        else if (tas1.Count == 0)
            Console.WriteLine($"2 {count}");
        else
            Console.WriteLine($"1 {count}");
    }
}