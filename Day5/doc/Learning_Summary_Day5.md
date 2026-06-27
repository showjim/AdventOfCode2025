# Advent of Code 2025 Day 5: C# Learning Summary

## Problem Overview

**Part 1:** The puzzle input is a database of ingredient IDs. It has two sections split by a blank line: first a list of *fresh* ID ranges (e.g. `3-5`, inclusive and allowed to overlap), then a list of individual *available* IDs. The task is to count how many of the available IDs fall inside at least one fresh range. An ID sitting exactly on a range boundary counts as fresh.

**Part 2:** Now the available IDs are irrelevant. Instead, count the **total number of distinct IDs** that the fresh ranges consider fresh — in other words, the size of the union of all the ranges. The ranges overlap heavily, so naive summing would double-count the overlapping regions.

## C# Concepts Learned

### 1. `long` vs `int` — mind the integer range

The largest value in the input was **562,824,901,924,389** (562 trillion), which blows past `int.MaxValue` (~2.1 billion). Using `int` here would have caused a silent overflow and a wrong answer, so `long` was not a defensive choice — it was **required**.

| Type | Max value | Use when |
|------|-----------|----------|
| `int` | ~2.1 × 10⁹ | Counts, small/medium IDs |
| `long` | ~9.2 × 10¹⁸ | Large IDs, big totals |

*Contrast with Python:* Python integers have arbitrary precision and grow as needed, so this kind of overflow simply doesn't happen there. In C#, the fixed-width types mean you must anticipate the range up front.

### 2. Parsing delimited strings — `Split` + `long.Parse`

Each range line like `"3-5"` needs to become two numbers. `Split` breaks it on a delimiter, and `long.Parse` converts each piece:

```csharp
string[] parts = line.Split('-');      // ["3", "5"]
long min = long.Parse(parts[0]);
long max = long.Parse(parts[1]);
```

A subtle point: `long.Parse` takes a **string**, not a `char`. After `Split`, each piece is already a string — so there is no need to break a line into individual characters first (the earlier `List<char>` parsing from grid-based puzzles didn't fit this problem).

### 3. LINQ for slicing and transforming collections

LINQ turned the two-section split into a few readable lines. First find the blank line, then slice:

```csharp
int i = Array.IndexOf(rawInput, "");          // index of the empty-string line
var ranges = rawInput.Take(i)                 // everything before the blank line
    .Select(line => ( /* parse into (min, max) */ ))
    .ToList();
var ids = rawInput.Skip(i + 1)                // everything after the blank line
    .Select(line => long.Parse(line))
    .ToList();
```

Key methods used: `Take(n)` (first n), `Skip(n)` (drop first n, keep the rest), `Select(...)` (transform each element), `OrderBy(...)` (sort), and `ToList()` (materialize the result).

### 4. Named tuples for readable pairs

A range is naturally a pair `(min, max)`. C# named tuples make this both compact and self-documenting:

```csharp
List<(long min, long max)> ranges = ...;
// access later as ranges[0].min, ranges[0].max — no magic indexing
```

This reads far more clearly than a 2-D `List<List<int>>` where you'd have to remember that index `0` means `min`.

### 5. Detecting input structure — the blank-line separator

`File.ReadAllLines` reads every line into a string array, including the blank one. A blank line becomes `""`, a string of **length 0** (not `null`, not a space). Spotting that with a quick debug print was what made the two-section split possible.

### 6. Boundary conditions — `>` vs `>=`

Ranges are **inclusive**: `3-5` means 3, 4, *and* 5 are fresh. The membership check must therefore use inclusive comparisons:

```csharp
if (value >= min && value <= max) { /* fresh */ }
```

Writing `value > min && value < max` instead would silently drop IDs sitting exactly on a boundary (like 5 in range 3-5). This is the classic off-by-one trap, and in C# it produces no error — just a quietly wrong count.

### 7. The Merge Intervals algorithm

Part 2 needed the size of the *union* of overlapping ranges. The reusable approach is **merge intervals**:

1. **Sort** ranges by `min`.
2. **Scan**, keeping one current interval `(curMin, curMax)`. For each range: if `next.min <= curMax` it overlaps → extend (`curMax = Math.Max(curMax, next.max)`); otherwise the current interval is finished → add its length `(curMax - curMin + 1)` to the total and start fresh.
3. **Close out the last interval** after the loop — easy to forget, and the #1 place to lose a range.

```csharp
var sorted = freshIDRanges.OrderBy(r => r.min).ToList();
long curMin = sorted[0].min, curMax = sorted[0].max, total = 0;
for (int k = 1; k < sorted.Count; k++)
{
    if (sorted[k].min <= curMax)
        curMax = Math.Max(curMax, sorted[k].max);   // overlap → extend
    else
    {
        total += curMax - curMin + 1;               // close current
        curMin = sorted[k].min; curMax = sorted[k].max;
    }
}
total += curMax - curMin + 1;                       // last interval
```

### 8. Complexity thinking — don't enumerate the impossible

The instinct for Part 2 might be "list every fresh ID and count them." But a *single* range in the input spanned about **7 trillion** IDs. Enumerating them would need terabytes of memory and run for years. The fix was to stop thinking about individual IDs and start doing **arithmetic on the intervals themselves** (lengths and overlaps). Merge intervals runs in O(n log n) — dominated by the sort — regardless of how huge the ranges are.

## Key Takeaways

1. **Test with the small example first.** Running on the test input confirmed the expected answers (3 fresh IDs for Part 1, 14 total fresh IDs for Part 2) before touching the real data. This caught nothing today, but it's the habit that catches the silent bugs.
2. **Watch boundary conditions.** Inclusive ranges demand `>=` and `<=`. A strict inequality produces no compiler error — just a wrong count that's hard to notice.
3. **Pick the data type up front.** When values can exceed ~2.1 billion, reach for `long`. A moment of foresight beats hours of debugging an overflow.
4. **When a problem smells like ranges, think "merge intervals," not "enumerate."** Sorting and merging disjoint intervals is simpler, faster, and avoids the combinatorial mess of subtracting overlaps.
5. **Estimate before you code.** Subtracting the endpoints of one range revealed ~7 trillion IDs — instantly ruling out brute force and pointing straight at the interval-arithmetic solution.
