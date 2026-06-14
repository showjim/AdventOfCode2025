using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day4
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            List<List<char>> input = rawInput.Select(line => line.ToList()).ToList();
            bool debugMode = true; // Control debug print
            
            Day4 day4 = new Day4();
            // Part 1
            // int result = day4.Part1(input, isRemoved, debugMode);
            // Console.WriteLine($"Part 1 output: {result}");
            // if (debugMode)
            // {
            //     Console.WriteLine("Updated grid:");
            //     foreach (var line in input)
            //     {
            //         Console.WriteLine(string.Join("", line));
            //     }
            // }

            // Part 2
            int part2Result = day4.Part2(input, debugMode);
            Console.WriteLine($"Part 2 output: {part2Result}");
        }

        int Part1(List<List<char>> input, bool debugMode)
        {
            int canBeAccessed = 0;
            for (int row = 0; row < input.Count; row++)
            {
                for (int col = 0; col < input[row].Count; col++)
                {
                    char c = input[row][col];
                    if (debugMode)
                    {
                        Console.Write(c);
                    }
                    // TODO: Implement the logic for Part 1 here
                    if (c == '@')
                    {
                        int atSymbolCount = CheckAdjacentPositions(input, row, col, '@');
                        if (atSymbolCount < 4)
                        {
                            input[row][col] = 'X'; // Mark as visited & accessible
                            canBeAccessed++;
                        }
                    }
                }
                if (debugMode) Console.WriteLine();
            }
            return canBeAccessed;
        }

        int CheckAdjacentPositions(List<List<char>> input, int row, int col, char target)
        {
            // Check the 8 adjacent positions (up, down, left, right, and diagonals)
            int count = 0;
            int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 }; // Up, Up-Right, Right, Down-Right, Down, Down-Left, Left, Up-Left
            int[] dCol = { -1, 0, 1, 1, -1, 1, 0, -1 };
            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];
                if (newRow >= 0 && newRow < input.Count && newCol >= 0 && newCol < input[newRow].Count)
                {
                    bool isTarget = input[newRow][newCol] == target || input[newRow][newCol] == 'X'; // Consider 'X' as visited & accessible
                    if (isTarget)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        int Part2(List<List<char>> input, bool debugMode)
        {
            int canBeAccessed = 0;
            int sum = 0;
            do
            {
                canBeAccessed = Part1(input, debugMode);  // or: this.Part1(input, debugMode)

                sum += canBeAccessed;
                if (debugMode)
                {
                    Console.WriteLine("Updated grid:");
                    foreach (var line in input)
                    {
                        Console.WriteLine(string.Join("", line));
                    }
                }
                // replace 'X' with '.' for the next iteration
                input = input.Select(line => line.Select(c => c == 'X' ? '.' : c).ToList()).ToList();
            } while (canBeAccessed > 0); // Repeat until no more '@' can be accessed
            
            return sum; 
        }
    }
}