using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;

namespace ConsoleApp1
{
    class Day9
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            // convert the raw input into a list of list of int, each line is split by ","
            List<(long x, long y)> input = rawInput.Select(line => line.Split(",").Select(long.Parse).ToList()).Select(coords => (coords[0], coords[1])).ToList();

            Day9 day9 = new Day9();
            day9.Part1(input);
            // day9.Part2(input);
        }

        void Part1(List<(long x, long y)> input)
        {
            long maxArea = 0;
            for (int i = 0; i < input.Count - 1; i++)
            {
                for (int j = i + 1; j < input.Count; j++)
                {
                    maxArea = Math.Max(maxArea, (Math.Abs(input[i].x - input[j].x) + 1) * (Math.Abs(input[i].y - input[j].y) + 1));
                }
            }
            Console.WriteLine($"Part 1: {maxArea}");
        }

        void Part2(List<(long x, long y)> input)
        {
            Console.WriteLine($"Part 2: ");
        }
    }
}