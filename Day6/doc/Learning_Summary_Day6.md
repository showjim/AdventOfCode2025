# Advent of Code 2025 Day 6: C# Learning Summary

## Problem Overview

**Part 1:** The input is a cephalopod "math worksheet" laid out as a very wide grid of characters. Each problem is a vertical *column* of numbers with an operator (`+` or `*`) printed at the bottom; neighboring problems are separated by a column consisting only of spaces. The task is to read every problem, apply its operator to its numbers (all added, or all multiplied), and return the grand total of all the answers summed together.

**Part 2:** The big cephalopods reveal that their math is written **right-to-left**, and crucially that each single character-column is one number — most-significant digit on top, least-significant on the bottom. A "problem" is now a *group* of adjacent columns (still separated by all-space columns), with the operator at the bottom. The same worksheet now yields entirely different numbers and a different grand total.

## C# Concepts Learned

### 1. Index-from-end — the `[^1]` operator

The operator row is always the last row of the grid. C#'s index-from-end operator reads it cleanly, exactly like Python's `list[-1]`:

```csharp
List<string> operatorRow = input[^1];   // last row
List<string> rowAbove    = input[^2];   // second-to-last
// classic form: input[input.Count - 1]
```

### 2. `string.Split` with `StringSplitOptions.RemoveEmptyEntries`

The worksheet rows contain runs of *varying* numbers of spaces between problems. Splitting on a single space leaves empty strings in the gaps, so the empty-removal option is essential:

```csharp
// "a  b".Split(' ')                       -> ["a", "", "b"]          // WRONG: gap becomes ""
// "a  b".Split(' ', RemoveEmptyEntries)   -> ["a", "b"]             // CORRECT
var tokens = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
```

*Contrast with Python:* Python's `str.split()` with **no argument** already collapses runs of whitespace and drops empties. C# is stricter — you must opt in with `RemoveEmptyEntries` (or use `Regex.Split(line, @"\s+")`).

### 3. LINQ `Aggregate` — reduce/fold with a seed

Each problem applies one operator to a whole column of numbers. `Aggregate` is C#'s `functools.reduce` — it carries a running value through a sequence:

```csharp
// multiply every number in the column:
long product = numbers.Aggregate(1L, (acc, n) => acc * n);
// add every number — just use the dedicated shortcut:
long sum = numbers.Sum();
```

The **seed** is the trap: a product must start at `1` (starting at `0` makes every product `0`), while a sum starts at `0`. For addition, `.Sum()` is cleaner and sidesteps the seed question entirely.

### 4. `IEnumerable<T>` and lazy LINQ

Every LINQ operator (`Select`, `Where`, `SkipLast`, …) returns an `IEnumerable<T>` — a *lazy* sequence evaluated only when consumed, not a `List`. You can read the element type right through the chain:

| Expression                             | Type                          |
| -------------------------------------- | ----------------------------- |
| `input` (after splitting)            | `List<List<string>>`        |
| `.SkipLast(1)`                       | `IEnumerable<List<string>>` |
| `.Select(row => long.Parse(row[i]))` | `IEnumerable<long>`         |

Because `.Sum()` and `.Aggregate()` are methods *on* `IEnumerable<T>`, you usually don't need `.ToList()` at all:

```csharp
IEnumerable<long> col = input.SkipLast(1).Select(row => long.Parse(row[i]));
sum += op == "+" ? col.Sum() : col.Aggregate(1L, (a, b) => a * b);   // no .ToList() needed
```

### 5. `Enumerable.Range` + `.All()` — "does every element satisfy this?"

Part 2 needs to find separator columns: a column that is entirely spaces, top to bottom. `Enumerable.Range` generates the row indices, and `.All()` checks that every one of them is a space:

```csharp
bool[] isSeparator = Enumerable.Range(0, cols)
    .Select(c => Enumerable.Range(0, rows).All(r => grid[r][c] == ' '))
    .ToArray();
```

Read the inner expression as *"for column `c`, are all rows spaces?"* → one `bool`. The outer `Select` produces that bool for every column.

### 6. 2D character grids — `ToList()`, `new string()`, `PadRight`

Part 2 works one character at a time, so the input becomes a grid of `char`. A `string` is already an `IEnumerable<char>`, so `.ToList()` turns each line into a `List<char>`:

```csharp
List<List<char>> grid = input.Select(l => l.ToList()).ToList();
char ch       = grid[r][c];                        // index like a 2-D array
string digits = new string(chars.ToArray());       // rebuild a string from chars
string padded = line.PadRight(width);              // pad with spaces to uniform width
```

`new string(charArray)` is the inverse of a string's `.ToArray()` / `.ToList()`. `PadRight` guarantees every row is the same width so `grid[r][c]` never throws on a short trailing line.

### 7. `.Skip().Take().First()` — querying a sub-range

The operator for a region lives somewhere in its bottom row. `Skip` + `Take` carve out the region's slice of columns, then `First` finds the operator character:

```csharp
char op = grid[rows - 1]
    .Skip(start)
    .Take(end - start + 1)
    .First(ch => ch == '+' || ch == '*');
```

### 8. `long` vs `int` vs `BigInteger` — mind the overflow

Part 2 multiplies many numbers per region, and the products can grow fast. `int` overflows past ~2.1 billion with no error — just a silently wrong (often negative) value.

| Type           | Max value      | When to use                       |
| -------------- | -------------- | --------------------------------- |
| `int`        | ~2.1 × 10⁹   | Small counts, indices             |
| `long`       | ~9.2 × 10¹⁸ | Large totals, most puzzle answers |
| `BigInteger` | unlimited      | Products of*many* large numbers |

Part 2's widest `*` regions could in principle exceed even `long` — the safety net is `System.Numerics.BigInteger`, which has no size limit and supports `+` / `*` directly (use `.Aggregate(BigInteger.One, (a, b) => a * b)`, since LINQ has no built-in `.Sum()` for it). On this input `long` was enough — but that was a deliberate, checked choice, not luck.

### 9. Two representations for two problems

Part 1 and Part 2 needed *different* data shapes. Part 1 worked from rows split into whitespace tokens — clean for "one number per problem per row," but that tokenizing **threw away the column geometry** Part 2 needed. Part 2 therefore re-parsed the raw lines into a character grid and read vertically. The lesson: pick the representation that fits the question; the convenient shape for one part may be useless for the next.

## Key Takeaways

1. **Don't hardcode to the test input's shape.** Part 1 passed the example but reported "too low" on the real input because it read exactly 3 number rows — and the real input had 4. "Passes the test, wrong on the real data" almost always means a hardcoded assumption. Loop over `0 .. rows-2` instead of indexing `[0]`, `[1]`, `[2]`.
2. **Match the data representation to the question.** When the problem is about columns of individual characters, reach for the raw character grid — tokenizing loses exactly the information you need.
3. **Anticipate overflow.** Use `long` (or `BigInteger`) whenever you multiply or sum many numbers. `int` overflows silently, producing a wrong answer with no exception.
4. **Read the type of a LINQ chain.** Most LINQ operators return lazy `IEnumerable<T>`, so `.Sum()` / `.Aggregate()` consume them directly — `.ToList()` is usually unnecessary. Tracing the element type through `SkipLast` → `Select` makes this obvious.
5. **Map new C# features to Python to learn them fast:** `list[-1]` → `[^1]`, `functools.reduce` → `Aggregate`, `str.split()` → `Split` + `RemoveEmptyEntries`, a list-of-lists grid → `List<List<char>>`.
