using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;

namespace ConsoleApp1
{
    class Day8
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt");
            string[] rawInput = File.ReadAllLines(filePath);
            // convert the raw input into a list of list of int, each line is split by ","
            List<List<int>> input = rawInput.Select(line => line.Split(",").Select(int.Parse).ToList()).ToList();
            
            Day8 day8 = new Day8();
            day8.Part1(input);
            // day8.Part2(input);
        }

        void Part1(List<List<int>> input)
        {
            var (sorted, n) = Setup(input);

            // create an array of int to present the index of junction box in "input" list
            int[] parent = Enumerable.Range(0, input.Count).ToArray();

            // union the junction box with the smallest distance
            for (int i = 0; i < 1000; i++)
            {
                int index1 = sorted[i].i;
                int index2 = sorted[i].j;

                Union(parent, index1, index2);
            }

            // find the largest 3 values in "parent" and multiple them together
            // Console.WriteLine($"parent: [{string.Join(", ", parent)}]");
            int[] realRoots = Enumerable.Range(0, n).Select(i => Find(parent, i)).ToArray();
            // int[] setSize = realRoots.Distinct().Select(x => realRoots.Count(y => y == x)).OrderByDescending(x => x).Take(3).ToArray();
            var top3 = realRoots.GroupBy(r => r).Select(g => g.Count()).OrderDescending().Take(3);

            Console.WriteLine($"Part 1: {top3.Aggregate((x, y) => x * y)}");
        }

        // Shared — called by both parts
        (List<(double distance, int i, int j)> sorted, int n) Setup(List<List<int>> input)
        {
            int n = input.Count;
            
            List<(double distance, int i, int j)> distanceList = new();
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    distanceList.Add((CalculateDistance(input[i], input[j]), i, j));
            
            var sorted = distanceList.OrderBy(x => x.distance).ToList();
            return (sorted, n);
        }


        static double CalculateDistance(List<int> box1, List<int> box2)
        {
            double distance = 0;
            for (int i = 0; i < box1.Count; i++)
            {
                distance += Math.Pow(box1[i] - box2[i], 2);
            }
            return distance; // Math.Sqrt(distance);
        }

        static int Find(int[] parent, int index)
        {
            // find the junction dox index until find the root index, then return the root index
            while (parent[index] != index)
            {
                index = parent[index];
            }
            return index;

        }

        static bool Union(int[] parent, int index1, int index2)
        {
            // find the root of both indexes
            int root1 = Find(parent, index1);
            int root2 = Find(parent, index2);

            // if they are not already in the same set, then union them
            if (root1 != root2)
            {
                parent[root2] = root1;
                return true; // actually merged
            }
            return false; // already same circuit, nothing happened
        }

        void Part2(List<List<int>> input)
        {
            var (sorted, n) = Setup(input);
            // create an array of int to present the index of junction box in "input" list
            int[] parent = Enumerable.Range(0, input.Count).ToArray();

            // union the junction box with the smallest distance
            int countUniqueRoots = n;
            int k = 0;
            while (countUniqueRoots > 1)
            {
                int index1 = sorted[k].i;
                int index2 = sorted[k].j;

                bool merged = Union(parent, index1, index2);
                // count the number of unique roots
                // countUniqueRoots = Enumerable.Range(0, n).Select(x => Find(parent, x)).Distinct().Count();
                if (merged) countUniqueRoots--;
                k++;
            }

            // find the last 2 junction boexes X coordinates, and multiply them together
            var last = sorted[k - 1];
            Console.WriteLine($"Part 2: {input[last.i][0] * input[last.j][0]}");
        }
    }
}