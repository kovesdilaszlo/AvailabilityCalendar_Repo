# Overview

AvailabilityCalendar is an ASP.NET Core application designed for personal event management and multi-user time coordination.

## Modes

- Personal Mode → shows the current user's events
- Shared Availability Mode → shows only common free time intervals

## Features

- Minimum duration filtering
- Slot generation for scheduling
- Privacy-oriented design

## Core Interaction Model

- One selected user → Personal Mode
- Multiple users → Shared Availability Mode

## Selection Rules

- The current user is always included
- Duplicate users are removed
- Selection can never be empty



## 1. Overview

AvailabilityCalendar is an ASP.NET Core application designed for personal event management and multi-user time coordination.

The system operates in two distinct modes:

- **Personal Mode** – displays the current user's own events
- **Shared Availability Mode** – displays only the common free time intervals of multiple selected users

The system also supports a **minimum duration filter**, allowing the user to request only those shared free intervals that are long enough for practical use. For example, if the user is interested only in one-hour meeting slots, shorter intervals are excluded from the result.

In the refined version of the system, shared mode also supports an optional **slot generation parameter**. Instead of hiding long shared free intervals, the application can split them into smaller, practically bookable meeting slots of a user-defined length. This makes the coordination view more useful for actual meeting creation.

From an architectural perspective, the project is structured into the following main layers:

- **Web** – controllers, Razor views, page-level UI models, authentication entry points
- **Application** – use-case orchestration, commands, DTOs, service interfaces and service implementations
- **Domain** – core entities, value objects, and enums
- **Infrastructure** – Entity Framework Core persistence, repositories, database schema and seed data
- **Tests** – automated verification of business and application rules

The application uses ASP.NET Core MVC, ASP.NET Core Identity, and Entity Framework Core with SQL Server LocalDB configuration.

---

## 2. Core Interaction Model

### 2.1 Default Behavior

When the user opens the application:

- only the currently logged-in user is selected
- the application displays the current user's own events
- the current mode is **Personal Mode**

This is the default interaction pattern and represents normal personal calendar usage.

### 2.2 Multi-user Selection

When the user selects one or more additional users:

- the application switches from **Personal Mode** to **Shared Availability Mode**
- event details are no longer displayed
- only the common free intervals of the selected users are shown

This design enforces a privacy-oriented coordination model. The user does not see the contents of other users’ events. Instead, the interface is focused on the scheduling problem itself: finding valid shared time slots.

### 2.3 Returning to Personal Mode

If the selection is reduced back to the current user only:

- the application switches back to **Personal Mode**
- the user's own events are displayed again

This creates a simple and deterministic model:

- one selected user → personal calendar
- multiple selected users → shared free-time coordination

### 2.4 Selection Constraints

The system enforces the following selection rules:

- the logged-in user is always part of the selection
- the selection cannot become empty
- duplicate user identifiers are removed
- invalid edge cases are normalized before the actual mode logic runs

This guarantees that:

- mode resolution always has a valid input
- controller logic remains simple
- the application never has to handle “no selected user” states

### 2.5 Visible Selection Behavior

In the refined UI, the current user is **not displayed** in the selectable user chip list. This is an intentional interaction decision:

- the active user is always implicitly included
- only additional selectable users are shown
- the visible selection area stays cleaner and easier to understand

### 2.6 Mode Determination Logic

The mode is determined using the normalized selected user list:

```text
if selectedUsers.Count == 1 → Personal Mode
else → Shared Availability Mode
```

Because the current user is always present, the system never has to handle selections that exclude the active user.

---