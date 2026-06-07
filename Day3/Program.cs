using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day3
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            List<List<int>> input = rawInput.Select(line => line.Select(c => int.Parse(c.ToString())).ToList()).ToList();
            Day3 day3 = new Day3();
            // day3.Part1(input);
            day3.Part2(input);
        }

        void Part1(List<List<int>> input)
        {
            List<int> joltageList = new List<int>();
            for (int i = 0; i < input.Count; i++)
            {
                int maxSoFar = -1;
                int bestJoltage = -1;
                for (int j = input[i].Count - 1; j >= 0; j--)
                {
                    if (j < input[i].Count - 1)
                    {
                        bestJoltage = Math.Max(bestJoltage, input[i][j] * 10 + maxSoFar);
                    }
                    maxSoFar = Math.Max(maxSoFar, input[i][j]);
                    if (bestJoltage == 99)
                    {
                        break;
                    }
                }
                joltageList.Add(bestJoltage);
            }
            Console.WriteLine($"Part 1 output {String.Join(", ", joltageList)}");
            Console.WriteLine($"Part 1 output {joltageList.Sum()}");
        }

        void Part2(List<List<int>> input)
        {
            List<long> joltageList = new List<long>();
            for (int i = 0; i < input.Count; i++)
            {
                List<int> curRow = input[i];
                long curJoltage = 0;

                int remainingToPick = 12;
                int startIndex = 0;
                while (remainingToPick > 0)
                {
                    int dropsAllowed = curRow.Count - startIndex - remainingToPick;

                    // List<int> searchRange = curRow.Skip(startIndex).Take(dropsAllowed + 1).ToList();
                    // int pickedMax = searchRange.Max();
                    // int pickedIndex = searchRange.IndexOf(pickedMax) + startIndex;
                    // 更高效的做法（直接遍历，不创建新 List）：
                    int pickedMax = -1;
                    int pickedIndex = -1;
                    for (int k = startIndex; k <= startIndex + dropsAllowed; k++)
                    {
                        if (curRow[k] > pickedMax)
                        {
                            pickedMax = curRow[k];
                            pickedIndex = k;
                        }
                    }

                    // curJoltage += pickedMax * (long)Math.Pow(10, remainingToPick - 1);
                    // Math.Pow 的精度问题
                    curJoltage = curJoltage * 10 + pickedMax;

                    startIndex = pickedIndex + 1;
                    remainingToPick--;
                }
                joltageList.Add(curJoltage);
            }
            Console.WriteLine($"Part 2 output {String.Join(", ", joltageList)}");
            Console.WriteLine($"Part 2 output {joltageList.Sum()}");
        }
    }
}