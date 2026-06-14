# Day 4 — C# Concepts Learned

## 1. 2D Grid Neighbor Checking with Bounds Handling

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

**Key takeaway:** Always bounds-check both row *and* col before accessing grid neighbors. Use `>= 0` and `< Count` on the appropriate dimension.

---

## 2. `do-while` Loop vs `while` Loop

```csharp
do
{
    canBeAccessed = Part1(input, debugMode);
    sum += canBeAccessed;
    // modify grid for next pass...
} while (canBeAccessed > 0);
```

**Key takeaway:** A `do-while` runs the body **at least once** before checking the condition. Perfect for iterative algorithms where you always need one pass, then repeat only if something changed.

A regular `while` checks the condition *first* — you'd need to initialize `canBeAccessed = 1` as a hack just to enter the loop. `do-while` is the cleaner choice here.

---

## 3. Calling Instance Methods from the Same Class

```csharp
// Before (unnecessary):
Day4 day4 = new Day4();
canBeAccessed = day4.Part1(input, debugMode);

// After (direct):
canBeAccessed = Part1(input, debugMode);
```

**Key takeaway:** When you're already inside an instance method of `Day4`, you can call other instance methods directly (no `new`, no variable). The `this` is implicit in C#.

---

## 4. C# Ternary Operator (`? :`)

```csharp
condition ? value_if_true : value_if_false
```

Example from today:
```csharp
c == 'X' ? '.' : c
```

**Key takeaway:** A compact alternative to `if-else` when assigning a value. Place it inline within expressions like LINQ `Select()` lambdas.

---

## 5. String → `List<char>` (and back)

```csharp
// string to List<char>:
List<List<char>> input = rawInput.Select(line => line.ToList()).ToList();

// List<char> to string (for printing):
Console.WriteLine(string.Join("", line));

// Modify chars via LINQ:
input = input.Select(line => line.Select(c => c == 'X' ? '.' : c).ToList()).ToList();
```

**Key takeaway:** A `string` is an `IEnumerable<char>`, so `.ToList()` works directly on it. `string.Join("", chars)` reassembles a char collection back into a string.

---

## 6. Clean Code Principles

- **Remove dead code:** Commented-out variables, TODOs that are already done, old approach comments — delete them once the code works.
- **Remove unused parameters:** If a method parameter is no longer used in the body, strip it from the signature and all call sites.
- **Single Responsibility:** `CheckAdjacentPositions` does one thing (count neighbors). `Part1` does one thing (count accessible rolls in one pass). `Part2` orchestrates repeated passes. Each method is short and focused.
