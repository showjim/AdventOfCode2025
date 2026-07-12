using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;

namespace ConsoleApp1
{
    class Day7
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            List<List<char>> input = rawInput.Select(line => line.ToList()).ToList();
            
            Day7 day7 = new Day7();
            // day7.Part1(input);
            day7.Part2(input);
        }

        void Part1(List<List<char>> input)
        {
            List<(int row, int col)>  posToCheck = new();
            HashSet<(int row, int col)> queued = new();   // remembers everything we've ever added
            int countSplit = 0;
            for (int i = 0; i < input.Count; i++)
            {
                if (i == 0)
                {
                    // find the start positon 'S'
                    for (int j = 0; j < input[i].Count; j++)
                    {
                        if (input[i][j] == 'S')
                        {
                            posToCheck.Add((i + 1, j));
                            queued.Add((i + 1, j));
                            break;
                        }
                    }
                }
                else 
                {
                    // find split positons '^'
                    foreach ((int row, int col) pos in posToCheck.ToList())
                    {
                        if (pos.row == i) // only check the positions in the current row
                        {
                            // if (row == 14)
                            // {
                            //     Console.WriteLine("");
                            // }
                            if (input[pos.row][pos.col] == '^')
                            {
                                bool beamSplit = false;
                                // Console.WriteLine($"row: {pos.row}, col: {pos.col}");
                                // add new positions to posToCheck, if they are not already in posToCheck
                                if (queued.Add((pos.row + 1, pos.col - 1))) // HashSet.Add returns true if it was new, false if already there. 
                                {
                                    posToCheck.Add((pos.row + 1, pos.col - 1));
                                    input[pos.row][pos.col - 1] = '|';
                                    beamSplit = true;
                                }
                                if (queued.Add((pos.row + 1, pos.col + 1)))
                                {
                                    posToCheck.Add((pos.row + 1, pos.col + 1));
                                    input[pos.row][pos.col + 1] = '|';
                                    beamSplit = true;
                                }
                                if (beamSplit)
                                {
                                    countSplit++;
                                }
                            }
                            else
                            {
                                // change the symbol to '|' to indicate that the path has been taken
                                input[pos.row][pos.col] = '|';
                                posToCheck.Add((pos.row + 1, pos.col));
                            }
                        }
                    }
                }
                // print current row in input
                Console.WriteLine(string.Join("", input[i]));
            }
            // // print the count of '^' in the all rows of input
            // int count = input.Sum(row => row.Count(c => c == '^'));
            // Console.WriteLine($"Part 1: {count}");
            Console.WriteLine($"Part 1: {countSplit}");
        }

        void Part2(List<List<char>> input)
        {
            List<(int row, int col)>  posToCheck = new();
            for (int i = 0; i < input.Count; i++)
            {
                if (i == 0)
                {
                    // find the start positon 'S'
                    for (int j = 0; j < input[i].Count; j++)
                    {
                        if (input[i][j] == 'S')
                        {
                            posToCheck.Add((i, j));
                            break;
                        }
                    }
                }
            }
            Console.WriteLine($"Part 2: {Ways(posToCheck[0].row, posToCheck[0].col, input)}");
        }

        static Dictionary<(int, int), BigInteger> cache = new();
        static BigInteger Ways(int row, int col, List<List<char>> input)
        {
            BigInteger totalWays = 0;
            if (row == input.Count)
            {
                return 1;
            }
            if (cache.ContainsKey((row, col)))
            {
                return cache[(row, col)];
            }

            if (input[row][col] == '^')
            {
                if (col - 1 >= 0)
                {
                    totalWays += Ways(row + 1, col - 1, input);
                }
                if (col + 1 < input[row].Count)
                {
                    totalWays += Ways(row + 1, col + 1, input);
                }
            }
            else if (input[row][col] == '.' || input[row][col] == 'S')
            {
                totalWays = Ways(row + 1, col, input);
            }

            cache[(row, col)] = totalWays;
            return totalWays;
        }
    }
}