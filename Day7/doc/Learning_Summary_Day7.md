# Advent of Code 2025 Day 7: C# Learning Summary

## Problem Overview

**Part 1:** The input is a diagram of a "tachyon manifold" — a grid where a beam enters at `S` and moves **straight down**. Empty cells (`.`) let it pass through; a splitter (`^`) stops the beam and emits **two new beams** from its immediate left and immediate right. The twist that makes this more than counting splitters: when two beams pour into the **same cell**, they **merge** — that cell only has one beam. The task is to count how many times the beam is split.

**Part 2:** The "quantum" reinterpretation: now a **single particle** takes **both** paths at every splitter, but these do **not merge**. Each distinct sequence of left/right choices is its own *timeline* forever, even if two timelines pass through the same cell. The task is to count the total number of distinct timelines that reach the bottom.

## The Central Conceptual Distinction

Same grid, two opposite assumptions about what happens when paths converge at the same cell:

| | Part 1 (classical) | Part 2 (quantum) |
| --- | --- | --- |
| Paths that meet at a cell | **Merge** into one beam | Stay **separate timelines** |
| Thing being counted | Split events (a cell visited once = one count) | Distinct paths (a cell visited N ways = N timelines) |
| Right data structure | A **set** of visited cells (dedup) | A **recurrence** over the grid (no dedup) |

Everything else in both solutions follows from this one difference.

## C# Concepts Learned

### 1. Tuples with named elements — `(int row, int col)`

A position was first represented as `List<int> { row, col }`, read back with `pos[0]` and `pos[1]`. A `List<int>` says nothing about what its elements *mean*. A **named tuple** fixes that — the names live in the type and travel with every value:

```csharp
List<(int row, int col)> posToCheck = new();   // list of named tuples
posToCheck.Add((1, 7));                         // names borrowed from the list's type

foreach ((int row, int col) pos in posToCheck)  // destructure in the loop
{
    Console.WriteLine(pos.row);                 // self-documenting, unlike pos[0]
}
```

*Contrast with Python:* Python tuples are unnamed (`(1, 7)`, accessed by index or unpacking). C# lets you **name** the parts in the type itself, so `pos.row` works everywhere with no extra ceremony. Two tuples with equal parts are **equal by value** — `(1, 7) == (1, 7)` is `true` — which matters for the next two sections.

### 2. `HashSet<T>` — Python's `set`, with a clever `Add`

Part 1 needed "is this cell already queued?" Deduping with `.Any(p => p.row == r && p.col == c)` **scans the whole list** every time — O(n²) overall. A `HashSet<T>` gives O(1) lookup, and its `Add` returns a `bool` that does "check + insert" in one call:

```csharp
HashSet<(int row, int col)> queued = new();
bool wasNew = queued.Add((1, 7));   // true  — newly added
bool again  = queued.Add((1, 7));   // false — already present

// The whole "if not already there, then add" pattern collapses:
if (queued.Add((row + 1, col - 1)))   // returns true ONLY if it was new
{
    posToCheck.Add((row + 1, col - 1));
}
```

`Add` returning a `bool` is the idiom that replaces Python's `if x not in s: s.add(x)`. Tuples work as elements here *because* they compare by value — a `List<int>` would not (two distinct list objects are never equal, even with identical contents).

### 3. `foreach` over a list you're modifying — and the `.ToList()` snapshot

Adding to a `List` *while `foreach`-ing over it* throws `InvalidOperationException: Collection was modified`. A `foreach` walks the list through an **enumerator** that tracks a version number; any `Add`/`Remove` bumps the version, and the enumerator refuses to continue. The fix is to **iterate a snapshot**:

```csharp
foreach ((int row, int col) pos in posToCheck.ToList())   // .ToList() = a fresh copy
{
    posToCheck.Add((row + 1, col));   // safe — modifying the original, not the copy
}
```

`.ToList()` copies the list as it is *right now*; newly-added positions aren't in the snapshot and so aren't processed this round — which, for a row-by-row BFS, is exactly the desired behavior.

### 4. Recursion + memoization — turning `2^N` into "once per cell"

Part 2's count of timelines follows a clean recurrence:

```
ways(bottom edge)             = 1                       // a completed journey
ways(empty cell)              = ways(below it)          // beam just falls
ways(splitter)                = ways(left child) + ways(right child)
```

The naive recursion re-computes the same cell *every time a path reaches it* — and many paths converge, so the call count explodes exponentially. **Memoization** (记忆化) caches `ways(cell)` so **each cell is fully computed exactly once**, then reused:

```csharp
static Dictionary<(int, int), BigInteger> cache = new();

static BigInteger Ways(int row, int col, List<List<char>> input)
{
    if (row == input.Count) return 1;                       // base case: fell off the bottom
    if (cache.ContainsKey((row, col)))                       // (a) already computed?
        return cache[(row, col)];

    BigInteger totalWays;
    if (input[row][col] == '^')
    {
        totalWays = 0;                                       // each child added independently
        if (col - 1 >= 0)          totalWays += Ways(row + 1, col - 1, input);
        if (col + 1 < input[row].Count) totalWays += Ways(row + 1, col + 1, input);
    }
    else
    {
        totalWays = Ways(row + 1, col, input);               // '.' or 'S' — just fall
    }

    cache[(row, col)] = totalWays;                           // (b) remember it
    return totalWays;
}
```

Two things to internalize:

- **Recursion is the *approach*; memoization is the *technique* that makes it fast.** Pure recursion got us into the blowup; the cache got us out. The guarantee: *each distinct input is computed once.*
- **The cache must be shared across every call** (a field, not a local) — a per-call local cache is useless because it's discarded after each return.

### 5. `Dictionary<TKey, TValue>` — Python's `dict`, the memoization workhorse

The cache is a `Dictionary<(int, int), BigInteger>`: the **key** is the cell, the **value** is the remembered `ways` result. Idioms used:

```csharp
cache.ContainsKey((row, col))   // "key in dict"  →  bool
cache[(row, col)]               // dict[key]      →  value (throws if absent)
cache[(row, col)] = totalWays;  // dict[key] = v  →  insert/overwrite
```

`TryGetValue` is the one-shot "get if present" variant, but `ContainsKey` + indexer reads more like the Python code it mirrors.

### 6. `StackOverflowException` — the one exception you *cannot* catch

An early recursion bug (`Ways` called itself without ever moving `row` forward) ran forever and crashed with `StackOverflowException`. The Python-vs-C# difference here is sharp:

- **Python:** runaway recursion raises `RecursionError`, which you **can** `except` and recover from.
- **C#:** `StackOverflowException` **terminates the process** — no `catch` will save you, no matter what you write. It's treated as unrecoverable corruption.

The lesson: **a stack overflow is not a runtime hazard to handle; it's a bug in your recursion.** The fix is to find why the function never reaches its base case, never to wrap it in `try/catch`.

### 7. `try/catch` — Python's `try/except`

The keyword translation, for the record:

```python
# Python
try:
    risky()
except ValueError as ex:
    print(ex)
```
```csharp
// C#
try
{
    Risky();
}
catch (Exception ex)        // "except E as ex"  →  "catch (E ex)"
{
    Console.WriteLine(ex.Message);
}
```

Python's `except` is C#'s `catch`; multiple `catch` blocks sort by type, and `finally` works identically. **But** — as above — `StackOverflowException` is specifically *not* recoverable in C#, so the existence of `catch` doesn't mean every exception is catchable.

### 8. `int` overflow (silent wrap) → `BigInteger`

Part 2 was reported "too low" with a *correct* algorithm. The cause: ~70 splitter rows means the timeline count is astronomically large, far beyond C#'s 32-bit `int`. And `int` overflow in C# **does not raise** — it silently **wraps around** (an odometer rolling past its max), turning a giant positive number into a small or negative one.

| Type           | Max value      | Behavior at overflow        |
| -------------- | -------------- | --------------------------- |
| `int`        | ~2.1 × 10⁹   | **Silent wrap** (wrong answer, no error) |
| `long`       | ~9.2 × 10¹⁸ | Silent wrap (but much later) |
| `BigInteger` | unlimited      | **Never overflows** — grows as needed |

`System.Numerics.BigInteger` is the closest thing to a Python `int`: arbitrary precision, supports `+` / `*` with the usual operators. With ~70 doublings, `long` was also risky, so `BigInteger` was the safe choice. The "too low" answer was a **negative number** — the tell-tale sign of a wrapped `int`.

*Contrast with Python:* Python `int` is arbitrary-precision by default — you never think about overflow. In C#, picking the right integer type is a deliberate, checked decision every time you multiply or sum large quantities.

## Key Takeaways

1. **Understand the merge rule before writing code.** Part 1 and Part 2 use the *same grid* but reach opposite conclusions because of one assumption: do paths that meet at a cell merge, or stay distinct? Nail that, and the algorithm choice (set vs. recurrence) falls out automatically.
2. **Recursion without memoization is exponential; memoization makes it polynomial.** When a recursive function revisits the same inputs, cache them in a `Dictionary`. The payoff here was `2^70` calls → roughly one call per cell. Recursion is the approach; memoization is the technique that makes it viable.
3. **A stack overflow is a recursion bug, not an exception to handle.** `StackOverflowException` can't be caught. If you hit it, find the missing or unreachable base case — don't reach for `try/catch`.
4. **`int` overflows silently.** Any time a number can get large (products, counts of paths, anything exponential), reach for `long` or `BigInteger`. A *negative* or suspiciously small answer for a "big count" puzzle is the signature of an `int` wrap.
5. **Prefer `HashSet.Add`'s bool return and named tuples over `List` + index.** `if (set.Add(x))` replaces `if (!list.Any(...)) list.Add(x)`; `pos.row` replaces `pos[0]`. Both make the intent obvious and the code faster.
6. **Don't modify a `List` while `foreach`-ing it.** Either iterate a `.ToList()` snapshot, or accumulate into a separate list and merge after the loop.
7. **Map new C# features to Python:** Python `set` → `HashSet<T>` (with bool-returning `Add`), Python `dict` → `Dictionary<K,V>`, Python `int` → `BigInteger`, Python `try/except` → `try/catch`, Python `tuple` → C# **named** tuple `(int row, int col)`.
