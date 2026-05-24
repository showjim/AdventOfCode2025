using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConsoleApp1
{
    class Day1
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] input = File.ReadAllLines(filePath);
            int arrowPos = 50;
            int count = 0;

            Day1 day1 = new Day1();
            day1.Part1(input, arrowPos, count);
        }

        void Part1(string[] input, int arrowPos, int count)
        {
            for (int i = 0; i < input.Length; i++)
            {
                char direction = input[i][0];
                int moveCount = int.Parse(input[i].Substring(1));
                if (direction == 'R')
                {
                    arrowPos += moveCount;
                }
                else if (direction == 'L')
                {
                    arrowPos -= moveCount;
                }

                // let's move the arrow back to the range of 0-99
                arrowPos = (arrowPos % 100 + 100) % 100;

                // check if the arrow is pointing to 0
                if (arrowPos == 0)
                {
                    count++;
                }
            }
            Console.WriteLine(count);
        }

        void Part2(string[] input, int arrowPos, int count)
        {
            for (int i = 0; i < input.Length; i++)
            {
                char direction = input[i][0];
                int moveCount = int.Parse(input[i].Substring(1));
                // check if the arow is passing through 0 or stop at 0
                if (direction == 'R')
                {
                    int newRawPos = arrowPos + moveCount;
                    count += (int)Math.Floor(newRawPos / 100.0) - (int)Math.Floor(arrowPos / 100.0);
                    arrowPos = newRawPos;
                }
                else if (direction == 'L')
                {
                    int newRawPos = arrowPos - moveCount;
                    count += (int)Math.Floor((arrowPos - 1) / 100.0) - (int)Math.Floor((newRawPos - 1) / 100.0);
                    arrowPos = newRawPos;
                }

                // let's move the arrow back to the range of 0-99
                arrowPos = (arrowPos % 100 + 100) % 100;
            }
            Console.WriteLine(count);
        }
    }
}