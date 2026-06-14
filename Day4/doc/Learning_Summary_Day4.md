# Advent of Code 2025 Day 4: C# Learning Summary

## Problem Overview

**Part 1:** Given a 2D grid of `.` (empty) and `@` (paper roll), count how many `@` have fewer than 4 `@` neighbors in their 8 adjacent positions. A roll is "accessible by a forklift" if it has 0-3 neighboring rolls.

**Part 2:** After removing all accessible rolls (replacing them with `.`), some previously blocked rolls may now be accessible. Repeat the process — count and remove accessible rolls each pass — until no more can be removed. Return the **total** number of rolls removed across all passes.

## C# Concepts Learned

### 1. 2D Grid Neighbor Checking with Bounds Handling

Use paired direction arrays (`dRow`/`dCol`) to iterate all 8 neighbors, and always bounds-check both dimensions before accessing:

```csharp
int[] dRow = { -1, -1, -1, 0, 0, 1, 1, 1 };
int[] dCol = { -1, 0, 1, 1, -1, 1, 0, -1 };
for (int i = 0; i < 8; i++)
{
    int newRow = row + dRow[i];
    int newCol = col + dCol[i];
    if (newRow >= 0 && newRow < input.Count && newCol >= 0 && newCol < input[newRow].Count)
    {
        // safe to access input[newRow][newCol]
    }
}
```

The bounds check uses `newRow < input.Count` (number of rows) and `newCol < input[newRow].Count` (length of that specific row) — this handles jagged arrays correctly.

### 2. `do-while` Loop for Iterative Algorithms

A `do-while` runs the body **at least once** before checking the condition:

```csharp
int canBeAccessed;
int sum = 0;
do
{
    canBeAccessed = Part1(input, debugMode);
    sum += canBeAccessed;
    // Remove the rolls we just counted, so next pass sees updated grid
    input = input.Select(line => line.Select(c => c == 'X' ? '.' : c).ToList()).ToList();
} while (canBeAccessed > 0);
```

Compare with a regular `while` loop, which checks the condition *first* — you'd need a fake initial value like `canBeAccessed = 1` just to enter. `do-while` expresses the intent more clearly: "always do one pass, then repeat if we found anything."

### 3. Calling Instance Methods from the Same Class

```csharp
// Unnecessary — creates a new object for no reason:
Day4 day4 = new Day4();
canBeAccessed = day4.Part1(input, debugMode);

// Correct — call directly from within the same instance:
canBeAccessed = Part1(input, debugMode);
```

When you're already inside an instance method of `Day4`, `this` is implicit. You can call other instance methods directly — no `new`, no variable needed. This avoids creating a fresh object that would have no shared state with the current one.

### 4. Ternary Operator (`? :`)

```csharp
condition ? value_if_true : value_if_false
```

Example from today, inside a LINQ `Select` lambda:
```csharp
c == 'X' ? '.' : c   // if char is 'X', replace with '.'; otherwise keep it
```

A compact alternative to `if-else` when assigning a value. Works especially well inline within LINQ expressions where a full `if-else` block can't fit.

### 5. String ↔ `List<char>` Conversion

A `string` is already an `IEnumerable<char>`, so LINQ methods work directly on it:

```csharp
// string → List<char> (one row):
List<List<char>> input = rawInput.Select(line => line.ToList()).ToList();

// List<char> → string (for printing):
Console.WriteLine(string.Join("", line));
```

`string.Join("", chars)` reassembles a char collection back into a single string with no separator.

### 6. Modifying a 2D Grid In-Place vs Creating a New Copy

In Part 1, the grid is modified **in-place** during a single pass (`input[row][col] = 'X'`). This affects later cells in the same pass, so the neighbor count for position (row+1, col) includes any already-marked `X` from above.

In Part 2, after each complete pass, a **new copy** of the grid is created with all `X` replaced by `.`:
```csharp
input = input.Select(line => line.Select(c => c == 'X' ? '.' : c).ToList()).ToList();
```

This distinction is important: in-place mutation during a pass vs. batch replacement after a pass are different design choices with different correctness implications.

## Key Takeaways

1. **Test with the small example first.** The example grid confirmed 13 for Part 1 and 43 for Part 2 before running the full puzzle input.
2. **Bounds-check every grid access.** A single out-of-bounds index crashes the program — `(newRow, newCol)` must satisfy both `>= 0` and `< Count` independently.
3. **`do-while` is the right tool for "execute at least once, then repeat conditionally."** Don't hack a `while` loop with a dummy initial value.
4. **Remove dead code and unused parameters before finishing.** Commented-out old approaches, leftover TODOs, and unused method parameters clutter the code and confuse future readers (including yourself).
5. **Instance methods on the same class don't need `new`.** Call them directly — `this` is implicit.
