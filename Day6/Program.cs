using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day6
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            List<List<string>> input = rawInput.Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();
            
            Day6 day6 = new Day6();
            // day6.Part1(input);
            day6.Part2(rawInput);
        }

        void Part1(List<List<string>> input)
        {
            long sum = 0;
            for (int i = 0; i < input[^1].Count; i++)
            {
                IEnumerable<long> colValues = input.SkipLast(1).Select(row => long.Parse(row[i]));
                sum += input[^1][i] == "+" ? colValues.Sum() : colValues.Aggregate(1L, (a, b) => a * b);
            }
            Console.WriteLine($"Part 1: {sum}");
        }

        void Part2(string[] input)
        {
            List<List<char>> grid = input.Select(l => l.ToList()).ToList();
            int rows = grid.Count;
            int cols = grid[0].Count;

            bool[] isSeparator = Enumerable.Range(0, cols)
                .Select(c => Enumerable.Range(0, rows).All(r => grid[r][c] == ' '))
                .ToArray();

            int c = 0;
            long total = 0;
            while (c < cols)
            {
                while (c < cols && isSeparator[c]) c++;   // skip separator columns
                if (c >= cols) break;
                int start = c;
                while (c < cols && !isSeparator[c]) c++;  // gobble the region
                int end = c - 1;                          // inclusive last column of the region

                // Console.WriteLine($"region: {start}-{end}");   // TODO Step 4 replaces this
                var numbers = new List<long>();
                for (int xc = start; xc <= end; xc++)
                {
                    string digits = new string(
                        Enumerable.Range(0, rows - 1)          // rows 0 .. rows-2  (NOT the operator row)
                                .Select(r => grid[r][xc])
                                .Where(ch => ch != ' ')
                                .ToArray());
                    if (digits.Length > 0)
                        numbers.Add(long.Parse(digits));
                }
                char op = grid[rows - 1].Skip(start).Take(end - start + 1)
                    .First(ch => ch == '+' || ch == '*');

                total += op == '+' ? numbers.Sum() : numbers.Aggregate(1L, (a, b) => a * b);    
            }
            Console.WriteLine($"Part 2: {total}");
        }
    }
}