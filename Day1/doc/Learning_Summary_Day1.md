# Advent of Code 2025 Day 1: C# Learning Summary

## Problem Overview

**Part 1:** Simulate a circular dial (0 to 99) starting at 50. Parse a sequence of rotations (e.g., `L68`, `R48`) and count how many times the dial lands on 0 at the end of a rotation.

**Part 2:** Same input, but now also count every time the dial passes through 0 *during* a rotation, not just at the end. The raw position may go far beyond the 0 to 99 range (e.g., `R1000` crosses 0 ten times).

## C# Concepts Learned

### 1. Program Entry Point: `Main` method

C# requires a `static void Main(string[] args)` method as the entry point. When using top level statements (C# 10+), no explicit `Main` is needed. Once you add `namespace` and `class`, you must provide a `Main` method.

### 2. Modulo Operator `%` and Negative Numbers

In C#, `-5 % 100` returns `-5` (truncates toward zero), not `95` like Python. The pattern `(n % 100 + 100) % 100` safely maps any integer into 0 to 99 range.

Example for why `(n - 99) / 100` fixes floor division: When `n = -1`, C# integer division `-1 / 100` returns `0` (truncates toward zero), but `(-1 - 99) / 100` = `-100 / 100` = `-1`, which is the correct floor result.

### 3. Compiled Output Directory vs Source Directory

Unlike Python, C# compiles code into `bin/Debug/net10.0/` and runs from there. Relative paths in `File.ReadAllLines` resolve relative to that output directory, not the project root. Solutions:

| Approach | Example |
|---|---|
| Use `AppDomain.CurrentDomain.BaseDirectory` with `..` | `Path.Combine(baseDir, "..", "..", "..", "doc", "input.txt")` |
| `Path.Combine` for cross platform paths | Never manually join with `/` or `\` |

### 4. `Path.Combine`

C# method that joins path segments with the correct OS separator. Prefer this over string concatenation.

### 5. `char` vs `string` Comparison

| Type | Syntax | When to Use |
|---|---|---|
| `char` | `'R'` (single quotes) | Comparing a single character, no heap allocation |
| `string` | `"R"` (double quotes) | Comparing sequences of characters |

Using `char` is more precise and avoids unnecessary `.ToString()` calls.

### 6. `static` vs Instance Methods

| Feature | `static` method | Instance method |
|---|---|---|
| Belongs to | The class itself | An object instance |
| Calling | `ClassName.Method()` or just `Method()` | `new ClassName().Method()` |
| Can access instance fields | No | Yes |

`Main` must be `static`. Helper methods can be either; `static` is simpler when no instance state is needed.

### 7. `int.Parse` and `Substring`

Parsing a rotation instruction like `"L68"`:

```csharp
char direction = input[i][0];          // 'L'
int moveCount = int.Parse(input[i].Substring(1));  // 68
```

`Substring(1)` returns everything from index 1 to the end of the string.

### 8. `Math.Floor` for Floor Division

C# integer division truncates toward zero. For proper floor division (needed when counting crossings of 0), use `(int)Math.Floor(n / 100.0)`.

### 9. Counting Crossings of 0 with Raw Position

Part 2 introduced the concept of a **raw position** that is not constrained to 0 to 99. When the raw position passes through multiples of 100, the dial is pointing at 0.

Right rotation formula:
```
crossings = floor(newRaw / 100) - floor(oldRaw / 100)
```

Left rotation formula (excludes starting point, includes endpoint):
```
crossings = floor((oldRaw - 1) / 100) - floor((newRaw - 1) / 100)
```

### 10. DRY Principle (Don't Repeat Yourself)

Identical code in `Part1()` and `Part2()` for reading files and parsing input is a sign to extract shared logic into a separate method or to unify the two implementations (since Part 1 is a special case of Part 2).

## Key Takeaways

1. Always test with the small example input before running on the full puzzle input.
2. C# integer division truncates toward zero, unlike Python's floor division. Use `Math.Floor` when floor behavior is needed.
3. Variable naming matters: `rawPos` communicates intent better than `arrowPos` when tracking values outside the 0 to 99 range.
4. C# compilation creates a separation between source and output directories that affects file path resolution.
