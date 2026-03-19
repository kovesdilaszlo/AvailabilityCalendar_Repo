# Scheduling Model

## Problem

Finding common free time intervals among multiple users.

## Mathematical Model

CommonFree(U) = ⋂ Free(u)

## Implementation Steps

1. Collect busy intervals
2. Merge overlapping intervals
3. Compute gaps
4. Apply minimum duration filter

## Complexity

O(n log n)

## Slot Generation

Free intervals can be split into smaller meeting slots.

## 1. Problem Model

The scheduling problem can be described as a constraint-oriented time coordination problem.

### Goal
Find valid time intervals that satisfy all selected users simultaneously.

### Constraints
- users may already be busy because of existing events
- all returned results must remain inside the selected search range
- all returned results must satisfy the requested minimum duration
- the current user must be included in the selected user set

The implemented solution does not use a generic external CSP solver. Instead, it solves the practical problem using deterministic interval processing.

---

## 2. Mathematical Model

Let:

- `T` = searched time range
- `Busy(u)` = busy intervals of user `u`
- `Free(u)` = `T - Busy(u)`

For multiple users, the common free time can be conceptualized as:

```text
CommonFree(U) = ⋂ Free(u)
```

In implementation terms, this is computed indirectly:

1. gather busy intervals of all selected users
2. merge overlapping/touching busy intervals
3. compute remaining free gaps inside `T`

Then apply the duration filter:

```text
Result = { interval ∈ CommonFree(U) | Duration(interval) >= minimumDuration }
```

Optional slot generation then transforms the result into smaller equally sized candidate meeting intervals:

```text
Slots = Split(Result, slotLength)
```

with the additional rule that no produced slot may violate the configured minimum duration.

This model corresponds to the actual `AvailabilityService` implementation plus controller-level shared-slot post-processing.

---

## 3. Complexity

The dominant work of the scheduling algorithm comes from:

- collecting intervals
- sorting intervals
- merging overlaps
- scanning merged intervals to compute gaps

For `n` busy intervals, the dominant time complexity is approximately:

```text
O(n log n)
```

This is primarily due to sorting.

The optional slot splitting step is linear in the number of produced slots and is therefore smaller-order compared to the main merge-and-gap computation in normal usage.

### Storage considerations
- events are stored in relational tables
- event-user membership is represented through a join table
- indices exist on frequently filtered fields such as start date, end date, and creator ID

This is sufficient for the intended educational and medium-scale practical scenarios.

---

