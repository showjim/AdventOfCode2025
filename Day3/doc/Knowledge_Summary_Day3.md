# Day 3 — C# Knowledge Summary

## Problem Recap

Given banks of batteries (one per line), pick digits in left-to-right order to form the largest possible number. Part 1: pick 2 digits per bank. Part 2: pick 12 digits per bank. Sum the results.

---

## Concepts Learned

### 1. `List<T>` Capacity vs Count

```csharp
// WRONG assumption: this creates a list with 4 elements
List<int> list = new List<int>(4);
Console.WriteLine(list.Count);  // 0! The list is empty.

// CORRECT: the parameter sets capacity (internal array size), not element count.
// To add elements, use Add():
list.Add(10);  // Count becomes 1

// Or pre-fill with placeholders if you need index-based assignment:
List<int> list = new List<int>(new int[4]);  // Count = 4, all zeros
```

**Why it matters:** `list[i] = x` works only if index `i` already exists. `Add(x)` always works and is simpler.

### 2. Right-to-Left Scanning (O(n) for max pair)

For Part 1 ("find max `digit[i]*10 + digit[j]` where `i < j`"), scanning right-to-left tracks the maximum digit seen so far, avoiding a nested loop:

```csharp
int maxSoFar = -1, best = -1;
for (int j = list.Count - 1; j >= 0; j--)
{
    if (j < list.Count - 1)
        best = Math.Max(best, list[j] * 10 + maxSoFar);
    maxSoFar = Math.Max(maxSoFar, list[j]);
}
```

**Why it works:** When you're at position `j`, you've already seen everything to the right, so `maxSoFar` is the best possible ones-digit.

### 3. Greedy Algorithm: Pick K Digits to Form Max Number

For Part 2, a classic greedy approach: at each step, pick the largest digit from the **valid range** (the first `dropsAllowed + 1` positions), ensuring enough digits remain:

```csharp
int dropsAllowed = totalLength - startIndex - remainingToPick;
// Search range: [startIndex, startIndex + dropsAllowed]
```

**Key insight:** The leftmost digit has the most weight, so be greedy from left to right — always pick the max available, skipping at most as many as you can afford to drop.

### 4. Declare Variables Close to Usage

```csharp
// Avoid: variable declared far from where it's used
int curJoltage = -1;
// ... many lines later ...
curJoltage = something;

// Better: declare at point of use
int curJoltage = input[i][j] * 10 + maxSoFar;
```

This improves readability and prevents accidentally reusing stale values.

### 5. `Math.Pow` Returns `double` — Beware Precision Loss

```csharp
// Risky: Math.Pow returns double, precision loss for large integers
long result = digit * (long)Math.Pow(10, 11);

// Safe: pure integer arithmetic
result = result * 10 + digit;  // Build number digit by digit
```

`double` has ~15-16 significant digits. For 12-digit integers it happened to work, but using integer math is always correct.

### 6. Avoid Unnecessary Allocations

```csharp
// Creates a new List<int> every iteration — allocates memory
List<int> searchRange = curRow.Skip(startIndex).Take(n).ToList();
int max = searchRange.Max();
int idx = searchRange.IndexOf(max) + startIndex;

// Better: iterate directly over the original list
int max = -1, idx = -1;
for (int k = startIndex; k <= startIndex + n; k++)
{
    if (curRow[k] > max) { max = curRow[k]; idx = k; }
}
```

LINQ is convenient but creates temporary objects. For performance-sensitive code, a simple loop is both faster and more memory-efficient.

### 7. Early Exit Optimization

When you know the absolute maximum (`99` for 2-digit numbers with digits 1-9), you can `break` early once you find it. This is a valid micro-optimization.

---

## C# Methods/Types Used

| Item | What it does |
|------|-------------|
| `List<T>.Count` | Number of elements actually in the list |
| `List<T>.Capacity` | Size of internal array (not usually needed) |
| `List<T>.Add(x)` | Adds an element to the end |
| `Math.Max(a, b)` | Returns the larger of two values |
| `Math.Pow(x, y)` | Returns x^y as `double` — use with caution |
| `long` | 64-bit signed integer, use for numbers > 2 billion |
| `int.Parse(s)` | Converts string to int |
| `String.Join(sep, list)` | Joins list elements into a string |
