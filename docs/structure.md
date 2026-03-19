## 1. Project Structure and Class Hierarchy

### 1.1 Logical Project Structure

AvailabilityCalendar
├── Web
│   ├── Controllers
│   ├── Extensions
│   ├── ViewModels
│   ├── Views
│   └── wwwroot
├── Application
│   ├── Commands
│   ├── DTOs
│   ├── Interfaces
│   └── Services
├── Domain
│   ├── Entities
│   ├── Enums
│   └── ValueObjects
├── Infrastructure
│   ├── Identity
│   ├── Persistence
│   ├── Repositories
│   └── Migrations
└── Tests

### 1.2 Inheritance and Interface Hierarchy


IdentityUser<Guid>
└── ApplicationUser

IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
└── AvailabilityCalendarDbContext

IEventRepository
└── EventRepository

IUserRepository
└── UserRepository

IEventService
└── EventService

IAvailabilityService
└── AvailabilityService

Controller
├── CalendarController
├── HomeController
└── AccountController

### 1.3 Dependency Flow

Web Controllers
    ↓
Application Services / Interfaces
    ↓
Infrastructure Repositories
    ↓
EF Core DbContext
    ↓
SQL Database

### 1.4 Relationship Overview

User 1 ─── * CreatedEvents
User 1 ─── * EventParticipants
Event 1 ─── * EventParticipants
EventParticipant * ─── 1 User
EventParticipant * ─── 1 Event

## 2. Explicit System Hierarchy and Execution Flow

### 2.1 End-to-End Request Flow

```
HTTP Request (Browser)
    ↓
CalendarController (Web Layer)
    ↓
IEventService / IAvailabilityService (Application Layer)
    ↓
EventService / AvailabilityService (Implementation)
    ↓
IEventRepository / IUserRepository
    ↓
EventRepository / UserRepository (Infrastructure)
    ↓
AvailabilityCalendarDbContext (EF Core)
    ↓
SQL Database
```

### 2.2 UI → ViewModel → Data Flow

```
Index.cshtml
    ↓
CalendarPageViewModel
    ├── Cells
    ├── Events
    ├── FreeIntervals
    ├── Blocks
    ├── SelectedUserIds
    ├── AvailableUsers
    ├── MinimumDurationMinutes
    └── SlotLengthMinutes
```

### 2.3 Domain Relationships

```
User
 ├── CreatedEvents (1:N)
 └── EventParticipants (1:N)

Event
 ├── CreatedByUser (N:1)
 └── EventParticipants (1:N)

EventParticipant
 ├── User (N:1)
 └── Event (N:1)
```

### 2.4 Service Dependencies

```
CalendarController
    ↓
IEventService → EventService
IAvailabilityService → AvailabilityService

EventService
    ↓
IEventRepository → EventRepository

AvailabilityService
    ↓
IEventRepository → EventRepository
```

### 2.5 Layer Rule

```
Web → Application → Infrastructure → Database
```

Rules:
- Web never directly accesses the database
- Application depends only on interfaces
- Infrastructure provides implementations
- Domain remains independent

### 2.6 UI Rendering Hierarchy

```
Calendar (Index.cshtml)
 ├── Day View
 ├── Week View
 └── Month View
```

Each view:
- uses the same page model
- renders different structures
- uses different visualization logic
- can open the same modal interaction flow for relevant items

### 2.7 Shared Slot Creation Flow

```
Shared free interval rendered in UI
    ↓
User clicks free slot
    ↓
Free-slot modal opens
    ↓
User confirms creation and enters title
    ↓
CalendarController.CreateFromFreeSlot(...)
    ↓
EventService.CreateEventAsync(...)
    ↓
Event + EventParticipants saved
    ↓
Redirect back to current filtered calendar view
```


---

## 3. Detailed File Responsibilities + Layer Dependencies

### 3.1 Project Dependency Structure

```
AvailabilityCalendar.Web
    → references AvailabilityCalendar.Application

AvailabilityCalendar.Application
    → references AvailabilityCalendar.Domain

AvailabilityCalendar.Infrastructure
    → references AvailabilityCalendar.Application
    → references AvailabilityCalendar.Domain

AvailabilityCalendar.Domain
    → no dependencies (pure domain layer)
```

### 3.2 File-Level Responsibilities

#### Controllers

```
CalendarController.cs
    - main orchestration of calendar logic
    - handles view switching (day/week/month)
    - handles personal vs shared mode
    - handles event CRUD operations
    - handles slot-based event creation

AccountController.cs
    - authentication (login/logout)

HomeController.cs
    - entry routing
```

#### Services

```
AvailabilityService.cs
    - selection normalization
    - mode determination
    - interval merging
    - shared free time calculation

EventService.cs
    - event creation/update/delete
    - authorization enforcement
```

#### Repositories

```
EventRepository.cs
    - event persistence
    - querying by users and time range

UserRepository.cs
    - user retrieval
```

#### Domain

```
Event.cs
    - core event entity logic

User.cs
    - domain user

EventParticipant.cs
    - many-to-many relation

TimeInterval.cs
    - interval math
```

---

## 4. UI ↔ Backend Interaction Flow

### 4.1 Selection Change Flow

```
User changes checkbox (chip)
    ↓
Form auto-submit (JS)
    ↓
CalendarController.Index
    ↓
AvailabilityService.NormalizeSelection
    ↓
AvailabilityService.DetermineViewMode
    ↓
Render updated view
```

### 4.2 Free Slot Creation Flow

```
User clicks free slot
    ↓
Modal opens
    ↓
User confirms
    ↓
POST CreateFromFreeSlot
    ↓
EventService.CreateEventAsync
    ↓
Database save
    ↓
Redirect to same view
```

### 4.3 Event Update Flow

```
User clicks event
    ↓
Modal opens
    ↓
User edits data
    ↓
POST Update
    ↓
EventService.UpdateEventAsync
    ↓
Redirect back
```

### 4.4 Event Delete Flow

```
User clicks delete
    ↓
Confirmation modal
    ↓
POST Delete
    ↓
EventService.DeleteEventAsync
    ↓
Redirect back
```

---

## 5. Validation Layers

Validation is implemented across multiple layers:

### 5.1 UI Layer
- required fields
- date inputs
- client-side validation

### 5.2 Controller Layer
- start < end check
- model state validation

### 5.3 Application Layer
- title trimming
- authorization checks
- command validation

### 5.4 Domain Layer
- TimeInterval validity (End >= Start)

---

## 6. Sequence Overview (Simplified)

### 6.1 Calendar Load

```
User → Controller → Service → Repository → DB → back to UI
```

### 6.2 Shared Availability

```
Users selected
    ↓
Load events
    ↓
Merge intervals
    ↓
Compute gaps
    ↓
Filter by duration
    ↓
Render
```

### 6.3 Slot Generation

```
Free intervals
    ↓
Split into slots
    ↓
Render clickable blocks
```

## Appendix A – Full Class and File Checklist

### Domain
- `User.cs`
- `Event.cs`
- `EventParticipant.cs`
- `TimeInterval.cs`
- `ViewMode.cs`

### Application
- `CreateEventCommand.cs`
- `UpdateEventCommand.cs`
- `EventDto.cs`
- `FreeTimeIntervalDto.cs`
- `IAvailabilityService.cs`
- `IEventRepository.cs`
- `IEventService.cs`
- `IUserRepository.cs`
- `AvailabilityService.cs`
- `EventService.cs`

### Infrastructure
- `ApplicationUser.cs`
- `AvailabilityCalendarDbContext.cs`
- `DbSeeder.cs`
- `EventRepository.cs`
- `UserRepository.cs`
- `InitialFullSchema.cs`
- `InitialFullSchema.Designer.cs`
- `AvailabilityCalendarDbContextModelSnapshot.cs`
- `Program.cs`
- `appsettings.json`

### Web
- `CalendarController.cs`
- `HomeController.cs`
- `AccountController.cs`
- `CalendarPageViewModel.cs`
- `CalendarBlockViewModel.cs`
- `CalendarViewType.cs`
- `CreateEventViewModel.cs`
- `UpdateEventViewModel.cs`
- `SelectableUserViewModel.cs`
- `CalendarCellViewModel.cs`
- `UserExtensions.cs`
- `Index.cshtml`
- `Create.cshtml`
- `Login.cshtml`
- `_Layout.cshtml`
- `_ViewStart.cshtml`
- `_ViewImports.cshtml`
- `calendar.css`
- `site.css`
- `calendar.js`

