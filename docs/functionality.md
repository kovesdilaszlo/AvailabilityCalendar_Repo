## 1. Core Functionality

### 1.1 Personal Mode

In personal mode, the system behaves as a personal calendar application.

**Main features**
- display own events
- create events
- update events
- delete events
- validate date/time input
- show the current period in the selected view

### 1.2 Shared Availability Mode

In shared mode, the system behaves as a coordination tool.

**Main features**
- resolve selected users
- calculate the common free intervals
- apply the minimum duration filter
- optionally split free intervals into meeting-sized slots
- display only valid shared free time
- create common events directly from a shared free slot

### 1.3 Privacy Principle

The system intentionally does not expose other users’ event details in shared mode. It displays only whether time is free and which intervals are suitable.

### 1.4 Event Authorization

Update and delete operations are restricted to the creator of the event.

### 1.5 Input Validation

Validation exists at multiple levels:

- view model validation for UI form input
- service-level validation for empty titles
- domain-level validation for invalid time ranges
- controller-level validation for end time not being earlier than start time

### 1.6 Calendar Range Resolution

The application computes different time ranges depending on the selected view:

- **Day** → one calendar day
- **Week** → Monday-based 7-day interval
- **Month** → first day of month until next month

These ranges are used both for event retrieval and free-time calculation.

### 1.7 Cell Generation

The system generates UI cells based on the selected view:

- **Day** → 1 cell
- **Week** → 7 cells
- **Month** → 42 cells

The month view uses a fixed 6×7 grid.

### 1.8 Month Grid Rationale

The month view contains 42 cells because:

- a month may begin late in a week
- a month may require six visual rows
- a fixed 6×7 grid keeps the layout stable

### 1.9 Month View Interval Splitting

In month view, free intervals that span across day boundaries are visually split into day-specific textual fragments.

Example:
- shared free interval: `2026-03-18 18:00 -> 2026-03-19 14:00`

Displayed as:
- `03.18` → `18:00-23:59`
- `03.19` → `00:00-14:00`

This prevents misleading rendering where a multi-day interval would otherwise appear only on its start date.