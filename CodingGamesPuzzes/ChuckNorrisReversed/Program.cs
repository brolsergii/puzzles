using System;
using System.Linq;

class Solution
{
    static void Main(string[] args)
    {
        string encrypted = Console.ReadLine();
        var sequence = encrypted.Split(' ');
        if (sequence.Length % 2 != 0)
        {
            Console.WriteLine("INVALID");
            return;
        }

        string value = "";
        int buffer = 0;
        for (int i = 0; i < sequence.Length; i += 2)
        {
            if (sequence[i] == "0")
            {
                foreach (var l in sequence[i + 1])
                {
                    value = value + "1";
                    buffer = buffer * 2 + 1;
                }
            }
            else if (sequence[i] == "00")
            {
                foreach (var l in sequence[i + 1])
                {
                    value = value + "0";
                    buffer = buffer * 2;
                }
            }
            else
            {
                Console.WriteLine("INVALID");
                return;
            }
        }

        if (value.Length % 7 != 0)
        {
            Console.WriteLine("INVALID");
            return;
        }

        string result = "";
        int letters = value.Length / 7;
        for (int i = 0; i < letters; i++)
        {
            int numLetter = value.ToCharArray().Skip(7 * i).Take(7).Aggregate(0, (x, y) => x * 2 + int.Parse(y.ToString()));
            var letter = Convert.ToChar(numLetter);
            Console.Error.WriteLine($"Value {i} L {letter}");
            result += letter;
        }
        Console.WriteLine(result);
    }
}