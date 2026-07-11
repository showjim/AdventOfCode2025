using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
            day7.Part1(input);
            // day7.Part2(rawInput);
        }

        void Part1(List<List<char>> input)
        {
            List<(int row, int col)>  posToCheck = new();
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
                            break;
                        }
                    }
                }
                else 
                {
                    // find split positons '^'
                    foreach ((int row, int col) pos in posToCheck.ToList())
                    {
                        if (pos.row == i) // only checj the positions in the current row
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
                                if (!posToCheck.Any(p => p.row == pos.row + 1 && p.col == pos.col - 1))
                                {
                                    posToCheck.Add((pos.row + 1, pos.col - 1));
                                    input[pos.row][pos.col - 1] = '|';
                                    beamSplit = true;
                                }
                                if (!posToCheck.Any(p => p.row == pos.row + 1 && p.col == pos.col + 1))
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

        void Part2()
        {
            Console.WriteLine($"Part 2: ");
        }
    }
}