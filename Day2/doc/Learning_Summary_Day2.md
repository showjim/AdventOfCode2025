# Advent of Code 2025 Day 2: C# Learning Summary

## Problem Overview

**Part 1:** Parse comma-separated ID ranges (e.g., `11-22,95-115`). An ID is invalid if its digits form a sequence repeated **exactly twice** (e.g., `55`, `6464`, `123123`). Sum all invalid IDs within the given ranges.

**Part 2:** An ID is now invalid if its digits form a sequence repeated **at least twice** (e.g., `111` = 1×3, `123123123` = 123×3, `1212121212` = 12×5). Sum all invalid IDs under these new rules.

## C# Concepts Learned

### 1. `string.Split()` for Simple Parsing

The puzzle input is a single line of comma-separated ranges. `Split` handles the delimiters:

```csharp
string[] ranges = rawInput[0].Split(',');        // ["11-22", "95-115", ...]
string[] bounds = "11-22".Split('-');              // ["11", "22"]
```

Unlike Python, `Split` in C# returns `string[]`, not a list — but you can chain `.ToList()` if needed.

### 2. Linq `Select` with Nested `Select` for Parsing Chains

Combined `Split` with `Select` and `long.Parse` to parse everything in one expression:

```csharp
List<List<long>> input = rawInput[0]
    .Split(',')
    .Select(x => x.Split('-').Select(long.Parse).ToList())
    .ToList();
```

The **inner** `Select` converts each bound string to `long`, and the **outer** `Select` processes each range. This is a common C# pattern: compose transformations with Linq rather than writing explicit loops.

### 3. `long` vs `int`: Overflow Awareness

| Type | C# Alias | Range |
|---|---|---|
| 32-bit signed | `int` | –2,147,483,648 to 2,147,483,647 |
| 64-bit signed | `long` | –9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 |

The real puzzle input contained numbers like `2121212118` (~2.1 billion) which overflow `int.Parse()`. AoC puzzles often push past 32-bit limits — always scan the input for large numbers before choosing `int` vs `long`.

**Error you'll see:** `System.OverflowException: Value was either too large or too small for an Int32.`

### 4. `string.Substring(startIndex, length)` for Slicing Strings

Unlike C#'s array slice syntax, string slicing uses `Substring`:

```csharp
string firstHalf  = idStr.Substring(0, idStr.Length / 2);       // start, length
string secondHalf = idStr.Substring(idStr.Length / 2);          // start (to end)
```

The two-argument form gives a specific number of characters; the one-argument form gives everything from start to end.

### 5. `Enumerable.Repeat()` + `string.Concat()` for Building Repeated Strings

To check if a string is a repeated pattern, build the expected result and compare:

```csharp
string repeatPart = idStr.Substring(0, 3);                      // "123"
string repeated   = string.Concat(Enumerable.Repeat(repeatPart, 3)); // "123123123"
return idStr == repeated;
```

`Enumerable.Repeat(element, count)` generates `count` copies of `element`. `string.Concat()` joins them without a separator. This avoids manual loop-and-compare logic.

### 6. Divisor-Based Loop for Pattern Checking

For Part 2, instead of only checking a 50/50 split, the algorithm checks **every possible divisor** of the string length:

```csharp
for (int i = 1; i <= idStr.Length / 2; i++)
{
    if (idStr.Length % i != 0) continue;  // Skip non-divisors
    // i = pattern length, idStr.Length / i = number of repeats
}
```

The bound `idStr.Length / 2` is the optimization: a repeat pattern can never be longer than half the string (you need at least 2 repeats). A pattern length of `idStr.Length` would mean 1 repeat, which isn't "at least twice."

### 7. DRY: Shared Iteration Pattern

Both `Part1` and `Part2` share the same structure: iterate ranges, loop through IDs, sum invalid ones. The only difference is which validation method (`IsInvalidProductID` vs `IsInvalidProductIDPart2`). This duplication can be eliminated by passing the validation function as a parameter (delegate/`Func`) or using a shared helper.

### 8. `.Append()` Creates a New Array

```csharp
array = array.Append(item).ToArray();  // Creates a NEW array each call!
```

`Append()` does NOT modify the original array — it returns a new `IEnumerable` with the item added. Combining it with `.ToArray()` in a loop reallocates the entire array on every iteration. This is O(n²) memory allocation. For collecting values in a loop, use `List<T>.Add()` instead:

```csharp
List<long> ids = new List<long>();
ids.Add(i);  // Efficient append — resizes occasionally, not every time
```

## Key Takeaways

1. **Test with small input first.** The example gave `1227775554` for Part 1 and `4174379265` for Part 2 — confirm these before running the real puzzle.
2. **Scan for overflow.** When puzzle numbers look large (10 digits+), use `long` immediately.
3. **String comparisons are case-sensitive.** `firstHalf == secondHalf` works for digit strings because they're all the same case.
4. **`%` is modulo, not remainder.** In C#, `-5 % 3` = `-2` (truncates toward zero). Today didn't need it, but remember Day 1.
5. **Linq chains are your friend.** Parsing complex input formats with nested `Select` is idiomatic C# — concise and readable once you're used to it.
