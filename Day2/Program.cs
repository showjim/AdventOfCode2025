using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day2
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            // List<List<string>> input = rawInput[0].Split(',').Select(x => x.Split('-').ToList()).ToList();
            List<List<long>> longInput = rawInput[0].Split(',').Select(x => x.Split('-').Select(long.Parse).ToList()).ToList();
            // Console.WriteLine(string.Join("\n", longInput.Select(x => string.Join("-", x))));
            Day2 day2 = new Day2();
            day2.Part2(longInput);
        }

        void Part1(List<List<long>> input)
        {
            long sum = 0;
            foreach (var pair in input)
            {
                long low = pair[0];
                long high = pair[1];
                if (low <= high)
                {
                    for (long i = low; i <= high; i++)
                    {
                        if (IsInvalidProductID(i))
                        {
                            sum += i;
                        }
                    }
                }
            }
            Console.WriteLine($"Sum of invalid product IDs: {sum}");
        }

        void Part2(List<List<long>> input)
        {
            long sum = 0;
            // long[] inValidProductIDs = new long[0]; 
            foreach (var pair in input)
            {
                long low = pair[0];
                long high = pair[1];
                for (long i = low; i <= high; i++)
                {
                    if (IsInvalidProductIDPart2(i))
                    {
                        sum += i;
                        // inValidProductIDs = inValidProductIDs.Append(i).ToArray();
                    }
                }
            }
            Console.WriteLine($"Sum of invalid product IDs: {sum}");
            // Console.WriteLine($"Invalid product IDs: {string.Join('\n', inValidProductIDs)}");
        }

        bool IsInvalidProductID(long id)
        {
            string idStr = id.ToString();
            // check if the length id can divide by 2, if so, it's a valid product ID
            if (idStr.Length % 2 != 0)
            {
                return false;
            }
            // check if the first half of the id is the same as the second half
            string firstHalf = idStr.Substring(0, idStr.Length / 2);
            string secondHalf = idStr.Substring(idStr.Length / 2);
            return firstHalf == secondHalf;
        }

        bool IsInvalidProductIDPart2(long id)
        {
            string idStr = id.ToString();
            
            for (int i = 1; i < idStr.Length/2 + 1; i++)
            {
                if (idStr.Length % i != 0)
                {
                    continue;
                }
                string repeatPart  = idStr.Substring(0, i);
                string repeated = string.Concat(Enumerable.Repeat(repeatPart, idStr.Length / i));
                if (idStr == repeated)                
                {
                    return true;
                }
            }
            return false;
        }
    }
}