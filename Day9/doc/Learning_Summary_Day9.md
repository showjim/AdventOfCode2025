# Day 9: C# Learning Summary — Point-in-Polygon & the Even-Odd Rule

## Problem Overview

**Part 1:** Red tiles are scattered on a grid (coordinates given as `x,y` per line). Pick any **two red tiles as opposite corners** of an axis-aligned rectangle. Find the **largest area** of any such rectangle. Area = `(|x1-x2| + 1) * (|y1-y2| + 1)` — the `+1` matters because the corner tiles themselves count.

**Part 2:** The red tiles actually form a **closed loop**: each red tile is connected to the *next one in the list* (and the last wraps to the first) by a straight line of green tiles. All tiles **inside** the loop are green too. Now a rectangle is only valid if *every* tile in it is red or green. Find the largest valid rectangle.

The real challenge in Part 2 is **determining "inside the loop"** for arbitrary tiles on a grid that can be ~100,000 × 100,000 — too big to build as an array. The solution uses the **even-odd rule** (a point-in-polygon technique) plus a geometric shortcut so we never iterate a rectangle's interior.

## The Core Algorithm Idea: The Even-Odd Rule

To test if a tile is *inside* the loop:

> Cast an imaginary **horizontal ray** from the tile's center to the right. Count how many **vertical loop segments** it crosses. If the count is **odd**, the tile is inside; if **even**, outside.

Why odd = inside: every time the ray crosses the loop, it flips between "outside" and "inside". Starting outside (left of everything) and crossing an odd number of boundaries lands you inside.

Why only vertical segments count: a *horizontal* ray can only intersect *vertical* segments.

**The rule is direction-independent.** If you cast the ray to the left instead, the answer is the same: a full line through the tile crosses a closed loop an even number of times in total, so the right half (`k`) and left half (`even - k`) always share the same parity.

### The three-part rectangle check

A rectangle can contain up to ~10⁹ tiles, so we **never iterate its interior**. Instead, for non-thin rectangles:

1. **All four corners** must be red or green.
2. **No loop segment may pass through the strict interior** of the rectangle — otherwise the loop would split the rectangle into inside + outside parts.
3. **One tile strictly inside** must be green — this catches the sneaky case of a rectangle sitting entirely in an outside "bay" of the loop where all 4 corners are still *on* the loop.

Thin rectangles (width < 3 or height < 3) have no interior tile to reason about, so we check every tile on the line directly.

## C# Concepts Learned

### 1. Named value tuples — `(long x, long y)`

A tuple groups several values into one object, and C# lets you **name the fields**:

```csharp
List<(long x, long y)> input = ...;   // each element is a point with named coordinates
long dx = input[i].x - input[j].x;    // readable field access, no index guessing
```

Much clearer than `input[i][0]` / `input[i][1]`. The names travel with the type — IntelliSense shows them.

*Contrast with Python:* Python's tuples are `(x, y)` with positional access `t[0]`. C#'s named tuples give you `.x` / `.y` directly at compile time.

### 2. Local functions — functions declared *inside* a method

`OnEdge`, `Inside`, and `RedOrGreen` were declared **inside** `Part2`, so they can "see" the `input` parameter without it being passed every time:

```csharp
void Part2(List<(long x, long y)> input)
{
    bool OnEdge(long x, long y) { ... }      // closes over `input`
    bool Inside(long x, long y) { ... }
    bool RedOrGreen(long x, long y) => red.Contains((x, y)) || OnEdge(x, y) || Inside(x, y);
}
```

This keeps helper functions near their use, avoids threading `input` through every call, and reads top-down like the algorithm.

*Contrast with Python:* Python has nested functions and closures too — the idea is the same. The difference is C# requires you to declare the helper *before* use and give types on parameters/returns.

### 3. `HashSet<T>` for O(1) membership tests

To answer "is this tile one of the red tiles?" fast, build a set once:

```csharp
var red = new HashSet<(long x, long y)>(input);
bool isRed = red.Contains((x, y));   // O(1) average — no scanning the list
```

*Contrast with Python:* `set(input)` and `(x, y) in s`. Nearly identical — but note C# tuples in a `HashSet` need the same element types on both sides.

### 4. `long` vs `int` — the overflow trap (again!)

Both answers exceed `int.MaxValue` (2,147,483,647):

```csharp
long maxArea = 0;                       // Part 1 answer: 4,763,040,296
long area = (x2 - x1 + 1) * (y2 - y1 + 1);   // Part 2 answer: 1,396,494,456
```

Had `int` been used, the multiplication would **silently wrap** (overflow) and produce a wrong number with no error. C# *doesn't* throw on integer overflow by default in `unchecked` context — it just wraps. Rule of thumb: **if a product of coordinates can exceed ~2.1 billion, use `long`.**

*Contrast with Python:* Python ints are arbitrary precision — overflow never happens, so this bug class is invisible until you switch to C#.

### 5. `Math.Min` / `Math.Max` for symmetric ranges

Instead of ordering endpoints by hand:

```csharp
long x1 = Math.Min(input[i].x, input[j].x);   // left edge, regardless of order
long y2 = Math.Max(input[i].y, input[j].y);   // top edge
```

Removes a whole class of "which point is smaller" `if` statements.

### 6. Reading input with LINQ chaining

```csharp
List<(long x, long y)> input = rawInput
    .Select(line => line.Split(",").Select(long.Parse).ToList())
    .Select(coords => (coords[0], coords[1]))
    .ToList();
```

Each line is split on commas, parsed to longs, then the two values are packed into a named tuple.

## Key Takeaways

1. **The even-odd rule answers "is this point inside a closed shape?" in O(segments).** Cast a ray, count crossings of the boundary, odd = inside. No grid needed — works for arbitrarily large coordinates.

2. **Think of "inside" as a flip-flop.** Crossing the loop boundary toggles inside/outside. Starting outside, an odd number of crossings ends you inside. This intuition makes the rule easy to re-derive instead of memorize.

3. **Named tuples + local functions make geometry code readable.** `(long x, long y)` carries meaning; local helpers capture context without parameter-passing noise. These are two of C#'s most pleasant ergonomics.

4. **`HashSet<T>` is the go-to for "is this value in the set?"** O(1) lookups instead of scanning a list — crucial when a check runs millions of times.

5. **Use `long` for coordinate products.** Coordinates up to ~100,000 multiply to ~10¹⁰, far beyond `int`. C# silently wraps on overflow, so the bug shows up as a wrong answer, not an error.

6. **Check corners + boundary, not every cell.** The rectangle validity test avoided iterating up to ~10⁹ tiles by checking four corners, ensuring no loop segment cuts through, and sampling one interior tile. Geometric shortcuts like this are how you scale "check a region" problems.
