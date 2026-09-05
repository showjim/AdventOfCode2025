using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Day10
    {
        static void Main(string[] args)
        {
            string projectDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string filePath = Path.Combine(projectDir, "doc", "input.txt"); // switch to test_input.txt to try the example
            string[] lines = File.ReadAllLines(filePath);

            Day10 day10 = new Day10();
            int part1 = day10.Part1(lines);
            int part2 = Part2Solver.SumAll(lines);
            Console.WriteLine($"Part 1 (lights): fewest total presses = {part1}");
            Console.WriteLine($"Part 2 (counters): fewest total presses = {part2}");
        }

        // =================================================================
        // PART 1 — light toggles  (bitmask + BFS over the 2^n states)
        // =================================================================
        int Part1(string[] lines)
        {
            int total = 0;
            foreach (string line in lines)
                total += MinPressesForMachine(line);
            return total;
        }

        static int MinPressesForMachine(string line)
        {
            // 1) Parse the target diagram "[.##.]" into a bitmask.
            string targetText = Regex.Match(line, @"\[([.#]+)\]").Groups[1].Value;
            int numLights = targetText.Length;
            int targetMask = 0;
            for (int i = 0; i < numLights; i++)
                if (targetText[i] == '#')
                    targetMask |= 1 << i;

            // 2) Parse every button "(1,3)" into its own bitmask.
            var buttons = new List<int>();
            foreach (Match m in Regex.Matches(line, @"\(([^)]*)\)"))
                buttons.Add(ParseMask(m.Groups[1].Value));

            // 3) BFS: fewest presses to reach targetMask from "all off".
            return BfsMinPresses(targetMask, buttons.ToArray(), numLights);
        }

        static int ParseMask(string text)
        {
            int mask = 0;
            foreach (string part in text.Split(','))
                mask |= 1 << int.Parse(part.Trim());
            return mask;
        }

        static int BfsMinPresses(int targetMask, int[] buttons, int numLights)
        {
            int stateCount = 1 << numLights;
            int[] dist = new int[stateCount];
            for (int i = 0; i < stateCount; i++) dist[i] = -1;

            dist[0] = 0;
            var queue = new Queue<int>();
            queue.Enqueue(0);

            while (queue.Count > 0)
            {
                int state = queue.Dequeue();
                if (state == targetMask) return dist[state];

                foreach (int b in buttons)
                {
                    int next = state ^ b;          // XOR toggles the button's bits
                    if (dist[next] == -1)
                    {
                        dist[next] = dist[state] + 1;
                        queue.Enqueue(next);
                    }
                }
            }
            return -1;
        }
    }

#nullable disable   // (the Part-2 ILP solver below is written in pre-nullable style)

    // =================================================================
    // PART 2 — joltage counters.
    //
    // This is NOT solvable by a state search any more. Counters start at 0
    // and every press of button j ADDS 1 to each counter in its set B_j.
    // If x_j = how many times we press button j, then we need, for EVERY
    // counter c:
    //        sum_{j : c in B_j} x_j  =  t_c          (t_c = its target)
    // and we want to MINIMIZE  sum_j x_j.
    //
    // That is a small INTEGER LINEAR PROGRAM (ILP):
    //   n <= 13 integer variables x_j,  k <= 10 equality constraints.
    // Real-world ILPs are solved by branch & bound, which is what we do:
    //   1. Solve the LP relaxation  (x_j allowed to be REAL) — this gives a
    //      LOWER BOUND: no integer answer can beat it. If the LP solution
    //      is already all-integer, it IS the optimum.
    //   2. Otherwise pick a variable whose LP value is fractional (say 7.3)
    //      and SPLIT into two sub-problems:  x_j <= 7  or  x_j >= 8.
    //      No integer solution is lost by this split.
    //   3. Recurse. Prune any sub-problem whose lower bound can't beat the
    //      best integer answer already found.
    //
    // The LP relaxation is solved exactly by enumerating "basic solutions"
    // (there are only ~C(13,10) of them per machine).
    // =================================================================
    static class Part2Solver
    {
        public static int SumAll(string[] lines)
        {
            int total = 0;
            foreach (string line in lines)
            {
                int[] target = ParseTargets(line);
                int[][] buttons = ParseButtons(line);
                total += SolveMachine(target, buttons);
            }
            return total;
        }

        static int[] ParseTargets(string line)
        {
            Match m = Regex.Match(line, @"\{([^}]*)\}");
            string[] parts = m.Groups[1].Value.Split(',');
            int[] t = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) t[i] = int.Parse(parts[i]);
            return t;
        }

        static int[][] ParseButtons(string line)
        {
            var list = new List<int[]>();
            foreach (Match m in Regex.Matches(line, @"\(([^)]*)\)"))
            {
                string p = m.Groups[1].Value;
                if (p.Length == 0) continue;
                string[] parts = p.Split(',');
                int[] b = new int[parts.Length];
                for (int i = 0; i < parts.Length; i++) b[i] = int.Parse(parts[i]);
                list.Add(b);
            }
            return list.ToArray();
        }

        // ------------------- the search object -------------------
        class Search
        {
            int k;             // number of counters
            int[][] B;         // buttons (counter indices)
            int n;             // number of buttons
            int best;          // best integer answer found (incumbent)

            public static int Run(int[] target, int[][] buttons)
            {
                var s = new Search();
                s.k = target.Length;
                s.B = buttons;
                s.n = buttons.Length;
                s.best = int.MaxValue;
                int[] cap = new int[s.n];
                for (int j = 0; j < s.n; j++) cap[j] = int.MaxValue; // unlimited presses
                s.Bb((int[])target.Clone(), 0, cap);
                return s.best;
            }

            void Update(int v) { if (v < best) best = v; }

            static bool Zero(int[] r) { foreach (var v in r) if (v != 0) return false; return true; }

            // smallest remaining demand over a button's counters
            static int MinR(int[] b, int[] r) { int m = int.MaxValue; foreach (var c in b) m = Math.Min(m, r[c]); return m; }

            // buttons we may still press: cap > 0 and every counter it touches still has demand
            List<int> Usable(int[] r, int[] cap)
            {
                var u = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    if (cap[j] <= 0) continue;
                    bool ok = true;
                    foreach (var c in B[j]) if (r[c] <= 0) { ok = false; break; }
                    if (ok) u.Add(j);
                }
                return u;
            }

            // ---------------------------------------------------------
            //  Exact LP relaxation  min sum dy  s.t. A dy = r, dy >= 0,
            //  over the usable buttons. Returns the optimal VALUE and the
            //  optimal dy (aligned with `usable`), or +inf if infeasible.
            //  Solved by: reduce the counter rows (Gaussian) to an
            //  independent set, then enumerate every basic solution
            //  (a square subset of buttons).
            // ---------------------------------------------------------
            double Lp(int[] r, List<int> usable, int[] cap, out double[] opt)
            {
                opt = null;
                int nn = usable.Count;
                int kk = 0;
                for (int c = 0; c < k; c++) if (r[c] > 0) kk++;
                if (kk == 0) { opt = new double[nn]; return 0; }
                if (nn == 0) return double.PositiveInfinity;

                // augmented matrix: one row per active counter
                double[,] mat = new double[kk, nn + 1];
                int row = 0;
                for (int c = 0; c < k; c++)
                {
                    if (r[c] <= 0) continue;
                    for (int a = 0; a < nn; a++)
                    {
                        bool has = false;
                        foreach (var cc in B[usable[a]]) if (cc == c) { has = true; break; }
                        mat[row, a] = has ? 1.0 : 0.0;
                    }
                    mat[row, nn] = r[c];
                    row++;
                }

                // Gauss-Jordan to find an independent, consistent set of rows
                int rank = 0;
                for (int col = 0; col < nn; col++)
                {
                    int piv = -1;
                    for (int i = rank; i < kk; i++) if (Math.Abs(mat[i, col]) > 1e-10) { piv = i; break; }
                    if (piv < 0) continue;
                    for (int j = 0; j <= nn; j++) { double t = mat[rank, j]; mat[rank, j] = mat[piv, j]; mat[piv, j] = t; }
                    double pv = mat[rank, col];
                    for (int j = col; j <= nn; j++) mat[rank, j] /= pv;
                    for (int i = 0; i < kk; i++)
                    {
                        if (i == rank || Math.Abs(mat[i, col]) < 1e-12) continue;
                        double f = mat[i, col];
                        for (int j = col; j <= nn; j++) mat[i, j] -= f * mat[rank, j];
                    }
                    rank++;
                }
                // leftover rows must be 0 = 0 (else the demands contradict each other)
                for (int i = rank; i < kk; i++)
                {
                    bool allZero = true;
                    for (int j = 0; j < nn; j++) if (Math.Abs(mat[i, j]) > 1e-9) { allZero = false; break; }
                    if (allZero && Math.Abs(mat[i, nn]) > 1e-7) return double.PositiveInfinity;
                }
                if (rank == 0) { opt = new double[nn]; return 0; }

                // reduced coefficient matrix over the `rank` independent rows
                double[] coef = new double[rank * nn];
                double[] rhs = new double[rank];
                for (int i = 0; i < rank; i++)
                {
                    for (int a = 0; a < nn; a++) coef[i * nn + a] = mat[i, a];
                    rhs[i] = mat[i, nn];
                }
                // effective per-button cap = min(user cap, residual min over its support)
                int[] ecap = new int[nn];
                bool capsBind = false;
                for (int a = 0; a < nn; a++)
                {
                    ecap[a] = Math.Min(cap[usable[a]], MinR(B[usable[a]], r));
                    if (ecap[a] < MinR(B[usable[a]], r)) capsBind = true;
                }

                double bestV = double.PositiveInfinity;
                double[] bestD = null;
                int[] bas = new int[rank];
                Enumerate(0, 0, nn, rank, bas, coef, rhs, ecap, capsBind, ref bestV, ref bestD);
                if (double.IsPositiveInfinity(bestV)) return double.PositiveInfinity;
                opt = bestD;
                return bestV;
            }

            // choose `rank` basic buttons among nn; the rest are 0 (or at their cap)
            void Enumerate(int start, int depth, int nn, int rank, int[] bas, double[] col, double[] rhs,
                           int[] ecap, bool capsBind, ref double bestV, ref double[] bestD)
            {
                if (depth == rank)
                {
                    bool[] isB = new bool[nn];
                    foreach (var b in bas) isB[b] = true;
                    var rest = new List<int>();
                    for (int a = 0; a < nn; a++) if (!isB[a]) rest.Add(a);
                    int nr = rest.Count;
                    int combos = capsBind ? (1 << nr) : 1;
                    for (int mask = 0; mask < combos; mask++)
                    {
                        double[] rr = (double[])rhs.Clone();
                        if (capsBind)
                            for (int a = 0; a < nr; a++)
                                if ((mask >> a & 1) == 1)
                                {
                                    int aa = rest[a];
                                    for (int i = 0; i < rank; i++) rr[i] -= ecap[aa] * col[i * nn + aa];
                                }
                        // solve:  sum_t col[i*nn + bas[t]] * x_t = rr[i]   (rank x rank)
                        double[,] M = new double[rank, rank + 1];
                        for (int i = 0; i < rank; i++)
                        {
                            for (int t = 0; t < rank; t++) M[i, t] = col[i * nn + bas[t]];
                            M[i, rank] = rr[i];
                        }
                        bool dep = false;
                        for (int cx = 0; cx < rank; cx++)
                        {
                            int piv = cx;
                            for (int r2 = cx + 1; r2 < rank; r2++) if (Math.Abs(M[r2, cx]) > Math.Abs(M[piv, cx])) piv = r2;
                            if (Math.Abs(M[piv, cx]) < 1e-10) { dep = true; break; }
                            for (int j = 0; j <= rank; j++) { double t = M[cx, j]; M[cx, j] = M[piv, j]; M[piv, j] = t; }
                            double pv = M[cx, cx];
                            for (int j = cx; j <= rank; j++) M[cx, j] /= pv;
                            for (int r2 = 0; r2 < rank; r2++)
                            {
                                if (r2 == cx || Math.Abs(M[r2, cx]) < 1e-12) continue;
                                double f = M[r2, cx];
                                for (int j = cx; j <= rank; j++) M[r2, j] -= f * M[cx, j];
                            }
                        }
                        if (dep) continue;
                        double[] xb = new double[rank];
                        for (int i = 0; i < rank; i++) xb[i] = M[i, rank];
                        bool bad = false;
                        for (int i = 0; i < rank; i++)
                            if (xb[i] < -1e-7 || (capsBind && xb[i] > ecap[bas[i]] + 1e-7)) { bad = true; break; }
                        if (bad) continue;
                        double obj = 0;
                        for (int i = 0; i < rank; i++) obj += xb[i];
                        if (capsBind) for (int a = 0; a < nr; a++) if ((mask >> a & 1) == 1) obj += ecap[rest[a]];
                        if (obj < bestV - 1e-8)
                        {
                            bestV = obj;
                            bestD = new double[nn];
                            for (int i = 0; i < rank; i++) bestD[bas[i]] = Math.Max(0, xb[i]);
                            if (capsBind) for (int a = 0; a < nr; a++) if ((mask >> a & 1) == 1) bestD[rest[a]] = ecap[rest[a]];
                        }
                    }
                    return;
                }
                for (int a = start; a < nn; a++) { bas[depth] = a; Enumerate(a + 1, depth + 1, nn, rank, bas, col, rhs, ecap, capsBind, ref bestV, ref bestD); }
            }

            // ---------------------------------------------------------
            //  branch & bound over integer x_j
            // ---------------------------------------------------------
            void Bb(int[] r, int p, int[] cap)
            {
                if (p >= best) return;
                if (Zero(r)) { Update(p); return; }

                // ---- propagation: a counter touched by only one remaining button
                // forces that button's presses to equal the counter's demand
                var usable = Usable(r, cap);
                bool changed = true;
                while (changed)
                {
                    changed = false;
                    for (int c = 0; c < k; c++)
                    {
                        if (r[c] <= 0) continue;
                        int who = -1, cnt = 0;
                        foreach (var j in usable)
                            if (Array.IndexOf(B[j], c) >= 0) { cnt++; who = j; }
                        if (cnt == 0) return;                 // unsatisfiable
                        if (cnt == 1)
                        {
                            int amt = r[c];                   // capture BEFORE mutating r[c]!
                            if (cap[who] < amt) return;
                            bool ok = true;
                            foreach (var cc in B[who]) if (r[cc] < amt) ok = false;
                            if (!ok) return;
                            foreach (var cc in B[who]) r[cc] -= amt;
                            cap[who] -= amt;
                            p += amt;
                            changed = true;
                            break;
                        }
                    }
                    if (changed) usable = Usable(r, cap);
                }
                if (Zero(r)) { Update(p); return; }
                usable = Usable(r, cap);
                if (usable.Count == 0) return;

                // ---- lower bound from the LP relaxation
                double[] opt;
                double V = Lp(r, usable, cap, out opt);
                if (double.IsPositiveInfinity(V)) return;
                if (p + Math.Ceiling(V - 1e-8) >= best) return;   // prune

                // all-integer LP solution => an incumbent for this whole sub-tree
                bool integral = true;
                foreach (var v in opt)
                    if (v > 1e-6 && Math.Abs(v - Math.Round(v)) > 1e-6) { integral = false; break; }
                if (integral)
                {
                    int cand = p;
                    foreach (var v in opt) cand += (int)Math.Round(v);
                    Update(cand);
                    return;
                }

                // ---- branch on the most-fractional LP variable
                int bj = usable[0];
                double bval = 0, bestFrac = -1;
                for (int a = 0; a < usable.Count; a++)
                {
                    double v = opt[a];
                    double fr = v - Math.Floor(v);
                    if (fr > 1e-6 && fr < 1 - 1e-6)
                    {
                        double near = Math.Abs(fr - 0.5);
                        if (near > bestFrac) { bestFrac = near; bj = usable[a]; bval = v; }
                    }
                }
                int fl = (int)Math.Floor(bval + 1e-9);
                int ce = (int)Math.Ceiling(bval - 1e-9);

                // left:  x_bj <= floor(...)
                var capL = (int[])cap.Clone();
                capL[bj] = Math.Min(capL[bj], fl);
                Bb((int[])r.Clone(), p, capL);

                // right: x_bj >= ceil(...)  => commit those presses now
                var rR = (int[])r.Clone();
                var capR = (int[])cap.Clone();
                bool safe = true;
                foreach (var c in B[bj]) if (rR[c] < ce) { safe = false; break; }
                if (safe)
                {
                    foreach (var c in B[bj]) rR[c] -= ce;
                    capR[bj] -= ce;
                    Bb(rR, p + ce, capR);
                }
            }
        }

        // ------------------- public entry per machine -------------------
        public static int SolveMachine(int[] target, int[][] buttons)
        {
            return Search.Run(target, buttons);
        }
    }
}

#nullable restore
