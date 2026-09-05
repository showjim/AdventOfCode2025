# Day 10: C# Learning Summary — Bitmask BFS & Branch-and-Bound for a Small ILP

## Problem Overview

**Part 1 (lights):** Each machine has indicator lights that start **off**. A button diagram like `(1,3)` means pushing that button **toggles** lights 1 and 3. The target `[.##.]` says which lights must end **on**. Find the fewest presses to reach the target.

**Part 2 (counters):** Throw away the lights. The `{3,5,4,7}` is now the target **value of each counter**; counters start at 0 and each press of button `(1,3)` **adds 1** to counters 1 and 3. Find the fewest presses so every counter lands *exactly* on its target.

The two parts are secretly different problem families:
- Part 1 = shortest path on a graph of **2ⁿ light states** (n ≤ 8 → at most 256 states) → **BFS**.
- Part 2 = a small **integer linear program (ILP)** → far too many counter-states to search (`~300¹⁰`), solved by **branch and bound** whose lower bound is the **LP relaxation**.

> Answers: Part 1 = 484, Part 2 = 19210 (sample: 7 and 33).

## The Core Algorithm Ideas

### Part 1 — a state is one `int`; a press is one XOR

Because toggling twice returns you to the start (`x ^ b ^ b == x`), pressing a button twice is pointless. So each button is a yes/no, and the whole puzzle is: pick the smallest subset of buttons whose XOR equals the target. You search over the **2ⁿ states**, not the subsets:

```
state (a bitmask) --press button b--> state ^ b
```

**BFS** explores states by "how many presses from the start": all 1-press states, then 2-press, … The first time the target pops out, that depth is the minimum. A FIFO **queue** is what makes the "in order of depth" guarantee true.

### Part 2 — model it as an ILP, then branch & bound

Let `x_j` = how many times you press button `j`. For every counter `c`:

```
sum of x_j over buttons j that touch c  =  target value of c
```

minimize `sum_j x_j`. That is an integer linear program with ≤ 13 variables and ≤ 10 equations.

Search strategy (branch & bound):
1. **Relax to real numbers** (`x_j` may be fractional) and solve the LP. That value is a **lower bound** — no integer answer can beat it. If the LP solution is already all-integer, it *is* the optimum.
2. If a variable is fractional (say `x₅ = 7.3`), split into two children: `x₅ ≤ 7` or `x₅ ≥ 8`. Every integer solution falls in one child.
3. Recurse, and **prune** any child whose lower bound can no longer beat the best integer answer found so far.

The LP itself is solved exactly by **enumerating basic solutions**: with `k ≤ 10` equations, an optimum lives at a "choose 10 buttons, solve a 10×10 system" corner — only ~C(13,10)=286 corners, so brute force is fast.

## C# Concepts Learned

### 1. Bit masks and the three operators you actually use

A bitmask is just an `int` used as a row of switches. The three operations you need:

```csharp
int targetMask = 0;
targetMask |= 1 << i;          // set bit i  (light i is on)
bool isOn = (state & mask) != 0;   // test a bit
int next = state ^ b;          // toggle exactly b's bits
```

`1 << i` shifts a `1` into position `i`; `|=` turns a bit on; `^` flips bits. For ≤ 10 items this is far cheaper and simpler than an array of bools, and you can use the whole `int` as a `HashSet`-free "set of positions".

*Contrast with Python:* the operators are identical (`<<`, `|`, `&`, `^`), but Python ints are arbitrary precision, so you rarely think about the fixed width — in C# a mask of 31+ bits stops fitting in `int` and you'd reach for `long`.

### 2. XOR cancels: `x ^ b ^ b == x`

Because toggling the same bit twice restores it, XOR is its own inverse. That one fact is what collapsed Part 1 from "press buttons in sequences" into "choose a subset": any even number of presses of a button is equivalent to zero.

### 3. `Queue<T>` and why FIFO = shortest path

```csharp
var queue = new Queue<int>();
queue.Enqueue(0);        // join the back of the line
int state = queue.Dequeue();  // leave from the front
```

BFS relies on *first-in-first-out*: all 1-press states are enqueued before any 2-press state, so they are processed first. If you used a `Stack<T>` (LIFO), you'd get depth-first order and *lose* the "first sighting is the minimum" guarantee.

### 4. Verbatim strings `@"..."` + `Regex` for parsing

The input line mixes `[.##.]`, `(1,3)`, and `{3,5,4,7}`. `Regex` picks each piece out in one line:

```csharp
string target = Regex.Match(line, @"\[([.#]+)\]").Groups[1].Value;   // ".###."
foreach (Match m in Regex.Matches(line, @"\(([^)]*)\)"))              // each button
    ...
```

The `@"..."` **verbatim string** makes `\` a literal character, so the regex escapes (`\[` etc.) read naturally instead of needing `\\`.

*Contrast with Python:* verbatim strings ≈ Python raw strings `r"..."`; `Regex.Match(s, p).Groups[1].Value` ≈ `re.search(p, s).group(1)`; `Regex.Matches` ≈ `re.finditer`.

### 5. Arrays are reference types — clone before you mutate

A method parameter that receives an array shares the *same* object; assigning `int[] copy = original;` copies the reference, not the data. Before a recursive call changes an array you still need afterward, make a copy:

```csharp
var rCopy = (int[])r.Clone();   // a fresh array — mutations won't touch `r`
```

*Contrast with Python:* `r.copy()` / `r[:]` do the same job. Python hides this less — you *know* lists are mutable — but in C# it's easy to forget that an `int[]` parameter is also a reference, not a value.

**A real bug this puzzle taught:** in a loop `foreach (int cc in btn) r[cc] -= r[c];`, the value `r[c]` is re-read on every iteration — and once the loop visits `cc == c` it sets `r[c] = 0`, so every *later* counter gets 0 subtracted instead of the intended amount. Save the value first:

```csharp
int amount = r[c];            // capture it ONCE
foreach (int cc in btn) r[cc] -= amount;
```

### 6. `out` parameters — a method that returns two things

The LP solver needs to hand back both an optimal *value* and the optimal *solution*. C# lets one method return via an `out` parameter:

```csharp
double V = Lp(r, usable, cap, out double[] x);   // V is the return, x comes back through `out`
```

`out` means "this method guarantees to fill this argument before returning." A clean alternative to returning a tuple everywhere.

### 7. Recursion + an `int.MaxValue` sentinel

`int.MaxValue` (2,147,483,647) makes a great "no answer yet / infinity" for a `best` variable:

```csharp
int best = int.MaxValue;
...
void Bb(...) {
    if (p + lowerBound >= best) return;   // can't improve -> prune this whole subtree
    ...
    if (candidate < best) best = candidate;
}
```

`return best;` at the top level yields `int.MaxValue` only if nothing feasible was ever found — a handy bug signal.

### 8. Floating point needs *tolerance*, never `==`

The LP is solved with `double` Gaussian elimination, where `2/3` isn't exact. Comparing the result needs an epsilon:

```csharp
if (Math.Abs(v - Math.Round(v)) > 1e-6) ...   // is v "fractional"?
if (p + Math.Ceiling(V - 1e-8) >= best) ...   // integer-safe prune of a fractional bound
```

`Math.Ceiling` on a fractional lower bound is important: a bound of `312.11` means the true integer optimum is at least `313`, so you prune against `ceil(V)`.

*Contrast with Python:* same problem, same `math.ceil` — but C# also offers `Math.Floor`, `Math.Round`, and plain cast `(int)` which *truncates toward zero*, so the three rounding behaviors are easy to mix up.

### 9. Structuring a small solver: a nested helper class

The Part-2 machinery (LP solve, basis enumeration, branch & bound) lives in its own `static class Part2Solver { ... }` with a private nested `class Search`. Keeping the search object's working state (`k`, `B`, `best`) as fields of `Search`, and the public API static, is a tidy way to isolate a self-contained algorithm inside one file.

## Key Takeaways

1. **Recognize the problem family before picking a tool.** "Toggle 0/1 bits" → graph of 2ⁿ states → BFS. "Add until you exactly hit target values" → integer linear program. Part 1 and Part 2 *look* alike and need completely different machinery.

2. **XOR is its own inverse**, which turns sequence problems into subset problems. That single identity (`x ^ b ^ b == x`) is what makes Part 1 tractable.

3. **BFS needs a FIFO queue on purpose.** The data structure choice is what guarantees "first found = fewest steps."

4. **Branch & bound is how you solve a small ILP by hand.** LP relaxation gives a hard lower bound; branching a fractional variable at `⌊·⌋`/`⌈·⌉` loses no integer solutions; pruning is what makes it fast.

5. **`double` arithmetic is not exact** — always compare with an epsilon, and `ceil` fractional lower bounds before comparing to integers.

6. **C# arrays are references.** Clone (`(int[])x.Clone()`) before recursive mutation, and don't read a value you're also mutating in the same loop.
