using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day5
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);

            int i = Array.IndexOf(rawInput, "");   // index of the empty-string line
            List<(long min, long max)> freshIDRanges = rawInput.Take(i).Select(line =>
            {
                string[] parts = line.Split('-');
                return (min: long.Parse(parts[0]), max: long.Parse(parts[1]));
            }).ToList();
            List<long> checkIDs = rawInput.Skip(i + 1).Select(line => long.Parse(line)).ToList();
            
            Day5 day5 = new Day5();
            // for (int i = 0; i < rawInput.Length; i++)
            // {
            //     Console.WriteLine($"[{i}] len={rawInput[i].Length} text='{rawInput[i]}'");
            // }
            // day5.Part1(freshIDRanges, checkIDs);
            day5.Part2(freshIDRanges);

        }

        void Part1(List<(long min, long max)> freshIDRanges, List<long> checkIDs)
        {
            int count = 0;
            for (int i = 0; i < checkIDs.Count; i++)
            {
                for (int j = 0; j < freshIDRanges.Count; j++)
                {
                    if (checkIDs[i] >= freshIDRanges[j].min && checkIDs[i] <= freshIDRanges[j].max)
                    {
                        // Console.WriteLine($"checkIDs[{i}]={checkIDs[i]} is in range freshIDRanges[{j}]={freshIDRanges[j]}");
                        count++;
                        break;  // no need to check other ranges for this checkID
                    }
                }
            }
            Console.WriteLine($"Part 1: {count} check IDs are in the fresh ID ranges.");
        }

        void Part2(List<(long min, long max)> freshIDRanges)
        {
            var sortedRanges = freshIDRanges.OrderBy(r => r.min).ToList();
            long curMin = sortedRanges[0].min;
            long curMax = sortedRanges[0].max;
            long count = 0;
            for (int i = 1; i < sortedRanges.Count; i++)
            {

                long nextMin = sortedRanges[i].min;
                long nextMax = sortedRanges[i].max;
                if (nextMin <= curMax)
                {
                    curMax = Math.Max(curMax, nextMax);
                    // Console.WriteLine($"Ranges overlap: ({curMin}, {curMax}) and ({nextMin}, {nextMax})");
                }
                else
                {
                    count += curMax - curMin + 1;
                    curMin = sortedRanges[i].min;
                    curMax = sortedRanges[i].max;
                }
            }
            count += curMax - curMin + 1;  // add the last range
            Console.WriteLine($"Part 2: {count} check IDs are in the fresh ID ranges.");
        }
    }
}