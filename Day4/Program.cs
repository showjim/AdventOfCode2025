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
            Day4 day4 = new Day4();
            day4.Part1(input);
        }

        void Part1(List<List<char>> input)
        {
            int canBeAccessed = 0;
            for (int row = 0; row < input.Count; row++)
            {
                for (int col = 0; col < input[row].Count; col++)
                {
                    char c = input[row][col];
                    Console.Write(c);
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
                Console.WriteLine();
            }
            Console.WriteLine($"Part 1 output: {canBeAccessed}");
            Console.WriteLine("Updated grid:");
            foreach (var line in input)            
            {
                Console.WriteLine(string.Join("", line));
            }
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
                    if ((input[newRow][newCol] == target) || input[newRow][newCol] == 'X')
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        void Part2(List<List<char>> input)
        {
            Console.WriteLine($"Part 2 output");
        }
    }
}