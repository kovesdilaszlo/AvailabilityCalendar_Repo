## 3. Domain Layer

The Domain layer contains the core business concepts and the scheduling-related rules of the system.

### 3.1 Entities

#### User

Represents a domain-level calendar user.

**Properties**
- `Guid Id`
- `string Name`
- `ICollection<EventParticipant> EventParticipants`
- `ICollection<Event> CreatedEvents`

**Role in the system**
- identifies a participant in the scheduling domain
- acts as the creator of events
- participates in events through the join entity

---

#### Event

Represents a scheduled busy interval.

**Properties**
- `Guid Id`
- `string Title`
- `DateTime Start`
- `DateTime End`
- `Guid CreatedByUserId`
- `User? CreatedByUser`
- `ICollection<EventParticipant> Participants`

**Methods**
- `void AddParticipant(Guid userId)`
- `void RemoveParticipant(Guid userId)`
- `bool HasParticipant(Guid userId)`
- `void UpdateTime(DateTime start, DateTime end)`
- `bool IsCreatedBy(Guid userId)`

**Role in the system**
- stores the title and time boundaries of a busy period
- keeps the creator information
- keeps the participant list
- enforces basic participant and time-related logic

---

#### EventParticipant

Represents the many-to-many relationship between users and events.

**Properties**
- `Guid EventId`
- `Event? Event`
- `Guid UserId`
- `User? User`

**Role in the system**
- links one event to one participant
- allows one event to have multiple users
- allows one user to participate in multiple events

---

### 3.2 Value Objects

#### TimeInterval

Represents a generic time range and is the mathematical core of the scheduling algorithm.

**Properties**
- `DateTime Start`
- `DateTime End`

**Methods**
- `bool OverlapsWith(TimeInterval other)`
- `TimeInterval MergeWith(TimeInterval other)`
- `TimeSpan Duration()`

**Role in the system**
- used to represent busy intervals
- used to represent free intervals
- used to merge overlaps
- used to measure minimum duration constraints

The constructor validates that `End` is not earlier than `Start`.

---

### 3.3 Domain Enums

#### ViewMode

Represents the business-level mode of the calendar.

**Members**
- `Personal = 0`
- `Shared = 1`

**Role in the system**
- separates own-event rendering from shared-free-time rendering

---

### 3.4 Domain Class Inventory

```text
AvailabilityCalendar.Domain
├── Entities
│   ├── User
│   ├── Event
│   └── EventParticipant
├── ValueObjects
│   └── TimeInterval
└── Enums
    └── ViewMode
```
