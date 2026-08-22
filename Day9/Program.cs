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
            day9.Part2(input);
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
            int n = input.Count;

            // The red tiles form a closed loop: consecutive tiles in the list are connected
            // by a straight line of green tiles (they share a row or column), and the list wraps.
            // So "edge i" is the segment from input[i] to input[(i+1) % n].
            // A tile is GREEN if it lies ON one of these segments, OR is strictly INSIDE the loop.
            // A rectangle is valid iff EVERY tile in it is red or green.

            // Quick O(1) lookup for red tiles.
            var red = new HashSet<(long x, long y)>(input);

            // Is (x, y) ON any loop segment?
            bool OnEdge(long x, long y)
            {
                for (int i = 0; i < n; i++)
                {
                    long x1 = input[i].x, y1 = input[i].y;
                    long x2 = input[(i + 1) % n].x, y2 = input[(i + 1) % n].y;
                    if (x1 == x2) // vertical segment: all tiles at column x1 between y1 and y2
                    {
                        if (x == x1 && y >= Math.Min(y1, y2) && y <= Math.Max(y1, y2)) return true;
                    }
                    else // horizontal segment: all tiles at row y1 between x1 and x2
                    {
                        if (y == y1 && x >= Math.Min(x1, x2) && x <= Math.Max(x1, x2)) return true;
                    }
                }
                return false;
            }

            // Even-odd rule: cast a ray from the tile's center to the right.
            // Count how many VERTICAL loop segments the ray crosses (vertical segments only,
            // since the ray is horizontal). Odd count => tile is strictly inside the loop.
            bool Inside(long x, long y)
            {
                double px = x + 0.5, py = y + 0.5;
                int crossings = 0;
                for (int i = 0; i < n; i++)
                {
                    long x1 = input[i].x, y1 = input[i].y;
                    long x2 = input[(i + 1) % n].x, y2 = input[(i + 1) % n].y;
                    // segment is vertical, lies to the right of the tile, and straddles the ray height
                    if (x1 == x2 && x1 > px && (y1 < py) != (y2 < py)) crossings++;
                }
                return crossings % 2 == 1;
            }

            bool RedOrGreen(long x, long y) => red.Contains((x, y)) || OnEdge(x, y) || Inside(x, y);

            long maxArea = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    long x1 = Math.Min(input[i].x, input[j].x), x2 = Math.Max(input[i].x, input[j].x);
                    long y1 = Math.Min(input[i].y, input[j].y), y2 = Math.Max(input[i].y, input[j].y);
                    long width = x2 - x1 + 1, height = y2 - y1 + 1;

                    bool valid;
                    if (width < 3 || height < 3)
                    {
                        // Thin rectangle: no interior tile to reason about, just check every tile.
                        valid = true;
                        for (long x = x1; x <= x2 && valid; x++)
                            for (long y = y1; y <= y2 && valid; y++)
                                if (!RedOrGreen(x, y)) valid = false;
                    }
                    else
                    {
                        // (1) All four corners must be red or green.
                        valid = RedOrGreen(x1, y1) && RedOrGreen(x2, y1) && RedOrGreen(x1, y2) && RedOrGreen(x2, y2);

                        // (2) No loop segment may pass through the strict interior of the rectangle,
                        //     otherwise it would split the rectangle into inside + outside parts.
                        if (valid)
                        {
                            for (int e = 0; e < n && valid; e++)
                            {
                                long ex1 = input[e].x, ey1 = input[e].y;
                                long ex2 = input[(e + 1) % n].x, ey2 = input[(e + 1) % n].y;
                                if (ex1 == ex2) // vertical segment
                                {
                                    if (x1 < ex1 && ex1 < x2
                                        && Math.Min(ey1, ey2) < y2 && Math.Max(ey1, ey2) > y1) valid = false;
                                }
                                else // horizontal segment
                                {
                                    if (y1 < ey1 && ey1 < y2
                                        && Math.Min(ex1, ex2) < x2 && Math.Max(ex1, ex2) > x1) valid = false;
                                }
                            }
                        }

                        // (3) One tile strictly inside must be green. This catches rectangles that sit
                        //     entirely in an outside "bay" of the loop even though all 4 corners are on it.
                        if (valid) valid = RedOrGreen(x1 + 1, y1 + 1);
                    }

                    if (valid) maxArea = Math.Max(maxArea, width * height);
                }
            }
            Console.WriteLine($"Part 2: {maxArea}");
        }
    }
}